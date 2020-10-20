using CoreApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreApi.Repositories.Interfaces
{
    public interface ITestRepository : IBaseRepository<TestEntity1>
    {
        string Say(string message);
        ValueTask<dynamic> EFCoreLeftJoinTestAsync();
        ValueTask<(IEnumerable<T>, int)> DapperPageTestAsync<T>(int pageIndex, int pageSize);
    }
}
