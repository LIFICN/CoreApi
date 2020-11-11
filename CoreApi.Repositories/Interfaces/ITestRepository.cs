using CoreApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreApi.Repositories.Interfaces
{
    public interface ITestRepository : IBaseRepository<TestEntity1>
    {
        string Say(string message);
        ValueTask<ValueTuple<dynamic, int>> EFCoreLeftJoinTestAsync(int pageIndex, int pageSize);
        ValueTask<(IEnumerable<T>, int)> DapperPageTestAsync<T>(int pageIndex, int pageSize);
    }
}
