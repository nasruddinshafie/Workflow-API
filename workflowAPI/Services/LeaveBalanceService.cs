using workflowAPI.Data.UnitOfWork;
using workflowAPI.Models.DTOs;

namespace workflowAPI.Services
{
    public class LeaveBalanceService : ILeaveBalanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LeaveBalanceService> _logger;

        public LeaveBalanceService(
            IUnitOfWork unitOfWork,
            ILogger<LeaveBalanceService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<LeaveBalanceDto>> GetUserBalancesAsync(string userId, int? year = null)
        {
            var currentYear = year ?? DateTime.UtcNow.Year;
            var balances = await _unitOfWork.LeaveBalances.GetUserBalancesAsync(userId, currentYear);

            return balances.Select(b => new LeaveBalanceDto
            {
                LeaveTypeCode = b.LeaveType.Code,
                LeaveTypeName = b.LeaveType.Name,
                Year = b.Year,
                TotalDays = b.TotalDays,
                UsedDays = b.UsedDays,
                PendingDays = b.PendingDays,
                AvailableDays = b.AvailableDays,
                CarryForwardDays = b.CarryForwardDays ?? 0,
                Color = b.LeaveType.Color
            }).ToList();
        }

        public async Task<LeaveBalanceDto?> GetBalanceAsync(string userId, string leaveTypeCode, int? year = null)
        {
            var currentYear = year ?? DateTime.UtcNow.Year;
            var balance = await _unitOfWork.LeaveBalances.GetBalanceByCodeAsync(userId, leaveTypeCode, currentYear);

            if (balance == null)
                return null;

            return new LeaveBalanceDto
            {
                LeaveTypeCode = balance.LeaveType.Code,
                LeaveTypeName = balance.LeaveType.Name,
                Year = balance.Year,
                TotalDays = balance.TotalDays,
                UsedDays = balance.UsedDays,
                PendingDays = balance.PendingDays,
                AvailableDays = balance.AvailableDays,
                CarryForwardDays = balance.CarryForwardDays ?? 0,
                Color = balance.LeaveType.Color
            };
        }

        public async Task<bool> HasSufficientBalanceAsync(string userId, string leaveTypeCode, int year, decimal daysRequired)
        {
            var balance = await _unitOfWork.LeaveBalances.GetBalanceByCodeAsync(userId, leaveTypeCode, year);
            if (balance == null)
            {
                _logger.LogWarning("Leave balance not found for user {UserId}, leave type {LeaveTypeCode}, year {Year}",
                    userId, leaveTypeCode, year);
                return false;
            }

            var hasSufficient = balance.AvailableDays >= daysRequired;

            _logger.LogInformation(
                "Balance check for user {UserId}, leave type {LeaveTypeCode}: Available={Available}, Required={Required}, Sufficient={HasSufficient}",
                userId, leaveTypeCode, balance.AvailableDays, daysRequired, hasSufficient);

            return hasSufficient;
        }

        public async Task ReservePendingDaysAsync(string userId, string leaveTypeCode, int year, decimal days)
        {
            var balance = await _unitOfWork.LeaveBalances.GetBalanceByCodeAsync(userId, leaveTypeCode, year);
            if (balance == null)
            {
                throw new InvalidOperationException(
                    $"Leave balance not found for user {userId}, leave type {leaveTypeCode}, year {year}");
            }

            balance.PendingDays += days;
            balance.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.LeaveBalances.Update(balance);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Reserved {Days} pending days for user {UserId}, leave type {LeaveTypeCode}. New pending: {PendingDays}",
                days, userId, leaveTypeCode, balance.PendingDays);
        }

        public async Task ConfirmLeaveAsync(string userId, string leaveTypeCode, int year, decimal days)
        {
            var balance = await _unitOfWork.LeaveBalances.GetBalanceByCodeAsync(userId, leaveTypeCode, year);
            if (balance == null)
            {
                throw new InvalidOperationException(
                    $"Leave balance not found for user {userId}, leave type {leaveTypeCode}, year {year}");
            }

            // Move from pending to used
            balance.PendingDays -= days;
            balance.UsedDays += days;
            balance.UpdatedDate = DateTime.UtcNow;

            // Ensure pending doesn't go negative
            if (balance.PendingDays < 0)
            {
                _logger.LogWarning(
                    "Pending days went negative for user {UserId}, leave type {LeaveTypeCode}. Adjusting to 0.",
                    userId, leaveTypeCode);
                balance.PendingDays = 0;
            }

            _unitOfWork.LeaveBalances.Update(balance);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Confirmed {Days} days for user {UserId}, leave type {LeaveTypeCode}. Used: {UsedDays}, Pending: {PendingDays}",
                days, userId, leaveTypeCode, balance.UsedDays, balance.PendingDays);
        }

        public async Task ReleasePendingDaysAsync(string userId, string leaveTypeCode, int year, decimal days)
        {
            var balance = await _unitOfWork.LeaveBalances.GetBalanceByCodeAsync(userId, leaveTypeCode, year);
            if (balance == null)
            {
                throw new InvalidOperationException(
                    $"Leave balance not found for user {userId}, leave type {leaveTypeCode}, year {year}");
            }

            balance.PendingDays -= days;
            balance.UpdatedDate = DateTime.UtcNow;

            // Ensure pending doesn't go negative
            if (balance.PendingDays < 0)
            {
                _logger.LogWarning(
                    "Pending days went negative for user {UserId}, leave type {LeaveTypeCode}. Adjusting to 0.",
                    userId, leaveTypeCode);
                balance.PendingDays = 0;
            }

            _unitOfWork.LeaveBalances.Update(balance);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Released {Days} pending days for user {UserId}, leave type {LeaveTypeCode}. New pending: {PendingDays}",
                days, userId, leaveTypeCode, balance.PendingDays);
        }

        public async Task InitializeBalancesForYearAsync(int year)
        {
            _logger.LogInformation("Initializing leave balances for year {Year}", year);

            var users = await _unitOfWork.Users.FindAsync(u => u.IsActive);
            var leaveTypes = await _unitOfWork.LeaveBalances.FindAsync(lt => true);

            var count = 0;
            foreach (var user in users)
            {
                await _unitOfWork.LeaveBalances.InitializeBalancesForUserAsync(user.Id, year);
                count++;
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Initialized leave balances for {Count} users for year {Year}", count, year);
        }

      
    }
}
