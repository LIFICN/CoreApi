using CoreApi.Repositories.Interfaces;
using CoreApi.Services.Interfaces;
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

        public async ValueTask<dynamic> EFCoreLeftJoinTestAsync() => await testRepository.EFCoreLeftJoinTestAsync();
    }
}
