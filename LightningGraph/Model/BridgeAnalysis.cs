namespace LightningGraph.Model;

public record BridgeAnalysis
{
    public int BridgeCount { get; init; }
    public List<(int From, int To)> Bridges { get; init; }
    public int ComponentsAfterRemoval { get; init; }
    public double CriticalityScore { get; init; }
}