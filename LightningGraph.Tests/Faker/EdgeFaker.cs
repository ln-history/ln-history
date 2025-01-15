using LightningGraph.Serialization;

namespace LightningGraph.Tests.Faker;

public class EdgeFaker
{
    public static Edge Create(int seed, string? from, string? to)
    {
        var faker = new Bogus.Faker<Edge>()
            .UseSeed(seed)
            .RuleFor(x => x.Scid, y => y.Random.Replace("#####x####x#"))
            .RuleFor(x => x.From, y => from ?? y.Random.AlphaNumeric(64))
            .RuleFor(x => x.To, y => to ?? y.Random.AlphaNumeric(64))
            .RuleFor(x => x.Weight, y => WeightFaker.Create(seed));
        
        return faker.Generate();
    }
}