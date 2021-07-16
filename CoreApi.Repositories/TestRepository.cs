using CoreApi.Extensions;
using CoreApi.Models;
using CoreApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoreApi.Repositories
{
    public class TestRepository : BaseRepository<TestEntity1>, ITestRepository
    {
        public TestRepository(CoreDbContext dbContext) : base(dbContext)
        {
        }

        public string Say(string message) => message;

        public override async Task<(List<TestEntity1>, int)> GetListAsync(Expression<Func<TestEntity1, bool>> expression, int pageIndex, int pageSize, bool isNoTracking = true)
        {
            using var trans = base.BeginTransaction();
            var query = (from t1 in base.BaseDbSet
                         join t2 in base.GetDbSet<TestEntity2>() on t1.ID equals t2.ID
                         into temp
                         from sub in temp.DefaultIfEmpty()
                         select new TestEntity1
                         {
                             ID = t1.ID,
                             Name = sub == null ? default : sub.Name
                         });

            query = query.WhereIf(expression != null, expression);
            if (isNoTracking) query = query.AsNoTracking();

            var data = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);
            var total = await query.CountAsync().ConfigureAwait(false);
            trans.Commit();
            return (data, total);
        }
    }
}
