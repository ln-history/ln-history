using LightningGraph.Core;
using LightningGraph.Implementations;

namespace LightningGraph.GraphAlgorithms.Common;

public static class GraphTraversal
{
    public static Dictionary<int, double> GetShortestPaths(
        FastLightningGraph graph,
        int startVertex,
        Func<int, int, double?> getCost)
    {
        var distances = new Dictionary<int, double>();
        var visited = new HashSet<int>();
        var pq = new PriorityQueue<int, double>();

        distances[startVertex] = 0;
        pq.Enqueue(startVertex, 0);

        while (pq.Count > 0)
        {
            int current = pq.Dequeue();
            if (visited.Contains(current)) continue;
            
            visited.Add(current);

            foreach (int neighbor in graph.GetNeighbors(current))
            {
                var cost = getCost(current, neighbor);
                if (!cost.HasValue) continue;

                double newDistance = distances[current] + cost.Value;
                if (!distances.ContainsKey(neighbor) || newDistance < distances[neighbor])
                {
                    distances[neighbor] = newDistance;
                    pq.Enqueue(neighbor, newDistance);
                }
            }
        }

        return distances;
    }

    public static PathResult GetShortestPath(
        FastLightningGraph graph,
        int startVertex,
        int endVertex)
    {
        if (!graph.VertexExists(startVertex) || !graph.VertexExists(endVertex))
            return PathResult.NotFound;

        var distances = new Dictionary<int, double>();
        var parents = new Dictionary<int, int>();
        var visited = new HashSet<int>();
        var pq = new PriorityQueue<int, double>();

        distances[startVertex] = 0;
        pq.Enqueue(startVertex, 0);

        while (pq.Count > 0)
        {
            int current = pq.Dequeue();
            if (current == endVertex) break;
            if (visited.Contains(current)) continue;

            visited.Add(current);

            foreach (int neighbor in graph.GetNeighbors(current))
            {
                var cost = graph.GetEdgeCost(current, neighbor);
                if (!cost.HasValue) continue;

                double newDistance = distances[current] + cost.Value;
                if (!distances.ContainsKey(neighbor) || newDistance < distances[neighbor])
                {
                    distances[neighbor] = newDistance;
                    parents[neighbor] = current;
                    pq.Enqueue(neighbor, newDistance);
                }
            }
        }

        if (!distances.ContainsKey(endVertex))
            return PathResult.NotFound;

        var path = ReconstructPath(parents, startVertex, endVertex);
        return PathResult.Found(path, distances[endVertex]);
    }

    public static Dictionary<int, List<int>> GetAllShortestPaths(
        FastLightningGraph graph,
        int startVertex)
    {
        if (!graph.VertexExists(startVertex))
            return new Dictionary<int, List<int>>();

        var distances = new Dictionary<int, double>();
        var parents = new Dictionary<int, HashSet<int>>();
        var visited = new HashSet<int>();
        var pq = new PriorityQueue<int, double>();

        distances[startVertex] = 0;
        parents[startVertex] = new HashSet<int> { -1 };
        pq.Enqueue(startVertex, 0);

        while (pq.Count > 0)
        {
            int current = pq.Dequeue();
            if (visited.Contains(current)) continue;

            visited.Add(current);

            foreach (int neighbor in graph.GetNeighbors(current))
            {
                var cost = graph.GetEdgeCost(current, neighbor);
                if (!cost.HasValue) continue;

                double newDistance = distances[current] + cost.Value;

                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = newDistance;
                    parents[neighbor] = new HashSet<int> { current };
                    pq.Enqueue(neighbor, newDistance);
                }
                else if (Math.Abs(newDistance - distances[neighbor]) < 1e-10) // Compare with epsilon for floating-point equality
                {
                    parents[neighbor].Add(current);
                }
                else if (newDistance < distances[neighbor])
                {
                    distances[neighbor] = newDistance;
                    parents[neighbor] = new HashSet<int> { current };
                    pq.Enqueue(neighbor, newDistance);
                }
            }
        }

        var paths = new Dictionary<int, List<int>>();
        foreach (var vertex in graph.Vertices)
        {
            if (distances.ContainsKey(vertex))
            {
                paths[vertex] = ReconstructShortestPath(parents, startVertex, vertex);
            }
        }

        return paths;
    }

    public static List<List<int>> GetAllPaths(
        FastLightningGraph graph,
        int startVertex,
        int endVertex,
        Func<int, int, double?> getCost,
        double maxCost = double.PositiveInfinity,
        int maxPaths = 100
        )
    {
        if (!graph.VertexExists(startVertex) || !graph.VertexExists(endVertex))
            return new List<List<int>>();

        var paths = new List<List<int>>();
        var visited = new HashSet<int>();
        var currentPath = new List<int>();
        var currentCost = 0.0;

        void DFS(int current)
        {
            if (paths.Count >= maxPaths) return;
            if (current == endVertex)
            {
                paths.Add(new List<int>(currentPath));
                return;
            }

            foreach (int neighbor in graph.GetNeighbors(current))
            {
                if (visited.Contains(neighbor)) continue;

                var edgeCost = getCost(current, neighbor);
                if (!edgeCost.HasValue) continue;

                double newCost = currentCost + edgeCost.Value;
                if (newCost > maxCost) continue;

                visited.Add(neighbor);
                currentPath.Add(neighbor);
                currentCost = newCost;

                DFS(neighbor);

                visited.Remove(neighbor);
                currentPath.RemoveAt(currentPath.Count - 1);
                currentCost -= edgeCost.Value;
            }
        }

        visited.Add(startVertex);
        currentPath.Add(startVertex);
        DFS(startVertex);

        return paths;
    }

    private static List<int> ReconstructPath(Dictionary<int, int> parents, int startVertex, int endVertex)
    {
        var path = new List<int>();
        int current = endVertex;

        while (current != startVertex)
        {
            path.Add(current);
            current = parents[current];
        }

        path.Add(startVertex);
        path.Reverse();
        return path;
    }

    private static List<int> ReconstructShortestPath(Dictionary<int, HashSet<int>> parents, int startVertex, int endVertex)
    {
        var path = new List<int>();
        int current = endVertex;

        while (current != startVertex)
        {
            path.Add(current);
            current = parents[current].First(); // Take any of the shortest paths
        }

        path.Add(startVertex);
        path.Reverse();
        return path;
    }
}