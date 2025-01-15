using LightningGraph.Core;
using LightningGraph.Model;
using LightningGraph.Serialization;

namespace LightningGraph.GraphAlgorithms.Metrics;

public static class NetworkAnalyzer
{
    public static NetworkMetrics CalculateAll(LightningFastGraph graph)
    {
        return new NetworkMetrics
        {
            Diameter = CalculateDiameter(graph),
            AveragePathLength = CalculateAveragePathLength(graph),
            AverageDegree = CalculateAverageOutDegree(graph),
            AverageLocalClusteringCoefficient = CalculateAverageLocalClusteringCoefficient(graph),
            GlobalClusteringCoefficient = CalculateGlobalClusteringCoefficient(graph),
            Density = CalculateDensity(graph),
            NodeIdsSortedByHighestDegreeTop100 = GetTopNodesByDegree(graph, 100)
        };
    }

    public static int CalculateDiameter(LightningFastGraph graph)
    {
        var maxDiameter = 0;

        foreach (var node in graph.GetVertices())
        {
            var distances = DijkstraShortestPaths.Calculate(graph, node);
            var longest = distances.Values.Where(d => d < double.MaxValue).Max();
            maxDiameter = Math.Max(maxDiameter, (int)longest);
        }

        return maxDiameter;
    }
    
    public static double CalculateAveragePathLength(LightningFastGraph graph)
    {
        double totalDistance = 0;
        int pairCount = 0;

        foreach (var node in graph.GetVertices())
        {
            var distances = DijkstraShortestPaths.Calculate(graph, node);
            foreach (var distance in distances.Values)
            {
                if (distance < double.MaxValue)
                {
                    totalDistance += distance;
                    pairCount++;
                }
            }
        }

        return pairCount == 0 ? 0 : totalDistance / pairCount;
    }
    
    public static double CalculateAverageOutDegree(LightningFastGraph graph)
    {
        var totalOutDegree = graph.GetVertices().Sum(node => graph.GetOutDegree(node));
        return totalOutDegree / (double)graph.NodeCount;
    }
    
    public static double CalculateAverageLocalClusteringCoefficient(LightningFastGraph graph)
    {
        double totalClustering = 0;

        foreach (var node in graph.GetVertices())
        {
            var neighbors = graph.GetNeighbors(node).Neighbors;
            if (neighbors.Length < 2) continue;

            int connections = neighbors
                .SelectMany(neighbor => graph.GetNeighbors(neighbor).Neighbors)
                .Count(neighbors.Contains);

            totalClustering += connections / (double)(neighbors.Length * (neighbors.Length - 1));
        }

        return totalClustering / graph.NodeCount;
    }

    public static double CalculateGlobalClusteringCoefficient(LightningFastGraph graph)
    {
        int triangles = 0, triplets = 0;

        foreach (var node in graph.GetVertices())
        {
            var neighbors = graph.GetNeighbors(node).Neighbors;

            foreach (var neighbor in neighbors)
            {
                foreach (var mutual in graph.GetNeighbors(neighbor).Neighbors)
                {
                    if (neighbors.Contains(mutual))
                    {
                        triangles++;
                    }
                }
            }

            triplets += neighbors.Length * (neighbors.Length - 1);
        }

        return triplets == 0 ? 0 : (double)triangles / triplets;
    }
    
    public static double CalculateDensity(LightningFastGraph graph)
    {
        return graph.EdgeCount / (double)(graph.NodeCount * (graph.NodeCount - 1));
    }
    
    public static string[] GetTopNodesByDegree(LightningFastGraph graph, int count)
    {
        return graph.GetVertices()
            .OrderByDescending(node => graph.GetOutDegree(node) + graph.GetInDegree(node))
            .Take(count)
            .ToArray();
    }
}