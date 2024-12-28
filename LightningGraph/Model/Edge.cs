namespace LightningGraph.Model;

public class Edge
{
    public string Scid { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public Weight Weight { get; set; }
}