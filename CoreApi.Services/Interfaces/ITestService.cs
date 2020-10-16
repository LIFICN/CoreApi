using System.Threading.Tasks;

namespace CoreApi.Services.Interfaces
{
    public interface ITestService
    {
        string SayService(string message);
        ValueTask<dynamic> EFCoreLeftJoinTestAsync();
    }
}
