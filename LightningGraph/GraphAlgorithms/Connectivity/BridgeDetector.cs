using LightningGraph.Core;
using LightningGraph.Model;

namespace LightningGraph.GraphAlgorithms.Connectivity;

public static class BridgeDetector
{
    public static BridgeAnalysis Analyze(LightningFastGraph graph)
    {
        var bridges = FindBridges(graph);
        var componentsAfterRemoval = CountComponentsAfterRemoval(graph, bridges);
        var criticalityScore = CalculateCriticality(graph, bridges);

        return new BridgeAnalysis
        {
            BridgeCount = bridges.Count,
            Bridges = bridges,
            ComponentsAfterRemoval = componentsAfterRemoval,
            CriticalityScore = criticalityScore
        };
    }

    private static int CountComponentsAfterRemoval(
        LightningFastGraph graph,
        List<(string From, string To)> bridges)
    {
        var nodes = graph.GetVertices().ToHashSet();
        var validEdges = new HashSet<(string, string)>();
        
        foreach (var node in nodes)
        {
            foreach (var edge in graph.GetOutgoingEdges(node))
            {
                if (!bridges.Contains((edge.From, edge.To)) && 
                    !bridges.Contains((edge.To, edge.From)))
                {
                    validEdges.Add((edge.From, edge.To));
                }
            }
        }

        var visited = new HashSet<string>();
        var components = 0;

        foreach (var node in nodes)
        {
            if (!visited.Contains(node))
            {
                ComponentDFS(graph, node, visited, validEdges);
                components++;
            }
        }

        return components;
    }

    // ReSharper disable once InconsistentNaming
    private static void ComponentDFS(
        LightningFastGraph graph,
        string current,
        HashSet<string> visited,
        HashSet<(string, string)> validEdges)
    {
        visited.Add(current);

        foreach (var edge in graph.GetOutgoingEdges(current))
        {
            if (!visited.Contains(edge.To) && 
                validEdges.Contains((current, edge.To)))
            {
                ComponentDFS(graph, edge.To, visited, validEdges);
            }
        }
    }

    public static List<(string From, string To)> FindBridges(LightningFastGraph graph)
    {
        var visited = new HashSet<string>();
        var discoveryTime = new Dictionary<string, int>();
        var low = new Dictionary<string, int>();
        var parent = new Dictionary<string, string>();
        var bridges = new List<(string, string)>();
        int time = 0;

        void Dfs(string node)
        {
            visited.Add(node);
            discoveryTime[node] = low[node] = ++time;

            foreach (var neighbor in graph.GetNeighbors(node).Neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    parent[neighbor] = node;
                    Dfs(neighbor);

                    low[node] = Math.Min(low[node], low[neighbor]);

                    if (low[neighbor] > discoveryTime[node])
                    {
                        bridges.Add((node, neighbor));
                    }
                }
                else if (neighbor != parent.GetValueOrDefault(node))
                {
                    low[node] = Math.Min(low[node], discoveryTime[neighbor]);
                }
            }
        }

        foreach (var node in graph.GetVertices())
        {
            if (!visited.Contains(node)) Dfs(node);
        }

        return bridges;
    }

    private static double CalculateCriticality(
        LightningFastGraph graph,
        List<(string, string)> bridges)
    {
        return graph.EdgeCount > 0 ? (double)bridges.Count / graph.EdgeCount : 0;
    }
}