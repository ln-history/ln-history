namespace LightningGraph.Model;

public record CentralityMetrics
{
    public Dictionary<int, double> BetweennessCentrality { get; init; }
    public double AverageCentrality { get; init; }
    public int MostCentralVertex { get; init; }
    public int Runs { get; init; } // For Monte Carlo results
}