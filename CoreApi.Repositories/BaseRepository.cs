using CoreApi.Models;
using CoreApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoreApi.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly DbContext coreDbContext;

        public DbSet<T> BaseDbSet { get; }

        public BaseRepository(CoreDbContext dbContext)
        {
            coreDbContext = dbContext;
            BaseDbSet = dbContext.Set<T>();
        }

        public async ValueTask<bool> SaveChangesAsync() => await coreDbContext.SaveChangesAsync().ConfigureAwait(false) > 0 ? true : false;

        public IDbConnection GetDbConnection() => coreDbContext.Database.GetDbConnection();

        public virtual async ValueTask<T> GetOneAsync(Expression<Func<T, bool>> expression, bool isNoTracking = true)
        {
            var query = BaseDbSet.Where(expression);
            if (isNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public virtual async ValueTask<T> AddAsync(T entity)
        {
            BaseDbSet.Add(entity);
            var res = await SaveChangesAsync().ConfigureAwait(false);
            return res ? entity : null;
        }

        public virtual async ValueTask<bool> UpdateAsync(T entity)
        {
            BaseDbSet.Update(entity);
            return await SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async ValueTask<bool> DeleteAsync(T entity)
        {
            BaseDbSet.Remove(entity);
            return await SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async ValueTask<bool> AddRangeAsync(IEnumerable<T> entities)
        {
            BaseDbSet.AddRange(entities);
            return await SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async ValueTask<bool> UpdateRangeAsync(IEnumerable<T> entities)
        {
            BaseDbSet.UpdateRange(entities);
            return await SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async ValueTask<bool> DeleteRangeAsync(IEnumerable<T> entities)
        {
            BaseDbSet.RemoveRange(entities);
            return await SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
