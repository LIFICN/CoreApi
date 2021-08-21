using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;

namespace CoreApi.Repositories
{
    public class CoreDbContext : DbContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var models = Assembly.Load("CoreApi.Models").GetTypes()
                       .Where(d => d.CustomAttributes.FirstOrDefault(
                           x => x.AttributeType.Namespace.Contains("System.ComponentModel")) != null && d.IsClass);

            foreach (var item in models)
            {
                modelBuilder.Model.AddEntityType(item);
            }
        }
    }
}
