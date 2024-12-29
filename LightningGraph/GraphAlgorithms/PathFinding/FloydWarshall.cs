// using LightningGraph.Core;
// using LightningGraph.Implementations;
//
// namespace LightningGraph.GraphAlgorithms.PathFinding;
//
// public static class FloydWarshall
// {
//     // Original unweighted version
//     public static int[,] Calculate(FastLightningGraph graph)
//     {
//         int n = graph.VertexCount;
//         var distances = new int[n, n];
//         
//         // Initialize distances
//         for (int i = 0; i < n; i++)
//         {
//             for (int j = 0; j < n; j++)
//             {
//                 if (i == j)
//                     distances[i, j] = 0;
//                 else if (graph.GetNeighbors(i).Contains(j))
//                     distances[i, j] = 1;
//                 else
//                     distances[i, j] = int.MaxValue;
//             }
//         }
//
//         // Floyd-Warshall algorithm
//         for (int k = 0; k < n; k++)
//         {
//             if (!graph.VertexExists(k)) continue;
//             
//             for (int i = 0; i < n; i++)
//             {
//                 if (!graph.VertexExists(i)) continue;
//                 
//                 for (int j = 0; j < n; j++)
//                 {
//                     if (!graph.VertexExists(j)) continue;
//                     
//                     if (distances[i, k] != int.MaxValue && 
//                         distances[k, j] != int.MaxValue &&
//                         distances[i, k] + distances[k, j] < distances[i, j])
//                     {
//                         distances[i, j] = distances[i, k] + distances[k, j];
//                     }
//                 }
//             }
//         }
//
//         return distances;
//     }
//
//     // New weighted version using edge costs
//     public static double[,] CalculateWeighted(
//         FastLightningGraph graph,
//         Func<int, int, double?> getCost)
//     {
//         int n = graph.VertexCount;
//         var distances = new double[n, n];
//         
//         // Initialize distances
//         for (int i = 0; i < n; i++)
//         {
//             for (int j = 0; j < n; j++)
//             {
//                 if (i == j)
//                     distances[i, j] = 0;
//                 else if (graph.GetNeighbors(i).Contains(j))
//                 {
//                     var cost = getCost(i, j);
//                     distances[i, j] = cost ?? double.PositiveInfinity;
//                 }
//                 else
//                     distances[i, j] = double.PositiveInfinity;
//             }
//         }
//
//         // Floyd-Warshall algorithm
//         for (int k = 0; k < n; k++)
//         {
//             if (!graph.VertexExists(k)) continue;
//             
//             for (int i = 0; i < n; i++)
//             {
//                 if (!graph.VertexExists(i)) continue;
//                 
//                 for (int j = 0; j < n; j++)
//                 {
//                     if (!graph.VertexExists(j)) continue;
//                     
//                     if (!double.IsPositiveInfinity(distances[i, k]) && 
//                         !double.IsPositiveInfinity(distances[k, j]) &&
//                         distances[i, k] + distances[k, j] < distances[i, j])
//                     {
//                         distances[i, j] = distances[i, k] + distances[k, j];
//                     }
//                 }
//             }
//         }
//
//         return distances;
//     }
// }