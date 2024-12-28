using LightningGraph.Core;
using LightningGraph.Model;

namespace LightningGraph.GraphAlgorithms.Metrics;

public static class NetworkAnalyzer
{
    public static NetworkMetrics Calculate(LightningFastGraph graph)
    {
        return new NetworkMetrics
        {
            Diameter = CalculateDiameter(graph),
            AveragePathLength = CalculateAveragePathLength(graph),
            AverageDegree = CalculateAverageDegree(graph),
            ClusteringCoefficient = CalculateClusteringCoefficient(graph),
            Density = CalculateDensity(graph),
            NodeIdsSortedByHighestDegreeTop100 = GetTopNodesByDegree(graph, 100)
        };
    }

    private static int CalculateDiameter(LightningFastGraph graph)
    {
        double maxDistance = 0;

        foreach (var node in graph.GetNodes())
        {
            var currentDistances = DijkstraShortestPaths(graph, node);
            foreach (var dist in currentDistances.Values)
            {
                maxDistance = Math.Max(maxDistance, dist);
            }
        }

        return (int)maxDistance;
    }

    private static double CalculateAveragePathLength(LightningFastGraph graph)
    {
        double totalDistance = 0;
        long pathCount = 0;

        foreach (var node in graph.GetNodes())
        {
            var distances = DijkstraShortestPaths(graph, node);
            totalDistance += distances.Values.Where(d => d < double.MaxValue).Sum();
            pathCount += distances.Values.Count(d => d < double.MaxValue);
        }

        return pathCount == 0 ? 0 : totalDistance / pathCount;
    }

    private static Dictionary<string, double> DijkstraShortestPaths(LightningFastGraph graph, string source)
    {
        var distances = new Dictionary<string, double>();
        var priorityQueue = new PriorityQueue<string, double>();

        foreach (var node in graph.GetNodes())
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
                var weight = weights[i];
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

    private static double CalculateAverageDegree(LightningFastGraph graph)
    {
        return graph.NodeCount == 0 ? 0 : (double)graph.EdgeCount * 2 / graph.NodeCount;
    }

    private static double CalculateClusteringCoefficient(LightningFastGraph graph)
    {
        double totalCoefficient = 0;

        foreach (var node in graph.GetNodes())
        {
            var (neighbors, _) = graph.GetNeighbors(node);
            int possibleConnections = neighbors.Length * (neighbors.Length - 1) / 2;
            if (possibleConnections == 0) continue;

            int actualConnections = 0;
            for (int i = 0; i < neighbors.Length; i++)
            {
                for (int j = i + 1; j < neighbors.Length; j++)
                {
                    var (subNeighbors, _) = graph.GetNeighbors(neighbors[i]);
                    if (subNeighbors.Contains(neighbors[j]))
                        actualConnections++;
                }
            }

            totalCoefficient += (double)actualConnections / possibleConnections;
        }

        return graph.NodeCount == 0 ? 0 : totalCoefficient / graph.NodeCount;
    }

    private static double CalculateDensity(LightningFastGraph graph)
    {
        int n = graph.NodeCount;
        return n == 0 ? 0 : (2.0 * graph.EdgeCount) / (n * (n - 1));
    }

    private static string[] GetTopNodesByDegree(LightningFastGraph graph, int count)
    {
        return graph.GetNodes()
            .Select(node => new { Node = node, Degree = graph.GetDegree(node) })
            .OrderByDescending(x => x.Degree)
            .Take(count)
            .Select(x => x.Node)
            .ToArray();
    }
}