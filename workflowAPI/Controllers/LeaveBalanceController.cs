using Microsoft.AspNetCore.Mvc;
using workflowAPI.Models.DTOs;
using workflowAPI.Models.Responses;
using workflowAPI.Services;

namespace workflowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveBalanceController : ControllerBase
    {
        private readonly ILeaveBalanceService _leaveBalanceService;
        private readonly ILogger<LeaveBalanceController> _logger;

        public LeaveBalanceController(
            ILeaveBalanceService leaveBalanceService,
            ILogger<LeaveBalanceController> logger)
        {
            _leaveBalanceService = leaveBalanceService;
            _logger = logger;
        }

        /// <summary>
        /// Get all leave balances for a user
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveBalanceDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveBalanceDto>>), 500)]
        public async Task<ActionResult<ApiResponse<List<LeaveBalanceDto>>>> GetUserBalances(
            string userId,
            [FromQuery] int? year = null)
        {
            try
            {
                _logger.LogInformation("Getting leave balances for user {UserId}, year {Year}", userId, year);
                var balances = await _leaveBalanceService.GetUserBalancesAsync(userId, year);
                return Ok(ApiResponse<List<LeaveBalanceDto>>.SuccessResponse(balances));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get leave balances for user {UserId}", userId);
                return StatusCode(500, ApiResponse<List<LeaveBalanceDto>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get specific leave balance for a user by leave type code
        /// </summary>
        [HttpGet("user/{userId}/type/{leaveTypeCode}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceDto>), 404)]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceDto>), 500)]
        public async Task<ActionResult<ApiResponse<LeaveBalanceDto>>> GetBalance(
            string userId,
            string leaveTypeCode,
            [FromQuery] int? year = null)
        {
            try
            {
                _logger.LogInformation("Getting leave balance for user {UserId}, type {LeaveTypeCode}, year {Year}",
                    userId, leaveTypeCode, year);

                var balance = await _leaveBalanceService.GetBalanceAsync(userId, leaveTypeCode, year);

                if (balance == null)
                {
                    return NotFound(ApiResponse<LeaveBalanceDto>.ErrorResponse(
                        $"Leave balance not found for user '{userId}' and leave type '{leaveTypeCode}'"));
                }

                return Ok(ApiResponse<LeaveBalanceDto>.SuccessResponse(balance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get leave balance for user {UserId}, type {LeaveTypeCode}",
                    userId, leaveTypeCode);
                return StatusCode(500, ApiResponse<LeaveBalanceDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Check if user has sufficient balance for a leave request
        /// </summary>
        [HttpGet("user/{userId}/check-balance")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> CheckSufficientBalance(
            string userId,
            [FromQuery] string leaveTypeCode,
            [FromQuery] int year,
            [FromQuery] decimal daysRequired)
        {
            try
            {
                _logger.LogInformation("Checking balance for user {UserId}, type {LeaveTypeCode}, days {Days}",
                    userId, leaveTypeCode, daysRequired);

                var hasSufficient = await _leaveBalanceService.HasSufficientBalanceAsync(
                    userId, leaveTypeCode, year, daysRequired);

                return Ok(ApiResponse<bool>.SuccessResponse(
                    hasSufficient,
                    hasSufficient ? "Sufficient balance available" : "Insufficient balance"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check balance for user {UserId}", userId);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get available days summary for all leave types
        /// </summary>
        [HttpGet("user/{userId}/available")]
        [ProducesResponseType(typeof(ApiResponse<Dictionary<string, decimal>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<Dictionary<string, decimal>>), 500)]
        public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetAvailableDays(
            string userId,
            [FromQuery] int? year = null)
        {
            try
            {
                _logger.LogInformation("Getting available days for user {UserId}, year {Year}", userId, year);

                var balances = await _leaveBalanceService.GetUserBalancesAsync(userId, year);
                var availableDays = balances.ToDictionary(
                    b => b.LeaveTypeCode,
                    b => b.AvailableDays);

                return Ok(ApiResponse<Dictionary<string, decimal>>.SuccessResponse(availableDays));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available days for user {UserId}", userId);
                return StatusCode(500, ApiResponse<Dictionary<string, decimal>>.ErrorResponse(ex.Message));
            }
        }
    }
}
