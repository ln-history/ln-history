using LightningGraph.Core;
using LightningGraph.Model;

namespace LightningGraph.GraphAlgorithms.Metrics;

public static class NetworkAnalyzer
{
    public static NetworkMetrics CalculateAll(LightningFastGraph graph)
    {
        return new NetworkMetrics
        {
            Diameter = CalculateDiameter(graph),
            AveragePathLength = CalculateAveragePathLength(graph),
            AverageDegree = CalculateAverageDegree(graph),
            AverageLocalClusteringCoefficient = CalculateAverageLocalClusteringCoefficient(graph),
            GlobalClusteringCoefficient = CalculateGlobalClusteringCoefficient(graph),
            Density = CalculateDensity(graph),
            NodeIdsSortedByHighestDegreeTop100 = GetTopNodesByDegree(graph, 100)
        };
    }

    public static int CalculateDiameter(LightningFastGraph graph)
    {
        double maxDistance = 0;

        foreach (var node in graph.GetNodes())
        {
            var currentDistances = DijkstraShortestPaths(graph, node);
            foreach (var dist in currentDistances.Values)
            {
                if (dist < double.MaxValue)
                {
                    maxDistance = Math.Max(maxDistance, dist);
                }
            }
        }

        return (int)maxDistance;
    }

    public static double CalculateAveragePathLength(LightningFastGraph graph)
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

    public static Dictionary<string, double> DijkstraShortestPaths(LightningFastGraph graph, string source)
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

    public static double CalculateAverageDegree(LightningFastGraph graph)
    {
        return graph.NodeCount == 0 ? 0 : (double)graph.EdgeCount * 2 / graph.NodeCount;
    }

    public static double CalculateAverageLocalClusteringCoefficient(LightningFastGraph graph)
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
    
    public static double CalculateGlobalClusteringCoefficient(LightningFastGraph graph)
    {
        double closedTriplets = 0;
        double totalTriplets = 0;

        foreach (var node in graph.GetNodes())
        {
            var (neighbors, _) = graph.GetNeighbors(node);
            int degree = neighbors.Length;

            // Count triplets involving this node
            totalTriplets += degree * (degree - 1) / 2;

            // Count closed triplets (triangles)
            for (int i = 0; i < neighbors.Length; i++)
            {
                for (int j = i + 1; j < neighbors.Length; j++)
                {
                    var (subNeighbors, _) = graph.GetNeighbors(neighbors[i]);
                    if (subNeighbors.Contains(neighbors[j]))
                        closedTriplets++;
                }
            }
        }

        return totalTriplets == 0 ? 0 : closedTriplets / totalTriplets;
    }

    public static double CalculateDensity(LightningFastGraph graph)
    {
        int n = graph.NodeCount;
        return n == 0 ? 0 : (2.0 * graph.EdgeCount) / (n * (n - 1));
    }

    public static string[] GetTopNodesByDegree(LightningFastGraph graph, int count)
    {
        return graph.GetNodes()
            .Select(node => new { Node = node, Degree = graph.GetDegree(node) })
            .OrderByDescending(x => x.Degree)
            .Take(count)
            .Select(x => x.Node)
            .ToArray();
    }
}