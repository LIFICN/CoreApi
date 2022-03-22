using CoreApi.Models;
using CoreApi.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreApi.Repositories.Interfaces;

namespace CoreApi.Services;

public class TestService : ITestServicecs
{
    private readonly ITestRepository testRepository;

    public TestService(ITestRepository _testReposiitory)
    {
        testRepository = _testReposiitory;
    }

    public async Task<(List<TestEntity1>, int)> GetListAsync(int pageIndex, int pageSize)
    {
        return await testRepository.GetListAsync(null, pageIndex, pageSize);
    }

    public string TestDI(string param) => param;
}
