using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoreApi.Repositories
{
    public interface IRepository<T> where T : class
    {
        public Task<bool> SaveChangesAsync();
        public IDbConnection GetDbConnection();
        public IDbContextTransaction BeginTransaction();
        public DbSet<TResult> GetDbSet<TResult>() where TResult : class;
        public Task<(List<T>, int)> GetListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, bool isNoTracking = true);
        public Task<T> GetOneAsync(Expression<Func<T, bool>> expression, bool isNoTracking = true);
        public Task<T> AddAsync(T entity);
        public Task<bool> UpdateAsync(T entity);
        public Task<bool> DeleteAsync(T entity);
        public Task<bool> AddRangeAsync(IEnumerable<T> entities);
        public Task<bool> UpdateRangeAsync(IEnumerable<T> entities);
        public Task<bool> DeleteRangeAsync(IEnumerable<T> entities);
        public Task<int> ExecuteSqlAsync(FormattableString sql);
    }
}