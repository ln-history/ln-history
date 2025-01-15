using LightningGraph.Core;
using Google.Protobuf;

namespace LightningGraph.Serialization;

public static class LightningFastGraphTopologySerializerService
{
    public static byte[] SerializeTopology(LightningFastGraph graph)
    {
        var graphTopology = new GraphTopology();

        foreach (var edge in graph.GetEdges())
        {
            graphTopology.Edges.Add(new Edge
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
        }

        return graphTopology.ToByteArray();
    }

    public static LightningFastGraph DeserializeTopology(byte[] data)
    {
        var graphTopology = GraphTopology.Parser.ParseFrom(data);

        var graph = new LightningFastGraph();

        foreach (var edge in graphTopology.Edges)
        {
            graph.AddEdge(new Edge
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
        }

        return graph;
    }
}