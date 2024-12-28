using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using LightningGraph.GraphAlgorithms.Connectivity;
using LightningGraph.GraphAlgorithms.Metrics;
using LightningGraph.GraphAlgorithms.PathFinding;
using LightningGraph.Implementations;
using LightningGraph.Model;

namespace LightningGraph.Core;

public class FastLightningGraph : IFastLightningGraph, IDisposable
{
    // Core data structures
    private int[][] _adjacencyArray;
    private int[] _adjacencyCount;
    private readonly Dictionary<int, string> _nodeIdMap;
    private readonly Dictionary<string, int> _nodeIdReverseMap;
    private readonly Dictionary<long, EdgeData> _edgeMap;
    private int[] _freeList;
    private int _freeCount;
    private int _nextVertexId;
    private readonly ArrayPool<int> _arrayPool;
    private readonly ReaderWriterLockSlim _lock;

    // Constants
    private const int InitialNeighborCapacity = 4;
    private const int DefaultInitialCapacity = 10_000;

    // Edge data structure
    private readonly struct EdgeData
    {
        public readonly string Scid;
        public readonly double Cost;
        public readonly bool HasCost;

        public EdgeData(string scid, double? cost)
        {
            Scid = string.Intern(scid);
            Cost = cost ?? 0;
            HasCost = cost.HasValue;
        }
    }

    public FastLightningGraph(int initialCapacity = DefaultInitialCapacity)
    {
        initialCapacity = (int)BitOperations.RoundUpToPowerOf2((uint)initialCapacity);
        _adjacencyArray = new int[initialCapacity][];
        _adjacencyCount = new int[initialCapacity];
        _nodeIdMap = new Dictionary<int, string>(initialCapacity);
        _nodeIdReverseMap = new Dictionary<string, int>(initialCapacity);
        _edgeMap = new Dictionary<long, EdgeData>(initialCapacity * 2);
        _freeList = new int[initialCapacity];
        _freeCount = 0;
        _nextVertexId = 0;
        _arrayPool = ArrayPool<int>.Shared;
        _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    }
    
    public int NextVertexId => _nextVertexId;

    public string GetEdgeData(int fromId, int toId)
    {
        var edgeKey = PackEdgeKey(fromId, toId);
        return (_edgeMap.TryGetValue(edgeKey, out var edge) ? edge.Scid : null)!;
    }

    // Utility methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long PackEdgeKey(int from, int to)
    {
        return ((long)Math.Min(from, to) << 32) | (uint)Math.Max(from, to);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UnpackEdgeKey(long packedKey, out int from, out int to)
    {
        from = (int)(packedKey >> 32);
        to = (int)(packedKey & 0xFFFFFFFF);
    }

    // Vertex Operations
    public int AddVertex(string nodeId)
    {
        _lock.EnterWriteLock();
        try
        {
            nodeId = string.Intern(nodeId);
            if (_nodeIdReverseMap.TryGetValue(nodeId, out int existingId))
            {
                return existingId;
            }

            int vertexId = _freeCount > 0 ? _freeList[--_freeCount] : _nextVertexId++;
            
            EnsureCapacity(vertexId);

            _adjacencyArray[vertexId] = _arrayPool.Rent(InitialNeighborCapacity);
            _adjacencyCount[vertexId] = 0;
            _nodeIdMap[vertexId] = nodeId;
            _nodeIdReverseMap[nodeId] = vertexId;
            
            return vertexId;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveVertex(int vertexId)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!VertexExists(vertexId)) return;

            string nodeId = _nodeIdMap[vertexId];

            // Remove all connected edges
            var neighbors = GetNeighborsArray(vertexId);
            for (int i = 0; i < _adjacencyCount[vertexId]; i++)
            {
                RemoveEdge(vertexId, neighbors[i]);
            }

            _arrayPool.Return(_adjacencyArray[vertexId]);
            _adjacencyArray[vertexId] = null;
            _adjacencyCount[vertexId] = 0;
            _nodeIdMap.Remove(vertexId);
            _nodeIdReverseMap.Remove(nodeId);
            _freeList[_freeCount++] = vertexId;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // Edge Operations
    private void AddNeighbor(int vertexId, int neighborId)
    {
        ref int count = ref _adjacencyCount[vertexId];
        int[] neighbors = _adjacencyArray[vertexId];
        
        if (count == neighbors.Length)
        {
            var newArray = _arrayPool.Rent(neighbors.Length * 2);
            Array.Copy(neighbors, newArray, count);
            _arrayPool.Return(neighbors);
            _adjacencyArray[vertexId] = newArray;
            neighbors = newArray;
        }

        neighbors[count++] = neighborId;
    }

    private void RemoveNeighbor(int vertexId, int neighborId)
    {
        int[] neighbors = _adjacencyArray[vertexId];
        ref int count = ref _adjacencyCount[vertexId];
        
        for (int i = 0; i < count; i++)
        {
            if (neighbors[i] == neighborId)
            {
                if (i < count - 1)
                {
                    neighbors[i] = neighbors[count - 1];
                }
                count--;
                return;
            }
        }
    }

    public void AddEdge(int fromId, int toId, string scid)
    {
        AddEdgeInternal(fromId, toId, scid, null);
    }

    public void AddEdgeWithCost(int fromId, int toId, string scid, double cost)
    {
        AddEdgeInternal(fromId, toId, scid, cost);
    }

    private void AddEdgeInternal(int fromId, int toId, string scid, double? cost)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!VertexExists(fromId) || !VertexExists(toId))
                throw new ArgumentException("Vertex does not exist");

            AddNeighbor(fromId, toId);
            AddNeighbor(toId, fromId);
            
            var edgeKey = PackEdgeKey(fromId, toId);
            _edgeMap[edgeKey] = new EdgeData(scid, cost);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveEdge(int fromId, int toId)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!VertexExists(fromId) || !VertexExists(toId)) return;

