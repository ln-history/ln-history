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
        var nodes = graph.GetNodes().ToHashSet();
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
        var lowTime = new Dictionary<string, int>();
        var bridges = new List<(string, string)>();
        var time = 0;

        foreach (var node in graph.GetNodes())
        {
            if (!visited.Contains(node))
            {
                DFSBridge(graph, node, null, visited, discoveryTime, lowTime, 
                         ref time, bridges);
            }
        }

        return bridges;
    }

    // ReSharper disable once InconsistentNaming
    private static void DFSBridge(
        LightningFastGraph graph,
        string current,
        string parent,
        HashSet<string> visited,
        Dictionary<string, int> discoveryTime,
        Dictionary<string, int> lowTime,
        ref int time,
        List<(string, string)> bridges)
    {
        visited.Add(current);
        discoveryTime[current] = lowTime[current] = ++time;

        foreach (var edge in graph.GetOutgoingEdges(current))
        {
            var neighbor = edge.To;
            
            if (neighbor == parent) continue;

            if (!visited.Contains(neighbor))
            {
                DFSBridge(graph, neighbor, current, visited, discoveryTime, 
                         lowTime, ref time, bridges);

                lowTime[current] = Math.Min(lowTime[current], lowTime[neighbor]);

                if (lowTime[neighbor] > discoveryTime[current])
                {
                    bridges.Add((current, neighbor));
                }
            }
            else
            {
                lowTime[current] = Math.Min(lowTime[current], 
                                          discoveryTime[neighbor]);
            }
        }
    }

    private static double CalculateCriticality(
        LightningFastGraph graph,
        List<(string, string)> bridges)
    {
        return graph.EdgeCount > 0 ? (double)bridges.Count / graph.EdgeCount : 0;
    }
}