using CoreApi.Extensions;
using CoreApi.Models;
using CoreApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi.Repositories
{
    public class TestRepository : BaseRepository<TestEntity1>, ITestRepository
    {
        private readonly CoreDbContext coreDbContext;

        public TestRepository(CoreDbContext dbContext) : base(dbContext)
        {
            coreDbContext = dbContext;
        }

        public string Say(string message) => message;

        public async ValueTask<dynamic> EFCoreLeftJoinTestAsync()
        {
            return await (from t1 in base.BaseDbSet
                          join t2 in coreDbContext.Set<TestEntity2>() on t1.ID equals t2.ID
                          into temp
                          from sub in temp.DefaultIfEmpty()
                          select new TestEntity2
                          {
                              ID = t1.ID,
                              Name = sub == null ? default : sub.Name
                          }
                        )
                        .AsNoTracking()
                        .ToListAsync()
                        .ConfigureAwait(false);
        }

        public async ValueTask<(IEnumerable<T>, int)> DapperPageTestAsync<T>(int pageIndex, int pageSize)
        {
            return await base.GetDbConnection().PageAsync<T>(p =>
            {
                p.TableName = new string[] { "test1", "test2" };
                p.KeyColumn = "id";
                p.PageIndex = pageIndex;
                p.PageSize = pageSize;
                p.On = new string[] { "test1.id=test2.id" };
                p.Column = "test1.id,test2.name";
            }).ConfigureAwait(false);
        }
    }
}
