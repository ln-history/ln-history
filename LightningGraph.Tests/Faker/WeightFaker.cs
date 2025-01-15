using LightningGraph.Serialization;

namespace LightningGraph.Tests.Faker;

public class WeightFaker
{
    public static Weight Create(int seed)
    {
        var faker = new Bogus.Faker<Weight>()
            .UseSeed(seed)
            .RuleFor(x => x.BaseMSat, y => y.Random.UInt(0, 100_000))
            .RuleFor(x => x.ProportionalMillionths, y => y.Random.UInt(0, 10_000_000));
        
        return faker.Generate();
    }
}