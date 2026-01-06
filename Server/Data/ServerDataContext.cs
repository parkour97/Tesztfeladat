using Microsoft.EntityFrameworkCore;
using Server.Model;

namespace Server.Data
{
    public class ServerDataContext : DbContext
    {
        public DbSet<User> User { get; set; }

        public ServerDataContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}
