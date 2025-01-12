namespace LightningGraph.Model;

public record BridgeAnalysis
{
    public int BridgeCount { get; init; }
    public List<(string From, string To)> Bridges { get; init; }
    public int ComponentsAfterRemoval { get; init; }
    public double CriticalityScore { get; init; }
}