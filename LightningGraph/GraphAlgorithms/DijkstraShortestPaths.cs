using LightningGraph.Core;
using LightningGraph.Serialization;

namespace LightningGraph.GraphAlgorithms;

public class DijkstraShortestPaths
{
    public static Dictionary<string, double> Calculate(LightningFastGraph graph, string source, int? paymentSizeSats = null)
    {
        var edgeDictionary = graph.GetEdgesDictionary();
        
        var distances = new Dictionary<string, double>();
        var priorityQueue = new PriorityQueue<string, double>();

        foreach (var node in graph.GetVertices())
        {
            distances[node] = double.MaxValue;
        }

        distances[source] = 0;
        priorityQueue.Enqueue(source, 0);

        while (priorityQueue.Count > 0)
        {
            var current = priorityQueue.Dequeue();

            var (neighbors, weights) = graph.GetNeighbors(current);
            for (int i = 0; i < neighbors.Length; i++)
            {
                var neighbor = neighbors[i];
                var weight = paymentSizeSats.HasValue
                    ? GetEdgeWeight(edgeDictionary[(current, neighbor)].Weight, paymentSizeSats.Value)
                    : 1.0;

                var newDist = distances[current] + weight;
                if (newDist < distances[neighbor])
                {
                    distances[neighbor] = newDist;
                    priorityQueue.Enqueue(neighbor, newDist);
                }
            }
        }

        return distances;
    }

    public static double GetEdgeWeight(Weight weight, int paymentSizeSats)
    {
        return weight.BaseMSat / 1000.0 + (paymentSizeSats / 1_000_000.0) * weight.ProportionalMillionths;
    }
}