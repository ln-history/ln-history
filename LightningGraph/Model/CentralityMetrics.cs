namespace LightningGraph.Model;

public record CentralityMetrics
{
    public Dictionary<string, double> BetweennessCentrality { get; init; }
    public double AverageCentrality { get; init; }
    public string MostCentralVertex { get; init; }
    public int Runs { get; init; } // For Monte Carlo results
}