// using LightningGraph.Core;
// using LightningGraph.GraphAlgorithms.Common;
// using LightningGraph.Implementations;
// using LightningGraph.Model;
//
// namespace LightningGraph.GraphAlgorithms.Metrics;
//
// public static class CentralityAnalyzer
// {
//     public static CentralityMetrics CalculateAnalyticalCentrality(
//         FastLightningGraph graph)
//     {
//         var betweenness = new Dictionary<int, double>();
//         var vertices = graph.Vertices.ToList();
//         
//         // Initialize betweenness centrality for all vertices
//         foreach (var vertex in vertices)
//         {
//             betweenness[vertex] = 0;
//         }
//
//         For each pair of vertices
//         for (int i = 0; i < vertices.Count; i++)
//         {
//             var source = vertices[i];
//             var shortestPaths = GraphTraversal.GetAllShortestPaths(graph, source);
//
//             foreach (var (target, path) in shortestPaths)
//             {
//                 if (source != target)
//                 {
//                     // Add centrality score for intermediate vertices
//                     foreach (var intermediateVertex in path.Skip(1).SkipLast(1))
//                     {
//                         betweenness[intermediateVertex] += 1.0;
//                     }
//                 }
//             }
//         }
//
//         // Normalize the centrality scores
//         int n = vertices.Count;
//         double normalizationFactor = 1.0 / ((n - 1) * (n - 2)); // Exclude source and target
//
//         foreach (var vertex in vertices)
//         {
//             betweenness[vertex] *= normalizationFactor;
//         }
//
//         return new CentralityMetrics
//         {
//             BetweennessCentrality = betweenness,
//             AverageCentrality = betweenness.Values.Average(),
//             MostCentralVertex = betweenness.MaxBy(x => x.Value).Key,
//             Runs = n * (n - 1) // Total number of paths analyzed
//         };
//     }
//
//     public static CentralityMetrics CalculateEmpiricalCentrality(
//         FastLightningGraph graph,
//         int runs = 1000,
//         int? seed = null)
//     {
//         var betweenness = new Dictionary<int, double>();
//         var vertices = graph.Vertices.ToList();
//         var random = seed.HasValue ? new Random(seed.Value) : new Random();
//         
//         // Initialize betweenness centrality for all vertices
//         foreach (var vertex in vertices)
//         {
//             betweenness[vertex] = 0;
//         }
//
//         int successfulRuns = 0;
//         for (int i = 0; i < runs; i++)
//         {
//             // Select random source and target
//             int sourceIndex = random.Next(vertices.Count);
//             int targetIndex = random.Next(vertices.Count);
//             
//             if (sourceIndex == targetIndex) continue;
//
//             var source = vertices[sourceIndex];
//             var target = vertices[targetIndex];
//
//             var pathResult = GraphTraversal.GetShortestPath(graph, source, target);
//             if (!pathResult.Exists) continue;
//
//             successfulRuns++;
//             
//             // Add centrality score for intermediate vertices
//             foreach (var intermediateVertex in pathResult.Path.Skip(1).SkipLast(1))
//             {
//                 betweenness[intermediateVertex] += 1.0;
//             }
//         }
//
//         // Normalize the centrality scores
//         if (successfulRuns > 0)
//         {
//             foreach (var vertex in vertices)
//             {
//                 betweenness[vertex] /= successfulRuns;
//             }
//         }
//
//         return new CentralityMetrics
//         {
//             BetweennessCentrality = betweenness,
//             AverageCentrality = betweenness.Values.Average(),
//             MostCentralVertex = betweenness.MaxBy(x => x.Value).Key,
//             Runs = successfulRuns
//         };
//     }
//
//     // Additional helper method for comparing analytical and empirical results
//     public static double CalculateCentralityCorrelation(
//         FastLightningGraph graph,
//         int runs = 1000,
//         int? seed = null)
//     {
//         var analytical = CalculateAnalyticalCentrality(graph);
//         var empirical = CalculateEmpiricalCentrality(graph, runs, seed);
//
//         var vertices = graph.Vertices.ToList();
//         var analyticalValues = vertices.Select(v => analytical.BetweennessCentrality[v]).ToList();
//         var empiricalValues = vertices.Select(v => empirical.BetweennessCentrality[v]).ToList();
//
//         return CalculatePearsonCorrelation(analyticalValues, empiricalValues);
//     }
//
//     private static double CalculatePearsonCorrelation(List<double> x, List<double> y)
//     {
//         double meanX = x.Average();
//         double meanY = y.Average();
//         
//         double sum1 = x.Zip(y, (a, b) => (a - meanX) * (b - meanY)).Sum();
//         double sum2 = Math.Sqrt(x.Sum(a => Math.Pow(a - meanX, 2)) * 
//                               y.Sum(b => Math.Pow(b - meanY, 2)));
//         
//         return sum2 == 0 ? 0 : sum1 / sum2;
//     }
// }