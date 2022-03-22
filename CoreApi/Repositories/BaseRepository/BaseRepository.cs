using CoreApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoreApi.Repositories.BaseRepository;

public class BaseRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext coreDbContext;

    public DbSet<T> BaseDbSet { get; }

    public BaseRepository(CoreDbContext dbContext)
    {
        coreDbContext = dbContext;
        BaseDbSet = dbContext.Set<T>();
    }

    public async Task<bool> SaveChangesAsync() => await coreDbContext.SaveChangesAsync().ConfigureAwait(false) > 0;

    public IDbConnection GetDbConnection() => coreDbContext.Database.GetDbConnection();

    public IDbContextTransaction BeginTransaction() => coreDbContext.Database.BeginTransaction();

    public DbSet<TSource> GetDbSet<TSource>() where TSource : class
    {
        return coreDbContext.Set<TSource>();
    }

    public virtual async Task<(List<T>, int)> GetListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, bool isNoTracking = true)
    {
        var query = BaseDbSet.Where(expression).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        if (isNoTracking) query = query.AsNoTracking();

        var res = await query.ToListAsync().ConfigureAwait(false);
        var total = await query.CountAsync().ConfigureAwait(false);
        return (res, total);
    }

    public virtual async Task<T> GetOneAsync(Expression<Func<T, bool>> expression, bool isNoTracking = true)
    {
        var query = BaseDbSet.Where(expression);
        if (isNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        BaseDbSet.Add(entity);
        var res = await SaveChangesAsync().ConfigureAwait(false);
        return res ? entity : null;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        BaseDbSet.Update(entity);
        return await SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual async Task<bool> DeleteAsync(T entity)
    {
        BaseDbSet.Remove(entity);
        return await SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual async Task<bool> AddRangeAsync(IEnumerable<T> entities)
    {
        BaseDbSet.AddRange(entities);
        return await SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual async Task<bool> UpdateRangeAsync(IEnumerable<T> entities)
    {
        BaseDbSet.UpdateRange(entities);
        return await SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual async Task<bool> DeleteRangeAsync(IEnumerable<T> entities)
    {
        BaseDbSet.RemoveRange(entities);
        return await SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<int> ExecuteSqlAsync(FormattableString sql)
    {
        return await coreDbContext.Database.ExecuteSqlInterpolatedAsync(sql).ConfigureAwait(false);
    }
}
