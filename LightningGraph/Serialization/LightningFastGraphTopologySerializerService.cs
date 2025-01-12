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
                Weight = new Weight
                {
                    BaseMSat = edge.Weight.BaseMSat,
                    ProportionalMillionths = edge.Weight.ProportionalMillionths
                }
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
                Weight = new Weight
                {
                    BaseMSat = edge.Weight.BaseMSat,
                    ProportionalMillionths = edge.Weight.ProportionalMillionths
                }
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
        
            foreach (var (nodeId, edgeList) in topology.AdjacencyList)
            {
                Console.WriteLine($"Node {nodeId} has {edgeList.Edges.Count} edges");
                foreach (var edge in edgeList.Edges)
                {
                    Console.WriteLine($"  Edge: {edge.From} -> {edge.To} (SCID: {edge.Scid})");
                }
            }

            var graph = new LightningFastGraph();

            foreach (var (nodeId, edgeList) in topology.AdjacencyList)
            {
                graph.AdjacencyList[nodeId] = edgeList.Edges.ToList();
                Console.WriteLine($"Added {edgeList.Edges.Count} edges for node {nodeId}");
            }

            foreach (var (nodeId, edgeList) in topology.ReverseAdjacencyList)
            {
                graph.ReverseAdjacencyList[nodeId] = edgeList.Edges.ToList();
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
            Weight = new Weight
            {
                BaseMSat = edge.Weight.BaseMSat,
                ProportionalMillionths = edge.Weight.ProportionalMillionths
            }
        };
    }

    private static Edge ConvertFromProtobufEdge(Edge protoEdge)
    {
        return new Edge
        {
            Scid = protoEdge.Scid,
            From = protoEdge.From,
            To = protoEdge.To,
            Weight = new Weight
            {
                BaseMSat = protoEdge.Weight.BaseMSat,
                ProportionalMillionths = protoEdge.Weight.ProportionalMillionths
            }
        };
    }
}