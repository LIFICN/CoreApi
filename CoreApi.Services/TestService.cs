using CoreApi.Repositories.Interfaces;
using CoreApi.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    public class TestService : ITestService
    {
        private readonly ITestRepository testRepository;

        public TestService(ITestRepository _testRepository)
        {
            testRepository = _testRepository;
        }

        public string SayService(string message) => testRepository.Say(message);

        public async Task<ValueTuple<dynamic, int>> GetListAsync(int pageIndex, int pageSize)
        {
            return await testRepository.GetListAsync(null, pageIndex, pageSize, true);
        }
    }
}
