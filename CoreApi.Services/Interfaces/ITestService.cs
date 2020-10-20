using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreApi.Services.Interfaces
{
    public interface ITestService
    {
        string SayService(string message);
        ValueTask<dynamic> EFCoreLeftJoinTestAsync();
        ValueTask<(IEnumerable<T>, int)> DapperPageTestAsync<T>(int pageIndex, int pageSize);
    }
}
