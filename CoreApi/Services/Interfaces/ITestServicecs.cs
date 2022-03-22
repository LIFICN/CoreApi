using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using CoreApi.Models;

namespace CoreApi.Services.Interfaces;

public interface ITestServicecs
{
    public string TestDI(string param);
    public Task<ValueTuple<List<TestEntity1>, int>> GetListAsync(int pageIndex, int pageSize);
}
