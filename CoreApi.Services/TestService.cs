using CoreApi.Repositories.Interfaces;
using CoreApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    public class TestService : ITestService
    {
        private ITestRepository testRepository;

        public TestService(ITestRepository _testRepository)
        {
            testRepository = _testRepository;
        }

        public string SayService(string message) => testRepository.Say(message);

        public async ValueTask<ValueTuple<dynamic, int>> EFCoreLeftJoinTestAsync(int pageIndex, int pageSize)
        {
            return await testRepository.EFCoreLeftJoinTestAsync(pageIndex, pageSize);
        }

        public async ValueTask<(IEnumerable<T>, int)> DapperPageTestAsync<T>(int pageIndex, int pageSize)
        {
            return await testRepository.DapperPageTestAsync<T>(pageIndex, pageSize);
        }
    }
}
