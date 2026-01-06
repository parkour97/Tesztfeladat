using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Frontend.Models;

namespace Frontend.Data
{
    public class FrontendContext : DbContext
    {
        public FrontendContext (DbContextOptions<FrontendContext> options)
            : base(options)
        {
        }

        public DbSet<Frontend.Models.SystemUsageView> SystemUsage { get; set; } = default!;
    }
}
