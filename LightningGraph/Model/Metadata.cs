namespace LightningGraph.Model;

public class Metadata
{
    public DateTime CreatedAt { get; set; }
    public string GraphName { get; set; }
    public int NumberOfNodes { get; set; }
    public int NumberOfEdges { get; set; }
}