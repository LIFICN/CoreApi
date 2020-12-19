using CoreApi.Extensions;
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
        public ValueTask<bool> SaveChangesAsync();
        public IDbConnection GetDbConnection();
        public ValueTask<(List<TResult>, int)> GetListAsync<TResult>(Action<DapperExtension.PageConfig> action);
        public ValueTask<(List<T>,int)> GetListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, bool isNoTracking = true);
        public ValueTask<T> GetOneAsync(Expression<Func<T, bool>> expression, bool isNoTracking = true);
        public ValueTask<T> AddAsync(T entity);
        public ValueTask<bool> UpdateAsync(T entity);
        public ValueTask<bool> DeleteAsync(T entity);
        public ValueTask<bool> AddRangeAsync(IEnumerable<T> entities);
        public ValueTask<bool> UpdateRangeAsync(IEnumerable<T> entities);
        public ValueTask<bool> DeleteRangeAsync(IEnumerable<T> entities);
        public IDbContextTransaction BeginTransaction();
        public DbSet<TResult> GetDbSet<TResult>() where TResult : class;
    }
}