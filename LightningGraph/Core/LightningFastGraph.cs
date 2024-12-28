using System.Text.Json;
using LightningGraph.Model;
using LN_history.Data.Model;

namespace LightningGraph.Core;

public class LightningFastGraph
{
    private readonly Dictionary<string, List<Edge>> _adjacencyList = new();
    private readonly Dictionary<string, List<Edge>> _reverseAdjacencyList = new();
    private int _edgeCount = 0;

    public readonly Dictionary<string, NodeInformation> _nodeInformationDict = new();
    public readonly Dictionary<string, ChannelInformation> _edgeInformationDict = new();

    
    public int NodeCount => _adjacencyList.Count;

    public int EdgeCount => _edgeCount;

    public void AddEdge(string from, string to, string scid, long baseWeight, long proportionalWeight)
    {
        var edge = new Edge
        {
            Scid = scid,
            From = from,
            To = to,
            Weight = new Weight { BaseMSat = baseWeight, ProportionalMillionths = proportionalWeight }
        };

        if (!_adjacencyList.ContainsKey(from))
        {
            _adjacencyList[from] = new List<Edge>();
        }
        if (!_reverseAdjacencyList.ContainsKey(to))
        {
            _reverseAdjacencyList[to] = new List<Edge>();
        }

        _adjacencyList[from].Add(edge);
        _reverseAdjacencyList[to].Add(edge);
        _edgeCount++;
    }

    // Method to add a batch of nodes
    public void AddVerticesBatch(IEnumerable<NodeInformation> validNodes)
    {
        foreach (var nodeInfo in validNodes)
        {
            // Store node information in the dictionary
            _nodeInformationDict[nodeInfo.NodeId] = new NodeInformation()
            {
                NodeId = nodeInfo.NodeId,
                Features = nodeInfo.Features,
                Timestamp = nodeInfo.Timestamp,
                RgbColor = nodeInfo.RgbColor,
                Addresses = nodeInfo.Addresses
            };

            // Ensure the node exists in the adjacency list
            if (!_adjacencyList.ContainsKey(nodeInfo.NodeId))
            {
                _adjacencyList[nodeInfo.NodeId] = new List<Edge>();
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
            _edgeInformationDict[channelInfo.Scid] = new ChannelInformation()
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
        return _adjacencyList.Keys;
    }

    public (string[] Neighbors, long[] Weights) GetNeighbors(string nodeId)
    {
        if (!_adjacencyList.TryGetValue(nodeId, out var edges)) return (Array.Empty<string>(), Array.Empty<long>());
        return (
            edges.Select(e => e.To).ToArray(),
            edges.Select(e => e.Weight.BaseMSat + e.Weight.ProportionalMillionths).ToArray()
        );
    }

    public int GetDegree(string nodeId)
    {
        return _adjacencyList.ContainsKey(nodeId) ? _adjacencyList[nodeId].Count : 0;
    }

    public int GetInDegree(string nodeId)
    {
        return _reverseAdjacencyList.ContainsKey(nodeId) ? _reverseAdjacencyList[nodeId].Count : 0;
    }

    public int GetOutDegree(string nodeId)
    {
        return GetDegree(nodeId);
    }

    public string SerializeToJson(string graphName)
    {
        var topology = new Topology
        {
            Metadata = new Metadata
            {
                CreatedAt = DateTime.UtcNow,
                GraphName = graphName,
                NumberOfNodes = NodeCount,
                NumberOfEdges = EdgeCount
            },
            Data = _adjacencyList
                .SelectMany(kvp => kvp.Value)
                .ToList()
        };

        return JsonSerializer.Serialize(topology, new JsonSerializerOptions { WriteIndented = true });
    }

    public static LightningFastGraph DeserializeFromJson(string json)
    {
        var topology = JsonSerializer.Deserialize<Topology>(json);
        if (topology == null)
            throw new ArgumentException("Invalid JSON format.");

        var graph = new LightningFastGraph();
        foreach (var edge in topology.Data)
        {
            graph.AddEdge(edge.From, edge.To, edge.Scid, edge.Weight.BaseMSat, edge.Weight.ProportionalMillionths);
        }

        return graph;
    }
}
