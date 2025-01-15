using LightningGraph.Core;
using LightningGraph.Model;

namespace LightningGraph.GraphAlgorithms.Metrics;

public static class CentralityAnalyzer
{
    public static CentralityMetrics CalculateCentralityAnalytically(
        LightningFastGraph graph, int? paymentSizeSats = null)
    {
        var centrality = new Dictionary<string, double>();

        foreach (var source in graph.GetVertices())
        {
            var distances = DijkstraShortestPaths.Calculate(graph, source);
            foreach (var target in graph.GetVertices())
            {
                if (target == source || Math.Abs(distances[target] - double.MaxValue) < 1e-6) continue;

                foreach (var intermediate in graph.GetVertices())
                {
                    if (intermediate == source || intermediate == target) continue;

                    var pathThroughIntermediate = distances[intermediate] + 
                        DijkstraShortestPaths.Calculate(graph, intermediate)[target];
                    
                    if (Math.Abs(pathThroughIntermediate - distances[target]) < 1e-6)
                    {
                        centrality[intermediate] = centrality.GetValueOrDefault(intermediate, 0) + 1;
                    }
                }
            }
        }

        var mostCentral = centrality.OrderByDescending(kvp => kvp.Value).First();
        return new CentralityMetrics
        {
            BetweennessCentrality = centrality,
            AverageCentrality = centrality.Values.Average(),
            MostCentralVertex = mostCentral.Key,
            Runs = graph.NodeCount
        };
    }
    
    public static CentralityMetrics CalculateEmpiricalCentrality(
        LightningFastGraph graph,
        int runs = 1000,
        int? paymentSizeSats = null,
        int? seed = null)
    {
        var random = seed.HasValue ? new Random(seed.Value) : new Random();
        var centrality = new Dictionary<string, double>();
        var nodes = graph.GetVertices().ToArray();
        var edgeDictionary = graph.GetEdgesDictionary();

        // Initialize centrality for all nodes
        foreach (var node in nodes)
        {
            centrality[node] = 0;
        }

        for (int run = 0; run < runs; run++)
        {
            // Randomly select source and target nodes
            var source = nodes[random.Next(nodes.Length)];
            var target = nodes[random.Next(nodes.Length)];

            // Skip if source and target are the same
            if (source == target) continue;

            // Calculate shortest paths from source
            var distances = DijkstraShortestPaths.Calculate(graph, source);

            if (Math.Abs(distances[target] - double.MaxValue) < 1e-6)
            {
                // No path exists between source and target
                continue;
            }

            // Backtrack to calculate contributions to centrality
            var currentNode = target;
            while (currentNode != source)
            {
                foreach (var neighbor in graph.GetNeighbors(currentNode).Neighbors)
                {
                    // Check if this neighbor is part of the shortest path
                    if (Math.Abs(distances[neighbor] + 
                        (paymentSizeSats.HasValue ? 
                        DijkstraShortestPaths.GetEdgeWeight(edgeDictionary[(currentNode, neighbor)].Weight, paymentSizeSats.Value) : 
                        1.0) 
                        - distances[currentNode]) < 1e-6)
                    {
                        centrality[neighbor] += 1;
                        currentNode = neighbor;
                        break;
                    }
                }
            }
        }

        var mostCentral = centrality.OrderByDescending(kvp => kvp.Value).First();

        return new CentralityMetrics
        {
            BetweennessCentrality = centrality,
            AverageCentrality = centrality.Values.Average(),
            MostCentralVertex = mostCentral.Key,
            Runs = runs
        };
    }
}