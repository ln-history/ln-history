using LightningGraph.Core;
using LightningGraph.Serialization;

namespace LightningGraph.Tests.Faker;

public class LightningFastGraphFaker
{
    public static LightningFastGraph Create(int seed, int vertexCount, int edgeCount)
    {
        var graph = new LightningFastGraph();
        
        var vertices = Enumerable.Range(0, vertexCount)
            .Select(i => VertexFaker.Create(seed+i))
            .ToList();

        var edges = Enumerable
            .Range(0, edgeCount)
            .Select(i =>
            {
                var vertex1 = vertices[new Random(seed+i).Next(0, vertexCount)];
                var vertex2 = vertices[new Random(2*seed+i).Next(0, vertexCount)];

                while (vertex1 == vertex2)
                {
                    var j = 0;
                    vertex2 = vertices[new Random(seed+i+j).Next(0, vertexCount)];
                }
                
                return EdgeFaker.Create(seed + i, vertex1, vertex2);
            })
            .ToList();

        graph.AddEdgesBatch(edges);
        
        return graph;
    }
}