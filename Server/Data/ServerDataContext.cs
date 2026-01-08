using Microsoft.EntityFrameworkCore;
using Server.Model.Entity;

namespace Server.Data
{
    public class ServerDataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserExp> UsersExp { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<DeviceExp> DeviceExp { get; set; }
        public DbSet<DeviceParam> DeviceParam { get; set; }
        public DbSet<DeviceParamExp> DeviceParamExp { get; set; }
        public DbSet<SystemUsage> SystemUsage { get; set; }
        public DbSet<SystemUsageExp> SystemUsageExp { get; set; }
        public DbSet<Log> Logs { get; set; }

        public ServerDataContext(DbContextOptions options) : base(options)
        {
            
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<SystemUsage>()
        //        .Property(e => e.Created)
        //        .HasConversion(
        //            v => v.ToUniversalTime(),
        //            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        //    modelBuilder.Entity<Log>()
        //        .Property(e => e.Timestamp)
        //        .HasConversion(
        //            v => v.ToUniversalTime(),
        //            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        //}
    }
}
