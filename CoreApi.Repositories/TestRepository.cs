using CoreApi.Models;
using CoreApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi.Repositories
{
    public class TestRepository : BaseRepository, ITestRepository
    {
        private readonly CoreDbContext coreDbContext;

        public TestRepository(CoreDbContext dbContext) : base(dbContext)
        {
            coreDbContext = dbContext;
        }

        public string Say(string message) => message;

        public async ValueTask<dynamic> EFCoreLeftJoinTestAsync()
        {
            return await (from t2 in coreDbContext.Set<TestEntity2>()
                          join t1 in coreDbContext.Set<TestEntity1>() on t2.ID equals t1.ID
                          into temp
                          from sub in temp.DefaultIfEmpty()
                          select new TestEntity2
                          {
                              ID = t2.ID,
                              Name = sub == null ? default : sub.Name
                          }
                        )
                        .AsNoTracking()
                        .ToListAsync()
                        .ConfigureAwait(false);
        }
    }
}
