using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreApi.Services.Interfaces
{
    public interface ITestService
    {
        string SayService(string message);
        Task<ValueTuple<dynamic, int>> GetListAsync(int pageIndex, int pageSize);
        Task<(IEnumerable<T>, int)> GetPageListAsync<T>(int pageIndex, int pageSize);
    }
}
