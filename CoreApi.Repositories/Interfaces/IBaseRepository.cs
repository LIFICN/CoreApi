using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoreApi.Repositories.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        public ValueTask<bool> SaveChangesAsync();
        public IDbConnection GetDbConnection();
        public ValueTask<T> GetOneAsync(Expression<Func<T, bool>> expression, bool isNoTracking = true);
        public ValueTask<T> AddAsync(T entity);
        public ValueTask<bool> UpdateAsync(T entity);
        public ValueTask<bool> DeleteAsync(T entity);
        public ValueTask<bool> AddRangeAsync(IEnumerable<T> entities);
        public ValueTask<bool> UpdateRangeAsync(IEnumerable<T> entities);
        public ValueTask<bool> DeleteRangeAsync(IEnumerable<T> entities);
    }
}