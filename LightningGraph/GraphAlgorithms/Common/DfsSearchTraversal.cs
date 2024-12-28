using LightningGraph.Core;
using LightningGraph.Implementations;

namespace LightningGraph.GraphAlgorithms.Common;

// ReSharper disable once InconsistentNaming
public class DfsSearchTraversal : IGraphSearchTraversal
{
    private TraversalState _state;
    private Action<int> _preVisit;
    private Action<int> _postVisit;
    private Action<int, int> _onEdgeTraversal;

    public void TraverseGraph(
        FastLightningGraph graph,
        int startVertex,
        Action<int> preVisit = null,
        Action<int> postVisit = null,
        Action<int, int> onEdgeTraversal = null)
    {
        _state = new TraversalState(graph.VertexCount);
        _preVisit = preVisit;
        _postVisit = postVisit;
        _onEdgeTraversal = onEdgeTraversal;

        DFS(graph, startVertex);
    }

    // ReSharper disable once InconsistentNaming
    private void DFS(FastLightningGraph graph, int vertex)
    {
        _state.Visited[vertex] = true;
        _state.DiscoveryTime[vertex] = ++_state.Time;
        _preVisit?.Invoke(vertex);

        foreach (int neighbor in graph.GetNeighbors(vertex))
        {
            _onEdgeTraversal?.Invoke(vertex, neighbor);

            if (!_state.Visited[neighbor])
            {
                _state.Parent[neighbor] = vertex;
                DFS(graph, neighbor);
            }
        }

        _state.FinishTime[vertex] = ++_state.Time;
        _postVisit?.Invoke(vertex);
    }

    public TraversalState GetTraversalState() => _state;
}