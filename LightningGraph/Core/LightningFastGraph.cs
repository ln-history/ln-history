using LightningGraph.GraphAlgorithms.Connectivity;
using LightningGraph.GraphAlgorithms.Metrics;
using LightningGraph.Model;
using LightningGraph.Serialization;
using LN_history.Data.Model;

namespace LightningGraph.Core;

public class LightningFastGraph
{
    private readonly Dictionary<string, HashSet<string>> _outgoingEdges;
    private readonly Dictionary<string, HashSet<string>> _incomingEdges;

    private readonly Dictionary<(string From, string To), Edge> _edges;
    private readonly HashSet<string> _nodes;

    // public readonly Dictionary<string, NodeInformation> NodeInformationDict = new();
    // public readonly Dictionary<string, ChannelInformation> EdgeInformationDict = new();
    
    public int NodeCount { get; private set; } = 0;

    public int EdgeCount { get; private set; } = 0;
    
    public LightningFastGraph(int initialNodeCapacity = 15000, int initialEdgeCapacity = 70000)
    {
        _outgoingEdges = new Dictionary<string, HashSet<string>>(initialNodeCapacity);
        _incomingEdges = new Dictionary<string, HashSet<string>>(initialNodeCapacity);
        _edges = new Dictionary<(string From, string To), Edge>(initialEdgeCapacity);
        _nodes = new HashSet<string>(initialNodeCapacity);
    }
    
    public void AddVertex(string nodeId)
    {
        if (!_nodes.Add(nodeId)) return;
        _outgoingEdges[nodeId] = new HashSet<string>();
        _incomingEdges[nodeId] = new HashSet<string>();

        NodeCount++;
    }

    public void AddEdge(Edge edge)
    {
        AddVertex(edge.From);
        AddVertex(edge.To);
        
        _outgoingEdges[edge.From].Add(edge.To);
        _incomingEdges[edge.To].Add(edge.From);
        _edges[(edge.From, edge.To)] = edge;
        
        EdgeCount++;
    }
    
    public void AddVerticesBatch(IEnumerable<string> vertices)
    {
        foreach (var vertex in vertices)
        {
            if (_nodes.Add(vertex))
            {
                _outgoingEdges[vertex] = new HashSet<string>();
                _incomingEdges[vertex] = new HashSet<string>();
                NodeCount++;
            }
        }
    }
    
    public void AddEdgesBatch(IEnumerable<Edge> edges)
    {
        foreach (var edge in edges)
        {
            // Add nodes if they do not already exist
            AddVertex(edge.From);
            AddVertex(edge.To);

            // Add the edge
            if (!_edges.ContainsKey((edge.From, edge.To)))
            {
                _outgoingEdges[edge.From].Add(edge.To);
                _incomingEdges[edge.To].Add(edge.From);
                _edges[(edge.From, edge.To)] = edge;

                EdgeCount++;
            }
        }
    }

    public IEnumerable<Edge> GetEdges()
    {
        return _edges.Values;
    }
    
    public Dictionary<(string, string), Edge> GetEdgesDictionary()
    {
        return _edges;
    }

    public IEnumerable<string> GetVertices()
    {
        return _nodes.ToList();
    }

    public IEnumerable<Edge> GetIncomingEdges(string nodeId)
    {
        if (!_incomingEdges.ContainsKey(nodeId))
            yield break;

        foreach (var fromNode in _incomingEdges[nodeId])
        {
            if (_edges.TryGetValue((fromNode, nodeId), out var edge))
            {
                yield return edge;
            }
        }
    }

    public IEnumerable<Edge> GetOutgoingEdges(string nodeId)
    {
        if (!_outgoingEdges.ContainsKey(nodeId))
            yield break;

        foreach (var toNode in _outgoingEdges[nodeId])
        {
            if (_edges.TryGetValue((nodeId, toNode), out var edge))
            {
                yield return edge;
            }
        }
    }

    public int GetOutDegree(string nodeId)
    {
        return _outgoingEdges.TryGetValue(nodeId, out var outgoing) ? outgoing.Count : 0;
    }

    public int GetInDegree(string nodeId)
    {
        return _incomingEdges.TryGetValue(nodeId, out var incoming) ? incoming.Count : 0;
    }

    public bool HasEdge(string from, string to)
    {
        return _edges.ContainsKey((from, to));
    }

    public (string[] Neighbors, Weight[] Weights) GetNeighbors(string nodeId)
    {
        if (!_outgoingEdges.TryGetValue(nodeId, out var neighbors))
            return (Array.Empty<string>(), Array.Empty<Weight>());

        var neighborList = new List<string>();
        var weightList = new List<Weight>();

        foreach (var neighbor in neighbors)
        {
            if (_edges.TryGetValue((nodeId, neighbor), out var edge))
            {
                neighborList.Add(neighbor);
                weightList.Add(edge.Weight);
            }
        }

        return (neighborList.ToArray(), weightList.ToArray());
    }

    // public string SerializeToJson(string graphName)
    // {
    //     var topology = new Topology
    //     {
    //         Metadata = new Metadata
    //         {
    //             CreatedAt = DateTime.UtcNow,
    //             GraphName = graphName,
    //             NumberOfNodes = NodeCount,
    //             NumberOfEdges = EdgeCount
    //         },
    //         Data = _adjacencyList
    //             .SelectMany(kvp => kvp.Value)
    //             .ToList()
    //     };
    //
    //     return JsonSerializer.Serialize(topology, new JsonSerializerOptions { WriteIndented = true });
    // }

    // public static LightningFastGraph DeserializeFromJson(string json)
    // {
    //     var topology = JsonSerializer.Deserialize<Topology>(json);
    //     if (topology == null)
    //         throw new ArgumentException("Invalid JSON format.");
    //
    //     var graph = new LightningFastGraph();
    //     foreach (var edge in topology.Data)
    //     {
    //         graph.AddEdge(edge.From, edge.To, edge.Scid, edge.Weight.BaseMSat, edge.Weight.ProportionalMillionths);
    //     }
    //
    //     return graph;
    // }

    public BridgeAnalysis GetBridgeAnalysis()
    {
        return BridgeDetector.Analyze(this);
    }

    public NetworkMetrics GetNetworkMetrics()
    {
        return NetworkAnalyzer.CalculateAll(this);
    }

    public double GetDensity()
    {
        return NetworkAnalyzer.CalculateDensity(this);
    }
    public int GetDiameter()
    {
        return NetworkAnalyzer.CalculateDiameter(this);
    }
    public double GetAverageDegree()
    {
        return NetworkAnalyzer.CalculateAverageOutDegree(this);
    }
    public double GetAverageLocalClusteringCoefficient()
    {
        return NetworkAnalyzer.CalculateAverageLocalClusteringCoefficient(this);
    }
    
    public double GetGlobalClusteringCoefficient()
    {
        return NetworkAnalyzer.CalculateGlobalClusteringCoefficient(this);
    }
    
    public double GetAveragePathLength()
    {
        return NetworkAnalyzer.CalculateAveragePathLength(this);
    }

    public CentralityMetrics GetCentralityMetricsAnalytically()
    {
        return CentralityAnalyzer.CalculateCentralityAnalytically(this);
    }
    
    public CentralityMetrics GetCentralityMetricsEmpirically(int? paymentSizeSats, int runs = 1_000, int? seed = null)
    {
        return CentralityAnalyzer.CalculateEmpiricalCentrality(this, runs, paymentSizeSats, seed);
    }
}
