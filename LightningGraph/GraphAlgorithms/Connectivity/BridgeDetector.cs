// using LightningGraph.Core;
// using LightningGraph.GraphAlgorithms.Common;
// using LightningGraph.Implementations;
// using LightningGraph.Model;
//
// namespace LightningGraph.GraphAlgorithms.Connectivity;
//
// public static class BridgeDetector
// {
//     public static BridgeAnalysis Analyze(FastLightningGraph graph)
//     {
//         var bridges = FindBridges(graph);
//         var componentsAfterRemoval = CountComponentsAfterRemoval(graph, bridges);
//         var criticalityScore = CalculateCriticality(graph, bridges);
//
//         return new BridgeAnalysis
//         {
//             BridgeCount = bridges.Count,
//             Bridges = bridges,
//             ComponentsAfterRemoval = componentsAfterRemoval,
//             CriticalityScore = criticalityScore
//         };
//     }
//
//     private static int CountComponentsAfterRemoval(
//         FastLightningGraph graph,
//         List<(int From, int To)> bridges)
//     {
//         // Create a working set of edges to consider
//         var validEdges = new HashSet<(int, int)>();
//         var bridgeSet = new HashSet<(int, int)>(bridges);
//
//         // Add all non-bridge edges to the working set
//         foreach (var edge in graph.Edges)
//         {
//             var normalizedEdge = (Math.Min(edge.From, edge.To), Math.Max(edge.From, edge.To));
//             if (!bridgeSet.Contains(normalizedEdge))
//             {
//                 validEdges.Add(normalizedEdge);
//             }
//         }
//
//         // Use DFS to count components
//         var visited = new bool[graph.VertexCount];
//         int components = 0;
//
//         foreach (var vertex in graph.Vertices)
//         {
//             if (!visited[vertex])
//             {
//                 ComponentDFS(graph, vertex, visited, validEdges);
//                 components++;
//             }
//         }
//
//         return components;
//     }
//     
//     // ReSharper disable once InconsistentNaming
//     private static void ComponentDFS(
//         FastLightningGraph graph,
//         int current,
//         bool[] visited,
//         HashSet<(int, int)> validEdges)
//     {
//         visited[current] = true;
//
//         foreach (var neighbor in graph.GetNeighbors(current))
//         {
//             var edge = (Math.Min(current, neighbor), Math.Max(current, neighbor));
//             if (!visited[neighbor] && validEdges.Contains(edge))
//             {
//                 ComponentDFS(graph, neighbor, visited, validEdges);
//             }
//         }
//     }
//
//     public static List<(int From, int To)> FindBridges(
//         FastLightningGraph graph)
//     {
//         var bridges = new List<(int, int)>();
//         var lowTime = new int[graph.VertexCount];
//         Array.Fill(lowTime, -1);
//
//         var dfs = new DfsSearchTraversal();
//
//         void PreVisit(int vertex)
//         {
//             lowTime[vertex] = dfs.GetTraversalState().DiscoveryTime[vertex];
//         }
//
//         void OnEdgeTraversal(int from, int to)
//         {
//             var state = dfs.GetTraversalState();
//             
//             if (state.Parent[from] != to) // Back edge
//             {
//                 lowTime[from] = Math.Min(lowTime[from], state.DiscoveryTime[to]);
//             }
//             else if (state.Parent[to] == from) // Tree edge
//             {
//                 if (lowTime[to] > state.DiscoveryTime[from])
//                 {
//                     bridges.Add((Math.Min(from, to), Math.Max(from, to)));
//                 }
//                 lowTime[from] = Math.Min(lowTime[from], lowTime[to]);
//             }
//         }
//
//         foreach (var vertex in graph.Vertices)
//         {
//             if (!dfs.GetTraversalState()?.Visited[vertex] ?? true)
//             {
//                 dfs.TraverseGraph(graph, vertex, PreVisit, onEdgeTraversal: OnEdgeTraversal);
//             }
//         }
//
//         return bridges;
//     }
//
//     // ReSharper disable once InconsistentNaming
//     private static void DFSBridge(
//         FastLightningGraph graph,
//         int current,
//         int parent,
//         bool[] visited,
//         int[] discoveryTime,
//         int[] lowTime,
//         ref int time,
//         List<(int, int)> bridges)
//     {
//         visited[current] = true;
//         discoveryTime[current] = lowTime[current] = ++time;
//
//         foreach (int neighbor in graph.GetNeighbors(current))
//         {
//             if (!visited[neighbor])
//             {
//                 DFSBridge(graph, neighbor, current, visited, discoveryTime, lowTime, ref time, bridges);
//
//                 lowTime[current] = Math.Min(lowTime[current], lowTime[neighbor]);
//
//                 if (lowTime[neighbor] > discoveryTime[current])
//                 {
//                     bridges.Add((Math.Min(current, neighbor), Math.Max(current, neighbor)));
//                 }
//             }
//             else if (neighbor != parent)
//             {
//                 lowTime[current] = Math.Min(lowTime[current], discoveryTime[neighbor]);
//             }
//         }
//     }
//
//     private static double CalculateCriticality(
//         FastLightningGraph graph,
//         List<(int, int)> bridges)
//     {
//         return graph.EdgeCount > 0 ? (double)bridges.Count / graph.EdgeCount : 0;
//     }
// }