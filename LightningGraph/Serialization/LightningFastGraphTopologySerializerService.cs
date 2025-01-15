using Google.Protobuf;
using LightningGraph.Core;

namespace LightningGraph.Serialization;

public static class LightningFastGraphTopologySerializerService
{
    public static byte[] SerializeTopology(LightningFastGraph graph)
    {
        var topology = new Topology();

        // Convert adjacency list
        foreach (var (nodeId, edges) in graph.AdjacencyList)
        {
            var edgeList = new EdgeList();
            // Convert domain Edge to protobuf Edge
            var protoEdges = edges.Select(edge => new Edge
            {
                Scid = edge.Scid,
                From = edge.From,
                To = edge.To,
                BaseMSat = edge.BaseMSat,
                ProportionalMillionths = edge.ProportionalMillionths
            });
            
            edgeList.Edges.AddRange(protoEdges);
            topology.AdjacencyList.Add(nodeId, edgeList);
        }

        // Convert reverse adjacency list
        foreach (var (nodeId, edges) in graph.ReverseAdjacencyList)
        {
            var edgeList = new EdgeList();
            var protoEdges = edges.Select(edge => new Edge
            {
                Scid = edge.Scid,
                From = edge.From,
                To = edge.To,
                BaseMSat = edge.BaseMSat,
                ProportionalMillionths = edge.ProportionalMillionths
            });
            
            edgeList.Edges.AddRange(protoEdges);
            topology.ReverseAdjacencyList.Add(nodeId, edgeList);
        }

        return topology.ToByteArray();
    }

    public static LightningFastGraph DeserializeTopology(byte[] data)
    {
        try
        {
            var topology = Topology.Parser.ParseFrom(data);
            Console.WriteLine($"Successfully parsed protobuf data");
            Console.WriteLine($"Adjacency list size: {topology.AdjacencyList.Count}");

            var graph = new LightningFastGraph();

            // Convert adjacency list
            foreach (var (nodeId, edgeList) in topology.AdjacencyList)
            {
                var edges = edgeList.Edges
                    .Select(protoEdge => new Edge
                    {
                        Scid = protoEdge.Scid,
                        From = protoEdge.From,
                        To = protoEdge.To,
                        BaseMSat = protoEdge.BaseMSat,
                        ProportionalMillionths = protoEdge.ProportionalMillionths
                    })
                    .ToList();

                graph.AdjacencyList[nodeId] = edges;
                Console.WriteLine($"Added {edges.Count} edges for node {nodeId}");
            }

            // Convert reverse adjacency list
            foreach (var (nodeId, edgeList) in topology.ReverseAdjacencyList)
            {
                var edges = edgeList.Edges
                    .Select(protoEdge => new Edge
                    {
                        Scid = protoEdge.Scid,
                        From = protoEdge.From,
                        To = protoEdge.To,
                        BaseMSat = protoEdge.BaseMSat,
                        ProportionalMillionths = protoEdge.ProportionalMillionths
                    })
                    .ToList();

                graph.ReverseAdjacencyList[nodeId] = edges;
            }

            Console.WriteLine($"Final graph has {graph.AdjacencyList.Count} nodes");
            foreach (var (nodeId, edges) in graph.AdjacencyList)
            {
                Console.WriteLine($"Node {nodeId} has {edges.Count} edges in final graph");
            }

            return graph;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during deserialization: {ex}");
            throw;
        }
    }

    private static Edge ConvertToProtobufEdge(Edge edge)
    {
        return new Edge
        {
            Scid = edge.Scid,
            From = edge.From,
            To = edge.To,
            BaseMSat = edge.BaseMSat,
            ProportionalMillionths = edge.ProportionalMillionths
        };
    }

    private static Edge ConvertFromProtobufEdge(Edge protoEdge)
    {
        return new Edge
        {
            Scid = protoEdge.Scid,
            From = protoEdge.From,
            To = protoEdge.To,
            BaseMSat = protoEdge.BaseMSat,
            ProportionalMillionths = protoEdge.ProportionalMillionths
        };
    }
}