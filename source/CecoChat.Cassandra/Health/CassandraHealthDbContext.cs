using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CecoChat.Cassandra.Health;

public interface ICassandraHealthDbContext : ICassandraDbContext
{ }

public class CassandraHealthDbContext : CassandraDbContext, ICassandraHealthDbContext
{
    public CassandraHealthDbContext(ILogger<CassandraHealthDbContext> logger, IOptions<CassandraOptions> options) : base(logger, options)
    { }
}
