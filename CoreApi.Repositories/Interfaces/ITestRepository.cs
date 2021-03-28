using CoreApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreApi.Repositories.Interfaces
{
    public interface ITestRepository : IRepository<TestEntity1>
    {
        string Say(string message);
        Task<(IEnumerable<T>, int)> DapperPageTestAsync<T>(int pageIndex, int pageSize);
    }
}
