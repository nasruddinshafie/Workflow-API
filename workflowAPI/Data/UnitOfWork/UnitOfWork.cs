using Microsoft.EntityFrameworkCore.Storage;
using workflowAPI.Data.Repositories;

namespace workflowAPI.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WorkflowDbContext _context;
        private IDbContextTransaction? _transaction;

        public IUserRepository Users { get; }
        public ILeaveRepository Leaves { get; }
        public ILeaveBalanceRepository LeaveBalances { get; }

        public UnitOfWork(
            WorkflowDbContext context,
            IUserRepository userRepository,
            ILeaveRepository leaveRepository,
            ILeaveBalanceRepository leaveBalanceRepository)
        {
            _context = context;
            Users = userRepository;
            Leaves = leaveRepository;
            LeaveBalances = leaveBalanceRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
