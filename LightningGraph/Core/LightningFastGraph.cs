using LightningGraph.GraphAlgorithms.Connectivity;
using LightningGraph.GraphAlgorithms.Metrics;
using LightningGraph.Model;
using LightningGraph.Serialization;
using LN_history.Data.Model;

namespace LightningGraph.Core;

public class LightningFastGraph
{
    public Dictionary<string, List<Edge>> AdjacencyList = new();
    public Dictionary<string, List<Edge>> ReverseAdjacencyList = new();
    private int _edgeCount = 0;

    public readonly Dictionary<string, NodeInformation> NodeInformationDict = new();
    public readonly Dictionary<string, ChannelInformation> EdgeInformationDict = new();
    
    public int NodeCount => AdjacencyList.Count;

    public int EdgeCount => _edgeCount;

    private void AddEdge(string from, string to, string scid, long baseWeight, long proportionalWeight)
    {
        var edge = new Edge
        {
            Scid = scid,
            From = from,
            To = to,
            BaseMSat = baseWeight,
            ProportionalMillionths = proportionalWeight
        };

        if (!AdjacencyList.ContainsKey(from))
        {
            AdjacencyList[from] = new List<Edge>();
        }
        if (!ReverseAdjacencyList.ContainsKey(to))
        {
            ReverseAdjacencyList[to] = new List<Edge>();
        }

        AdjacencyList[from].Add(edge);
        ReverseAdjacencyList[to].Add(edge);
        _edgeCount++;
    }

    // Method to add a batch of nodes
    public void AddVerticesBatch(IEnumerable<NodeInformation> validNodes)
    {
        foreach (var nodeInfo in validNodes)
        {
            // Store node information in the dictionary
            NodeInformationDict[nodeInfo.NodeId] = new NodeInformation()
            {
                NodeId = nodeInfo.NodeId,
                Features = nodeInfo.Features,
                Timestamp = nodeInfo.Timestamp,
                RgbColor = nodeInfo.RgbColor,
                Addresses = nodeInfo.Addresses
            };

            // Ensure the node exists in the adjacency list
            if (!AdjacencyList.ContainsKey(nodeInfo.NodeId))
            {
                AdjacencyList[nodeInfo.NodeId] = new List<Edge>();
            }
        }
    }

    // Method to add a batch of edges
    public void AddEdgesBatch(IEnumerable<ChannelInformation> validChannels)
    {
        foreach (var channelInfo in validChannels)
        {
            // Assuming 'Cost' is calculated and provided in channelInfo
            var baseWeight = (int)(channelInfo.FeeBaseMSat);
            var proportionalWeight = (int)(channelInfo.FeeProportionalMillionths);

            // Add the edge to the graph
            AddEdge(channelInfo.NodeId1, channelInfo.NodeId2, channelInfo.Scid, baseWeight, proportionalWeight);

            // Store edge information in the dictionary
            EdgeInformationDict[channelInfo.Scid] = new ChannelInformation()
            {
                Scid = channelInfo.Scid,
                Features = channelInfo.Features,
                NodeId1 = channelInfo.NodeId1,
                NodeId2 = channelInfo.NodeId2,
                Timestamp = channelInfo.Timestamp,
                MessageFlags = channelInfo.MessageFlags,
                ChannelFlags = channelInfo.ChannelFlags,
                CltvExpiryDelta = channelInfo.CltvExpiryDelta,
                HtlcMinimumMSat = channelInfo.HtlcMinimumMSat,
                FeeBaseMSat = channelInfo.FeeBaseMSat,
                FeeProportionalMillionths = channelInfo.FeeProportionalMillionths,
                HtlcMaximumMSat = channelInfo.HtlcMaximumMSat,
                ChainHash = channelInfo.ChainHash
            };
        }
    }

    public IEnumerable<string> GetNodes()
    {
        return AdjacencyList.Keys;
    }

    public (string[] Neighbors, long[] Weights) GetNeighbors(string nodeId)
    {
        if (!AdjacencyList.TryGetValue(nodeId, out var edges)) return (Array.Empty<string>(), Array.Empty<long>());
        return (
            edges.Select(e => e.To).ToArray(),
            edges.Select(e => e.BaseMSat + e.ProportionalMillionths).ToArray()
        );
    }

    public int GetDegree(string nodeId)
    {
        return AdjacencyList.ContainsKey(nodeId) ? AdjacencyList[nodeId].Count : 0;
    }

    public int GetInDegree(string nodeId)
    {
        return ReverseAdjacencyList.ContainsKey(nodeId) ? ReverseAdjacencyList[nodeId].Count : 0;
    }

    public int GetOutDegree(string nodeId)
    {
        return GetDegree(nodeId);
    }
    
    public IEnumerable<Edge> GetOutgoingEdges(string nodeId)
    {
        return AdjacencyList.TryGetValue(nodeId, out var edges) ? edges : Enumerable.Empty<Edge>();
    }

    public IEnumerable<Edge> GetIncomingEdges(string nodeId)
    {
        return ReverseAdjacencyList.TryGetValue(nodeId, out var edges) ? edges : Enumerable.Empty<Edge>();
    }

    public bool HasEdge(string from, string to)
    {
        return AdjacencyList.TryGetValue(from, out var edges) && 
               edges.Any(e => e.To == to);
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
        return NetworkAnalyzer.CalculateAverageDegree(this);
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
    
    public CentralityMetrics GetCentralityMetricsEmpirically(int runs = 1_000, int? seed = null)
    {
        return CentralityAnalyzer.CalculateEmpiricalCentrality(this, runs, seed);
    }
}
