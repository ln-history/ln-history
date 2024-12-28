namespace LightningGraph.GraphAlgorithms.Common;

public class TraversalState
{
    public bool[] Visited { get; }
    public int[] Parent { get; }
    public int[] DiscoveryTime { get; }
    public int[] FinishTime { get; }
    public int Time { get; set; }

    public TraversalState(int vertexCount)
    {
        Visited = new bool[vertexCount];
        Parent = new int[vertexCount];
        DiscoveryTime = new int[vertexCount];
        FinishTime = new int[vertexCount];
        Time = 0;

        Array.Fill(Parent, -1);
    }
}