            RemoveNeighbor(fromId, toId);
            RemoveNeighbor(toId, fromId);
            _edgeMap.Remove(PackEdgeKey(fromId, toId));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // Batch Operations
    public void AddVerticesBatch(IEnumerable<string> nodeIds)
    {
        _lock.EnterWriteLock();
        try
        {
            if (nodeIds is ICollection<string> collection)
            {
                EnsureCapacity(_nextVertexId + collection.Count);
            }

            foreach (var nodeId in nodeIds)
            {
                AddVertex(nodeId);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void AddEdgesBatch(IEnumerable<(string fromNodeId, string toNodeId, string scid, double cost)> edges)
    {
        _lock.EnterWriteLock();
        try
        {
            if (edges is ICollection<(string, string, string, double)> collection)
            {
                EnsureEdgeCapacity(collection.Count);
            }

            foreach (var (fromNodeId, toNodeId, scid, cost) in edges)
            {
                if (_nodeIdReverseMap.TryGetValue(fromNodeId, out int fromId) && 
                    _nodeIdReverseMap.TryGetValue(toNodeId, out int toId))
                {
                    AddEdgeInternal(fromId, toId, scid, cost);
                }
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // Query Operations
    public bool VertexExists(int vertexId)
    {
        _lock.EnterReadLock();
        try
        {
            return vertexId >= 0 && vertexId < _adjacencyArray.Length && _adjacencyArray[vertexId] != null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    private int[] GetNeighborsArray(int vertexId)
    {
        return VertexExists(vertexId) ? _adjacencyArray[vertexId] : Array.Empty<int>();
    }

    public IEnumerable<int> GetNeighbors(int vertexId)
    {
        _lock.EnterReadLock();
        try
        {
            if (!VertexExists(vertexId)) yield break;
            
            var neighbors = _adjacencyArray[vertexId];
            var count = _adjacencyCount[vertexId];
            
            for (int i = 0; i < count; i++)
            {
                yield return neighbors[i];
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public double? GetEdgeCost(int fromId, int toId)
    {
        _lock.EnterReadLock();
        try
        {
            var edgeKey = PackEdgeKey(fromId, toId);
            return _edgeMap.TryGetValue(edgeKey, out var edge) && edge.HasCost ? edge.Cost : null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    // Capacity Management
    private void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity >= _adjacencyArray.Length)
        {
            int newSize = (int)BitOperations.RoundUpToPowerOf2((uint)(requiredCapacity + 1));
            Array.Resize(ref _adjacencyArray, newSize);
            Array.Resize(ref _adjacencyCount, newSize);
            Array.Resize(ref _freeList, newSize);
        }
    }

    private void EnsureEdgeCapacity(int additionalEdges)
    {
        if (_edgeMap.Count + additionalEdges > _edgeMap.EnsureCapacity(_edgeMap.Count + additionalEdges))
        {
            _edgeMap.EnsureCapacity((_edgeMap.Count + additionalEdges) * 2);
        }
    }

    // Properties
    public int VertexCount => _nodeIdMap.Count;
    public int EdgeCount => _edgeMap.Count;
    
    public IEnumerable<int> Vertices
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _nodeIdMap.Keys.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public IEnumerable<(int From, int To)> Edges
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                foreach (var edgeKey in _edgeMap.Keys)
                {
                    UnpackEdgeKey(edgeKey, out int from, out int to);
                    if (from < to)
                    {
                        yield return (from, to);
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    // IDisposable implementation
    public void Dispose()
    {
        _lock.Dispose();
        for (int i = 0; i < _adjacencyArray.Length; i++)
        {
            if (_adjacencyArray[i] != null)
            {
                _arrayPool.Return(_adjacencyArray[i]);
            }
        }
    }

    // Helper methods for external access
    public bool TryGetInternalId(string nodeId, out int internalId)
    {
        _lock.EnterReadLock();
        try
        {
            return _nodeIdReverseMap.TryGetValue(nodeId, out internalId);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public string GetNodeId(int internalId)
    {
        _lock.EnterReadLock();
        try
        {
            return _nodeIdMap[internalId];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    // Analysis methods (implement as needed)
    // public NetworkMetrics AnalyzeNetwork() => NetworkAnalyzer.Calculate(this);
    public BridgeAnalysis AnalyzeBridges() => BridgeDetector.Analyze(this);
    public double[,] CalculateAllPairsShortestPaths() => FloydWarshall.CalculateWeighted(this, GetEdgeCost);
    public CentralityMetrics AnalyzeCentrality() => CentralityAnalyzer.CalculateAnalyticalCentrality(this);
    public CentralityMetrics AnalyzeCentralityMonteCarlo(int runs = 1_000, int? seed = null) => CentralityAnalyzer.CalculateEmpiricalCentrality(this, runs);
    public double CompareCentralityMethods(int runs = 1_000, int? seed = null) => CentralityAnalyzer.CalculateCentralityCorrelation(this, runs);
}