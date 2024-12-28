using Bitcoin.Data.Datastores;
using Bitcoin.Data.Settings;
using CouchDB.Driver.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bitcoin.Data;

public static class ServiceCollectionExtension
{
    public static void AddBitcoinBlocks(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var couchDbSettings = configuration.GetSection("CouchDb");
        
        if (couchDbSettings == null)
        {
            throw new InvalidOperationException("CouchDbSettings configuration section is missing or malformed.");
        }
        
        var endpoint = couchDbSettings.GetSection("Endpoint").Value;
        var accessKey = couchDbSettings.GetSection("Username").Value;
        var secretKey = couchDbSettings.GetSection("Password").Value;
     
        serviceCollection.AddCouchContext<BitcoinBlocksCouchDbContext>(options =>
            {
                options
                    .UseEndpoint(endpoint)
                    .EnsureDatabaseExists()
                    .UseBasicAuthentication(
                        username: accessKey, 
                        password: secretKey
                    );
            }
        );

        serviceCollection.AddScoped<IBitcoinBlocksDataStore, BitcoinBlocksDataStore>();
        serviceCollection.AddScoped<IBitcoinMiningFeeDataStore, BitcoinMiningFeeDataStore>();
        serviceCollection.AddScoped<IBitcoinTimestampDataStore, BitcoinTimestampDataStore>();
        serviceCollection.AddScoped<IBitcoinBlockSubsidyDataStore, BitcoinBlockSubsidyDataStore>();
    }
}