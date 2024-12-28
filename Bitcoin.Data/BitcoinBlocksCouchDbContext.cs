using Bitcoin.Data.Model;
using CouchDB.Driver;
using CouchDB.Driver.Options;

namespace Bitcoin.Data;

public class BitcoinBlocksCouchDbContext : CouchContext
{
    public CouchDatabase<Block> Blocks { get; set; }
    
    public BitcoinBlocksCouchDbContext(CouchOptions<BitcoinBlocksCouchDbContext> options)
        : base(options) { }
}