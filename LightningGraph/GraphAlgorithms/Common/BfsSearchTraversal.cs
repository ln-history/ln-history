// using LightningGraph.Core;
// using LightningGraph.Implementations;
//
// namespace LightningGraph.GraphAlgorithms.Common;
//
// // ReSharper disable once InconsistentNaming
// public class BfsSearchTraversal : IGraphSearchTraversal
// {
//     private TraversalState _state;
//     private Action<int> _preVisit;
//     private Action<int> _postVisit;
//     private Action<int, int> _onEdgeTraversal;
//
//     public void TraverseGraph(
//         FastLightningGraph graph,
//         int startVertex,
//         Action<int> preVisit = null,
//         Action<int> postVisit = null,
//         Action<int, int> onEdgeTraversal = null)
//     {
//         _state = new TraversalState(graph.VertexCount);
//         _preVisit = preVisit;
//         _postVisit = postVisit;
//         _onEdgeTraversal = onEdgeTraversal;
//
//         var queue = new Queue<int>();
//         queue.Enqueue(startVertex);
//         _state.Visited[startVertex] = true;
//         _state.DiscoveryTime[startVertex] = ++_state.Time;
//         _preVisit?.Invoke(startVertex);
//
//         while (queue.Count > 0)
//         {
//             int vertex = queue.Dequeue();
//
//             foreach (int neighbor in graph.GetNeighbors(vertex))
//             {
//                 _onEdgeTraversal?.Invoke(vertex, neighbor);
//
//                 if (!_state.Visited[neighbor])
//                 {
//                     queue.Enqueue(neighbor);
//                     _state.Visited[neighbor] = true;
//                     _state.Parent[neighbor] = vertex;
//                     _state.DiscoveryTime[neighbor] = ++_state.Time;
//                     _preVisit?.Invoke(neighbor);
//                 }
//             }
//
//             _state.FinishTime[vertex] = ++_state.Time;
//             _postVisit?.Invoke(vertex);
//         }
//     }
//
//     public TraversalState GetTraversalState() => _state;
// }