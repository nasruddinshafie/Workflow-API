using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using workflowAPI.Models.DTOs;
using workflowAPI.Models.Requests;
using workflowAPI.Models.Responses;
using workflowAPI.Services;

namespace workflowAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;
        private readonly ILeaveService _leaveService;
        private readonly ILeaveBalanceService _leaveBalanceService;
        private readonly IIdentityService _identityService;
        private readonly ILogger<LeaveController> _logger;

        public LeaveController(
        IWorkflowService workflowService,
        ILeaveService leaveService,
        ILeaveBalanceService leaveBalanceService,
        IIdentityService identityService,
        ILogger<LeaveController> logger)
        {
            _workflowService = workflowService;
            _leaveService = leaveService;
            _leaveBalanceService = leaveBalanceService;
            _identityService = identityService;
            _logger = logger;
        }


        /// <summary>
        /// Get user's manager
        /// </summary>
        [HttpGet("approvers/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<Models.Identity.User>), 200)]
        [ProducesResponseType(typeof(ApiResponse<Models.Identity.User>), 404)]
        [ProducesResponseType(typeof(ApiResponse<Models.Identity.User>), 500)]
        public async Task<ActionResult<ApiResponse<Models.Identity.User>>> GetUserManager(string userId)
        {
            try
            {
                var manager = await _identityService.GetManagerAsync(userId);
                if (manager == null)
                {
                    return NotFound(ApiResponse<Models.Identity.User>.ErrorResponse($"No manager found for user {userId}"));
                }

                return Ok(ApiResponse<Models.Identity.User>.SuccessResponse(manager, "Manager retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get manager for user {UserId}", userId);
                return StatusCode(500, ApiResponse<Models.Identity.User>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Submit a new leave request
        /// </summary>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<ActionResult<ApiResponse<string>>> SubmitLeave(
            [FromBody] LeaveRequestDto request)
        {
            try
            {
                // Step 1: Validate selected approver
                if (string.IsNullOrEmpty(request.SelectedApproverId))
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Please select an approver for your leave request."));
                }

                // Validate that selected approver is the user's manager
                var manager = await _identityService.GetManagerAsync(request.EmployeeId);
                if (manager == null)
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("You don't have a manager assigned. Please contact HR."));
                }

                if (manager.Id != request.SelectedApproverId)
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("You can only select your direct manager as approver."));
                }

                // Step 2: Validate leave request (dates, overlapping)
                var isValid = await _leaveService.ValidateLeaveRequestAsync(request);
                if (!isValid)
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Leave request validation failed. Check dates and overlapping requests."));
                }

                // Step 2: Check sufficient balance
                var totalDays = (decimal)((request.EndDate - request.StartDate).Days + 1);
                var year = request.StartDate.Year;
                var hasSufficientBalance = await _leaveBalanceService.HasSufficientBalanceAsync(
                    request.EmployeeId,
                    request.LeaveType,
                    year,
                    totalDays);

                if (!hasSufficientBalance)
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse($"Insufficient leave balance. Required: {totalDays} days"));
                }

                // Step 3: Create leave request in database
                var leaveRequest = await _leaveService.CreateLeaveRequestAsync(request);

                // Step 4: Reserve pending days in balance
                await _leaveBalanceService.ReservePendingDaysAsync(
                    request.EmployeeId,
                    request.LeaveType,
                    year,
                    totalDays);

                // Step 5: Create workflow instance
                var parameters = new Dictionary<string, object>
                {
                    { "EmployeeId", request.EmployeeId },
                    { "EmployeeName", request.EmployeeName },
                    { "StartDate", request.StartDate.ToString("yyyy-MM-dd") },
                    { "EndDate", request.EndDate.ToString("yyyy-MM-dd") },
                    { "TotalDays", totalDays },
                    { "LeaveType", request.LeaveType },
                    { "Reason", request.Reason },
                    { "ManagerIdentity", request.SelectedApproverId },
                    { "SelectedApproverName", manager.FullName },
                    { "SubmittedDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                var processId = await _workflowService.CreateInstanceAsync(
                    "LeaveApproval",
                    request.EmployeeId,
                    parameters);

                // Step 6: Link database record with workflow ProcessId
                await _leaveService.SyncWithWorkflowAsync(leaveRequest.LeaveRequestId, processId, "Pending");

                var processInstance = await _workflowService.GetInstanceAsync(processId);

                _logger.LogInformation("Leave request submitted successfully. LeaveRequestId: {LeaveRequestId}, ProcessId: {ProcessId}",
                    leaveRequest.LeaveRequestId, processId);

                return Ok(ApiResponse<GetInstanceInfoResponse>.SuccessResponse(
                    processInstance,
                    "Leave request submitted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit leave request for {EmployeeId}", request.EmployeeId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }


        /// <summary>
        /// Get leave status by ID
        /// </summary>
        [HttpGet("{leaveId}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveStatusDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<LeaveStatusDto>), 404)]
        [ProducesResponseType(typeof(ApiResponse<LeaveStatusDto>), 500)]
        public async Task<ActionResult<ApiResponse<LeaveStatusDto>>> GetLeaveStatus(string leaveId)
        {
            try
            {
                var instance = await _workflowService.GetInstanceAsync(leaveId);

                var status = new LeaveStatusDto
                {
                    //LeaveId = instance.ProcessId,
                    //EmployeeId = instance.Parameters.GetValueOrDefault("EmployeeId")?.ToString() ?? "",
                    //EmployeeName = instance.Parameters.GetValueOrDefault("EmployeeName")?.ToString() ?? "",
                    //StartDate = DateTime.Parse(instance.Parameters.GetValueOrDefault("StartDate")?.ToString() ?? DateTime.Now.ToString()),
                    //EndDate = DateTime.Parse(instance.Parameters.GetValueOrDefault("EndDate")?.ToString() ?? DateTime.Now.ToString()),
                    //TotalDays = int.Parse(instance.Parameters.GetValueOrDefault("TotalDays")?.ToString() ?? "0"),
                    //LeaveType = instance.Parameters.GetValueOrDefault("LeaveType")?.ToString() ?? "",
                    //Reason = instance.Parameters.GetValueOrDefault("Reason")?.ToString() ?? "",
                    //CurrentState = instance.StateName,
                    //SchemeVersion = instance.SchemeCode,
                    //CreatedDate = instance.CreatedDate,
                    //UpdatedDate = instance.UpdatedDate
                };

                return Ok(ApiResponse<LeaveStatusDto>.SuccessResponse(status));
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Leave request {LeaveId} not found", leaveId);
                return NotFound(ApiResponse<LeaveStatusDto>.ErrorResponse($"Leave request {leaveId} not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get leave status for {LeaveId}", leaveId);
                return StatusCode(500, ApiResponse<LeaveStatusDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Approve or reject leave request (Manager)
        /// </summary>
        [HttpPost("{leaveId}/manager-action")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> ManagerAction(
            string leaveId,
            [FromBody] LeaveApprovalDto request)
        {
            try
            {
                var command = request.Approved ? "ManagerApprove" : "ManagerReject";

                // Step 1: Execute workflow command
                await _workflowService.ExecuteCommandAsync(new ExecuteCommandRequest
                {
                    ProcessId = leaveId,
                    Command = command,
                    IdentityId = request.ApproverId,
                    Parameters = new Dictionary<string, object>
                    {
                        { "ApproverId", request.ApproverId },
                        { "ApproverName", request.ApproverName },
                        { "Comments", request.Comments },
                        { "ActionDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                    }
                });

                // Step 2: Update leave request status in database
                var newStatus = request.Approved
                    ? Models.Entities.LeaveRequestStatus.ManagerApproved
                    : Models.Entities.LeaveRequestStatus.Rejected;
                await _leaveService.UpdateLeaveStatusAsync(leaveId, newStatus);

                // Step 3: Add approval record to LeaveApprovals table
                await _leaveService.AddApprovalAsync(
                    leaveId,
                    request.ApproverId,
                    "Manager",
                    request.Approved ? Models.Entities.ApprovalAction.Approved : Models.Entities.ApprovalAction.Rejected,
                    request.Comments);

                // Step 4: If rejected, release pending days
                if (!request.Approved)
                {
                    var leaveRequest = await _leaveService.GetLeaveRequestAsync(leaveId);
                    if (leaveRequest != null)
                    {
                        await _leaveBalanceService.ReleasePendingDaysAsync(
                            leaveRequest.UserId,
                            leaveRequest.LeaveType.Code,
                            leaveRequest.StartDate.Year,
                            leaveRequest.TotalDays);
                    }
                }

                var message = request.Approved ? "Leave approved by manager" : "Leave rejected by manager";
                _logger.LogInformation("Manager action completed. LeaveId: {LeaveId}, Approved: {Approved}, ManagerId: {ManagerId}",
                    leaveId, request.Approved, request.ApproverId);

                return Ok(ApiResponse<bool>.SuccessResponse(true, message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute manager action on leave {LeaveId}", leaveId);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Approve or reject leave request (HR)
        /// </summary>
        [HttpPost("{leaveId}/hr-action")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> HRAction(
            string leaveId,
            [FromBody] LeaveApprovalDto request)
        {
            try
            {
                var command = request.Approved ? "HRApprove" : "HRReject";

                // Step 1: Execute workflow command
                await _workflowService.ExecuteCommandAsync(new ExecuteCommandRequest
                {
                    ProcessId = leaveId,
                    Command = command,
                    IdentityId = request.ApproverId,
                    Parameters = new Dictionary<string, object>
                    {
                        { "HRId", request.ApproverId },
                        { "HRName", request.ApproverName },
                        { "HRComments", request.Comments },
                        { "HRActionDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                    }
                });

                // Step 2: Update leave request status in database
                var newStatus = request.Approved
                    ? Models.Entities.LeaveRequestStatus.Approved
                    : Models.Entities.LeaveRequestStatus.Rejected;
                await _leaveService.UpdateLeaveStatusAsync(leaveId, newStatus);

                // Step 3: Add approval record to LeaveApprovals table
                await _leaveService.AddApprovalAsync(
                    leaveId,
                    request.ApproverId,
                    "HRManager",
                    request.Approved ? Models.Entities.ApprovalAction.Approved : Models.Entities.ApprovalAction.Rejected,
                    request.Comments);

                // Step 4: Update leave balance based on outcome
                var leaveRequest = await _leaveService.GetLeaveRequestAsync(leaveId);
                if (leaveRequest != null)
                {
                    if (request.Approved)
                    {
                        // Convert pending to used days
                        await _leaveBalanceService.ConfirmLeaveAsync(
                            leaveRequest.UserId,
                            leaveRequest.LeaveType.Code,
                            leaveRequest.StartDate.Year,
                            leaveRequest.TotalDays);
                    }
                    else
                    {
                        // Release pending days
                        await _leaveBalanceService.ReleasePendingDaysAsync(
                            leaveRequest.UserId,
                            leaveRequest.LeaveType.Code,
                            leaveRequest.StartDate.Year,
                            leaveRequest.TotalDays);
                    }
                }

                var message = request.Approved ? "Leave approved by HR" : "Leave rejected by HR";
                _logger.LogInformation("HR action completed. LeaveId: {LeaveId}, Approved: {Approved}, HRId: {HRId}",
                    leaveId, request.Approved, request.ApproverId);

                return Ok(ApiResponse<bool>.SuccessResponse(true, message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute HR action on leave {LeaveId}", leaveId);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }


        /// <summary>
        /// Get available actions for a leave request
        /// </summary>
        [HttpGet("{leaveId}/actions")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 500)]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetAvailableActions(
            string leaveId,
            [FromQuery] string userId)
        {
            try
            {
                var commands = await _workflowService.GetAvailableCommandsAsync(leaveId, userId);
                var actions = commands.Commands.Select(c => c.CommandName).ToList();

                return Ok(ApiResponse<List<string>>.SuccessResponse(actions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available actions for {LeaveId}", leaveId);
                return StatusCode(500, ApiResponse<List<string>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Cancel leave request (by employee)
        /// </summary>
        [HttpPost("{leaveId}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> CancelLeave(
            string leaveId,
            [FromQuery] string employeeId)
        {
            try
            {
                // Step 1: Execute workflow command
                await _workflowService.ExecuteCommandAsync(new ExecuteCommandRequest
                {
                    ProcessId = leaveId,
                    Command = "Cancel",
                    IdentityId = employeeId,
                    Parameters = new Dictionary<string, object>
                    {
                        { "CancelledBy", employeeId },
                        { "CancelledDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                        { "Reason", "Cancelled by employee" }
                    }
                });

                // Step 2: Update leave request status in database
                await _leaveService.UpdateLeaveStatusAsync(leaveId, Models.Entities.LeaveRequestStatus.Cancelled);

                // Step 3: Release pending days
                var leaveRequest = await _leaveService.GetLeaveRequestAsync(leaveId);
                if (leaveRequest != null)
                {
                    await _leaveBalanceService.ReleasePendingDaysAsync(
                        leaveRequest.UserId,
                        leaveRequest.LeaveType.Code,
                        leaveRequest.StartDate.Year,
                        leaveRequest.TotalDays);
                }

                _logger.LogInformation("Leave request cancelled. LeaveId: {LeaveId}, EmployeeId: {EmployeeId}",
                    leaveId, employeeId);

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Leave request cancelled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel leave {LeaveId}", leaveId);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }
    }
}
