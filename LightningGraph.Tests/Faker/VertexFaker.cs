namespace LightningGraph.Tests.Faker;

public class VertexFaker
{
    public static string Create(int seed)
    {
        // var faker = new Bogus.Faker<Vertex>().UseSeed(seed);
        
        return Guid.NewGuid().ToString();
    }
}