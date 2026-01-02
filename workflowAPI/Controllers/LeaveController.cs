using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using workflowAPI.Models.DTOs;
using workflowAPI.Models.Entities;
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
        /// Get current user's leave requests with available commands from workflow server
        /// </summary>
        [HttpGet("my-requests/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), 500)]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetMyRequests(string userId)
        {
            try
            {
                var userLeaves = await _leaveService.GetUserLeavesAsync(userId);

                var leaveList = new List<object>();

                foreach (var leave in userLeaves)
                {
                    // Get available commands from workflow server for this request
                    List<CommandResponse>? availableCommands = null;
                    if (!string.IsNullOrEmpty(leave.WorkflowProcessId))
                    {
                        try
                        {
                            var commandsResponse = await _workflowService.GetAvailableCommandsAsync(
                                leave.WorkflowProcessId,
                                userId);
                            availableCommands = commandsResponse.Commands;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to get available commands for process {ProcessId}", leave.WorkflowProcessId);
                            // Continue without commands if workflow server is unavailable
                            availableCommands = new List<CommandResponse>();
                        }
                    }

                    // Get user's leave balance for this leave type
                    var balance = await _leaveBalanceService.GetBalanceAsync(
                        leave.UserId,
                        leave.LeaveTypeCode,
                        leave.StartDate.Year);

                    var commandsList = availableCommands != null
                        ? availableCommands.Select(c => new
                        {
                            commandName = c.CommandName,
                            localizedName = c.LocalizedName
                        }).Cast<object>().ToList()
                        : new List<object>();

                    leaveList.Add(new
                    {
                        leaveRequestId = leave.LeaveRequestId,
                        employeeId = leave.UserId,
                        employeeName = leave.UserFullName,
                        leaveType = leave.LeaveTypeName,
                        leaveTypeCode = leave.LeaveTypeCode,
                        leaveTypeColor = leave.LeaveTypeColor,
                        startDate = leave.StartDate.ToString("yyyy-MM-dd"),
                        endDate = leave.EndDate.ToString("yyyy-MM-dd"),
                        totalDays = leave.TotalDays,
                        reason = leave.Reason,
                        status = leave.Status,
                        submittedDate = leave.SubmittedDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        workflowProcessId = leave.WorkflowProcessId,
                        availableCommands = commandsList,
                        balance = balance != null ? new
                        {
                            totalDays = balance.TotalDays,
                            usedDays = balance.UsedDays,
                            pendingDays = balance.PendingDays,
                            availableDays = balance.AvailableDays
                        } : null
                    });
                }

                return Ok(ApiResponse<List<object>>.SuccessResponse(leaveList, "User leave requests retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get leave requests for user {UserId}", userId);
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get pending approvals for a manager or all leave requests for HR with available commands from workflow server
        /// </summary>
        [HttpGet("pending-approvals/{approverId}")]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), 500)]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetPendingApprovals(string approverId)
        {
            try
            {
                // Check if approver is HR
                var approver = await _identityService.GetUserByIdAsync(approverId);
                if (approver == null)
                {
                    return NotFound(ApiResponse<List<object>>.ErrorResponse("Approver not found"));
                }

                List<LeaveRequestEntity> pendingLeaves;

                // If HR role, get all leave requests
                if (approver.Roles.Contains(Models.Identity.Role.HRManager))
                {
                    _logger.LogInformation("HR user {ApproverId} requesting all leave requests", approverId);
                    pendingLeaves = await _leaveService.GetPendingLeavesAsync();
                }
                else
                {
                    // For managers, get only their assigned pending approvals
                    _logger.LogInformation("Manager {ApproverId} requesting pending approvals", approverId);
                    pendingLeaves = await _leaveService.GetPendingApprovalsAsync(approverId);
                }

                var leaveList = new List<object>();

                foreach (var leave in pendingLeaves)
                {
                    // Get available commands from workflow server for this request
                    List<CommandResponse>? availableCommands = null;
                    if (!string.IsNullOrEmpty(leave.WorkflowProcessId))
                    {
                        try
                        {
                            var commandsResponse = await _workflowService.GetAvailableCommandsAsync(
                                leave.WorkflowProcessId,
                                approverId);
                            availableCommands = commandsResponse.Commands;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to get available commands for leave {LeaveRequestId}", leave.LeaveRequestId);
                        }
                    }

                    leaveList.Add(new
                    {
                        leave.Id,
                        leaveRequestId = leave.LeaveRequestId,
                        workflowProcessId = leave.WorkflowProcessId,
                        employeeId = leave.UserId,
                        employeeName = leave.User?.FullName ?? "Unknown",
                        employeeEmail = leave.User?.Email ?? "",
                        employeeDepartment = leave.User?.Department ?? "",
                        leaveType = leave.LeaveType?.Name ?? "",
                        leaveTypeCode = leave.LeaveType?.Code ?? "",
                        leaveTypeColor = leave.LeaveType?.Color,
                        startDate = leave.StartDate,
                        endDate = leave.EndDate,
                        totalDays = leave.TotalDays,
                        reason = leave.Reason,
                        status = leave.Status.ToString(),
                        currentWorkflowState = leave.CurrentWorkflowState,
                        submittedDate = leave.SubmittedDate,
                        availableCommands = availableCommands
                    });
                }

                _logger.LogInformation("Retrieved {Count} pending approvals for {ApproverId}", leaveList.Count, approverId);

                return Ok(ApiResponse<List<object>>.SuccessResponse(leaveList, $"{leaveList.Count} pending approvals found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get pending approvals for {ApproverId}", approverId);
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(ex.Message));
            }
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

                // Step 5: Get all HR users for approval
                var hrUsers = await _identityService.GetUsersByRoleAsync(Models.Identity.Role.HRManager);
                var hrApproverIds = string.Join(",", hrUsers.Select(u => u.Id));
                var hrApproverNames = string.Join(",", hrUsers.Select(u => u.FullName));

                _logger.LogInformation(
                    "Found {HRCount} HR approvers for leave request: {HRApproverNames}",
                    hrUsers.Count,
                    hrApproverNames);

                // Step 6: Create workflow instance
                var parameters = new Dictionary<string, object>
                {
                    {"LeaveRequestId", leaveRequest.LeaveRequestId },
                    { "EmployeeId", request.EmployeeId },
                    { "EmployeeName", request.EmployeeName },
                    { "StartDate", request.StartDate.ToString("yyyy-MM-dd") },
                    { "EndDate", request.EndDate.ToString("yyyy-MM-dd") },
                    { "TotalDays", totalDays },
                    { "LeaveTypeCode", request.LeaveType },
                    { "Reason", request.Reason },
                    { "ManagerIdentity", request.SelectedApproverId },
                    { "SelectedApproverName", manager.FullName },
                    { "HRApproverIdentities", hrApproverIds },
                    { "HRApproverNames", hrApproverNames },
                    { "HRApproverCount", hrUsers.Count },
                    { "SubmittedDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                var processId = await _workflowService.CreateInstanceAsync(
                    "LeaveApproval",
                    request.EmployeeId,
                    leaveRequest.LeaveRequestId,
                    parameters);

                // Step 7: Get instance info to retrieve current state
                var processInstance = await _workflowService.GetInstanceAsync(processId);

                // Step 8: Map workflow state to leave status
                var leaveStatus = MapStateToStatus(processInstance.StateName ?? "Draft");

                // Step 9: Update leave status based on workflow state
                if (leaveStatus.HasValue)
                {
                    await _leaveService.UpdateLeaveStatusAsync(leaveRequest.LeaveRequestId, leaveStatus.Value);
                }

                // Step 10: Link database record with workflow ProcessId and current state
                await _leaveService.SyncWithWorkflowAsync(
                    leaveRequest.LeaveRequestId,
                    processId,
                    processInstance.StateName ?? "Draft");

                // Write log to Workflow Server
                await _workflowService.WriteLogAsync(
                    $"Leave request submitted by {request.EmployeeName} with Manager and {hrUsers.Count} HR approver(s)",
                    new Dictionary<string, object>
                    {
                        { "LeaveRequestId", leaveRequest.LeaveRequestId },
                        { "EmployeeId", request.EmployeeId },
                        { "EmployeeName", request.EmployeeName },
                        { "LeaveType", request.LeaveType },
                        { "StartDate", request.StartDate },
                        { "EndDate", request.EndDate },
                        { "TotalDays", totalDays },
                        { "ManagerId", request.SelectedApproverId },
                        { "ManagerName", manager.FullName },
                        { "HRApproverIds", hrApproverIds },
                        { "HRApproverNames", hrApproverNames },
                        { "HRCount", hrUsers.Count },
                        { "CurrentState", processInstance.StateName },
                        { "SubmittedDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                    });

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
                    ? Models.Entities.LeaveRequestStatus.Approved
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
        /// Execute a workflow command on a leave request
        /// </summary>
        [HttpPost("{leaveId}/execute-command")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> ExecuteCommand(
            string leaveId,
            [FromBody] ExecuteCommandRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Command))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Command is required"));
                }

                if (string.IsNullOrEmpty(request.IdentityId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Identity ID is required"));
                }

                // Set the ProcessId to the leaveId if not provided
                if (string.IsNullOrEmpty(request.ProcessId))
                {
                    request.ProcessId = leaveId;
                }

                // Execute workflow command
                await _workflowService.ExecuteCommandAsync(request);

                // Write log to Workflow Server
                await _workflowService.WriteLogAsync(
                    $"Command '{request.Command}' executed on leave request {leaveId}",
                    new Dictionary<string, object>
                    {
                        { "LeaveRequestId", leaveId },
                        { "Command", request.Command },
                        { "ExecutedBy", request.IdentityId },
                        { "Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                    });

                // Get the updated leave request to check status
                var leaveRequest = await _leaveService.GetLeaveRequestAsync(leaveId);

                _logger.LogInformation(
                    "Command {Command} executed on leave request {LeaveId} by {IdentityId}",
                    request.Command, leaveId, request.IdentityId);

                return Ok(ApiResponse<object>.SuccessResponse($"Command '{request.Command}' executed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute command {Command} on leave {LeaveId}",
                    request.Command, leaveId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
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

        /// <summary>
        /// Map workflow state name to leave request status
        /// </summary>
         private LeaveRequestStatus? MapStateToStatus(string activityName)
        {
            return activityName switch
            {
                "LeaveRequestCreated" => LeaveRequestStatus.LeaveRequestCreated,

                "ManagerSigning"=> LeaveRequestStatus.ManagerSigning,
                "HRSigning" => LeaveRequestStatus.HRSigning,
                "Approved" or "final" => LeaveRequestStatus.Approved,
                "Rejected" => LeaveRequestStatus.Rejected,
                "Cancelled" => LeaveRequestStatus.Cancelled,
                _ => null
            };
        }
    }
}
