using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoreApi.Repositories.Interfaces
{
    public interface IBaseRepository
    {
        public ValueTask<bool> SaveChangesAsync();
        public IDbConnection GetDbConnection();
        public ValueTask<T> GetOneAsync<T>(Expression<Func<T, bool>> expression) where T : class;
        public ValueTask<T> AddAsync<T>(T entity) where T : class;
        public ValueTask<bool> UpdateAsync<T>(T entity) where T : class;
        public ValueTask<bool> DeleteAsync<T>(T entity) where T : class;
        public ValueTask<bool> AddRangeAsync<T>(IEnumerable<T> entities) where T : class;
        public ValueTask<bool> UpdateRangeAsync<T>(IEnumerable<T> entities) where T : class;
        public ValueTask<bool> DeleteRangeAsync<T>(IEnumerable<T> entities) where T : class;
    }
}