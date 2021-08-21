using CoreApi.Models;

namespace CoreApi.Repositories.Interfaces
{
    public interface ITestRepository : IRepository<TestEntity1>
    {
        string Say(string message);
    }
}
