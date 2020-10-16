using System.Threading.Tasks;

namespace CoreApi.Repositories.Interfaces
{
    public interface ITestRepository : IBaseRepository
    {
        string Say(string message);
        ValueTask<dynamic> EFCoreLeftJoinTestAsync();
    }
}
