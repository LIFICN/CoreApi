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
    public class BaseRepository : IBaseRepository
    {
        private readonly DbContext coreDbContext;

        public BaseRepository(CoreDbContext dbContext)
        {
            coreDbContext = dbContext;
        }

        public async ValueTask<bool> SaveChangesAsync() => await coreDbContext.SaveChangesAsync().ConfigureAwait(false) > 0 ? true : false;

        public IDbConnection GetDbConnection() => coreDbContext.Database.GetDbConnection();

        public virtual async ValueTask<T> GetOneAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return await coreDbContext.Set<T>().Where(expression).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public virtual async ValueTask<T> AddAsync<T>(T entity) where T : class
        {
            coreDbContext.Set<T>().Add(entity);
            var res = await SaveChangesAsync().ConfigureAwait(false);
            return res ? entity : null;
        }

        public virtual async ValueTask<bool> UpdateAsync<T>(T entity) where T : class
        {
            coreDbContext.Set<T>().Update(entity);
            return await SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async ValueTask<bool> DeleteAsync<T>(T entity) where T : class
        {
            coreDbContext.Set<T>().Remove(entity);
            return await SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async ValueTask<bool> AddRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            coreDbContext.Set<T>().AddRange(entities);
            return await SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async ValueTask<bool> UpdateRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            coreDbContext.Set<T>().UpdateRange(entities);
            return await SaveChangesAsync().ConfigureAwait(false);
        }

        public virtual async ValueTask<bool> DeleteRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            coreDbContext.Set<T>().RemoveRange(entities);
            return await SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
