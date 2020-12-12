using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreApi.Services.Interfaces
{
    public interface ITestService
    {
        string SayService(string message);
        ValueTask<ValueTuple<dynamic, int>> GetListAsync(int pageIndex, int pageSize);
        ValueTask<(IEnumerable<T>, int)> GetPageListAsync<T>(int pageIndex, int pageSize);
    }
}
