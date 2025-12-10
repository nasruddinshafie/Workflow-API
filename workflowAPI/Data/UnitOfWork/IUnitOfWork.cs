using workflowAPI.Data.Repositories;

namespace workflowAPI.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ILeaveRepository Leaves { get; }
        ILeaveBalanceRepository LeaveBalances { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
