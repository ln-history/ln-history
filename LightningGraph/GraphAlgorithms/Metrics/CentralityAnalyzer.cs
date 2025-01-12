using LightningGraph.Core;
using LightningGraph.Model;

namespace LightningGraph.GraphAlgorithms.Metrics;

public static class CentralityAnalyzer
{
    public static CentralityMetrics CalculateCentralityAnalytically(LightningFastGraph graph)
    {
        var nodes = graph.GetNodes().ToList();
        var n = nodes.Count;
        var betweenness = nodes.ToDictionary(node => node, _ => 0.0);
        var nodeIndexMap = nodes.Select((node, index) => (node, index))
                               .ToDictionary(x => x.node, x => x.index);

        // For each node as source
        Parallel.ForEach(nodes, source =>
        {
            var stack = new Stack<string>();
            var predecessors = new Dictionary<string, List<string>>();
            var distances = new Dictionary<string, double>();
            var pathCount = new Dictionary<string, double>();

            // Initialize
            foreach (var node in nodes)
            {
                predecessors[node] = new List<string>();
                distances[node] = double.PositiveInfinity;
                pathCount[node] = 0;
            }

            distances[source] = 0;
            pathCount[source] = 1;

            // Modified Dijkstra's algorithm
            var queue = new PriorityQueue<string, double>();
            queue.Enqueue(source, 0);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                stack.Push(current);

                foreach (var edge in graph.GetOutgoingEdges(current))
                {
                    var neighbor = edge.To;
                    var distance = distances[current] + 1;

                    if (distance < distances[neighbor])
                    {
                        distances[neighbor] = distance;
                        predecessors[neighbor].Clear();
                        predecessors[neighbor].Add(current);
                        pathCount[neighbor] = pathCount[current];
                        queue.Enqueue(neighbor, distance);
                    }
                    else if (Math.Abs(distance - distances[neighbor]) < Math.Pow(10,-3))
                    {
                        predecessors[neighbor].Add(current);
                        pathCount[neighbor] += pathCount[current];
                    }
                }
            }

            // Accumulation phase
            var dependency = new Dictionary<string, double>();
            foreach (var node in nodes) dependency[node] = 0;

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                foreach (var predecessor in predecessors[current])
                {
                    var factor = pathCount[predecessor] / pathCount[current] * (1 + dependency[current]);
                    dependency[predecessor] += factor;
                }
                if (current != source)
                {
                    lock (betweenness)
                    {
                        betweenness[current] += dependency[current];
                    }
                }
            }
        });

        return new CentralityMetrics
        {
            BetweennessCentrality = betweenness,
            AverageCentrality = betweenness.Values.Average(),
            MostCentralVertex = betweenness.MaxBy(x => x.Value).Key,
            Runs = n * (n - 1)
        };
    }

    public static CentralityMetrics CalculateEmpiricalCentrality(
        LightningFastGraph graph,
        int runs = 1000,
        int? seed = null)
    {
        var random = seed.HasValue ? new Random(seed.Value) : new Random();
        var nodes = graph.GetNodes().ToList();
        var betweenness = nodes.ToDictionary(node => node, _ => 0.0);
        var successfulRuns = 0;

        Parallel.For(0, runs, _ =>
        {
            // Randomly select source and target
            var source = nodes[random.Next(nodes.Count)];
            var target = nodes[random.Next(nodes.Count)];
            if (source == target) return;

            var (path, found) = FindShortestPath(graph, source, target);
            if (!found) return;

            Interlocked.Increment(ref successfulRuns);
            
            // Update betweenness for intermediate nodes
            for (int i = 1; i < path.Count - 1; i++)
            {
                lock (betweenness)
                {
                    betweenness[path[i]] += 1.0 / runs;
                }
            }
        });

        return new CentralityMetrics
        {
            BetweennessCentrality = betweenness,
            AverageCentrality = betweenness.Values.Average(),
            MostCentralVertex = betweenness.MaxBy(x => x.Value).Key,
            Runs = successfulRuns
        };
    }

    private static (List<string> Path, bool Found) FindShortestPath(
        LightningFastGraph graph, 
        string source, 
        string target)
    {
        var distances = new Dictionary<string, double>();
        var predecessors = new Dictionary<string, string>();
        var queue = new PriorityQueue<string, double>();
        
        distances[source] = 0;
        queue.Enqueue(source, 0);
        
        var current = queue.Dequeue();

        while (queue.Count > 0)
        {
            if (current == target) break;

            foreach (var edge in graph.GetOutgoingEdges(current))
            {
                var newDist = distances[current] + 1;
                if (!distances.ContainsKey(edge.To) || newDist < distances[edge.To])
                {
                    distances[edge.To] = newDist;
                    predecessors[edge.To] = current;
                    queue.Enqueue(edge.To, newDist);
                }
            }
            
            current = queue.Dequeue();
        }

        if (!distances.ContainsKey(target))
            return (new List<string>(), false);

        var path = new List<string>();
        current = target;
        while (current != null)
        {
            path.Add(current);
            predecessors.TryGetValue(current, out current);
        }
        path.Reverse();
        return (path, true);
    }
}