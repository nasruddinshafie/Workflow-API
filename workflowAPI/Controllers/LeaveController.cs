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
        private readonly ILogger<LeaveController> _logger;

        public LeaveController(
        IWorkflowService workflowService,
        ILogger<LeaveController> logger)
        {
            _workflowService = workflowService;
            _logger = logger;
        }


        /// <summary>
        /// Submit a new leave request
        /// </summary>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<ActionResult<ApiResponse<string>>> SubmitLeave(
            [FromBody] LeaveRequestDto request)
        {
            try
            {
                var parameters = new Dictionary<string, object>
            {
                { "EmployeeId", request.EmployeeId },
                { "EmployeeName", request.EmployeeName },
                { "StartDate", request.StartDate.ToString("yyyy-MM-dd") },
                { "EndDate", request.EndDate.ToString("yyyy-MM-dd") },
                { "TotalDays", (request.EndDate - request.StartDate).Days + 1 },
                { "LeaveType", request.LeaveType },
                { "Reason", request.Reason },
                { "SubmittedDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
            };

                var result = await _workflowService.CreateInstanceAsync(
                    "LeaveApproval",
                    request.EmployeeId,
                    parameters);

                return Ok(ApiResponse<string>.SuccessResponse(
                    result.ProcessId,
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
                    LeaveId = instance.ProcessId,
                    EmployeeId = instance.Parameters.GetValueOrDefault("EmployeeId")?.ToString() ?? "",
                    EmployeeName = instance.Parameters.GetValueOrDefault("EmployeeName")?.ToString() ?? "",
                    StartDate = DateTime.Parse(instance.Parameters.GetValueOrDefault("StartDate")?.ToString() ?? DateTime.Now.ToString()),
                    EndDate = DateTime.Parse(instance.Parameters.GetValueOrDefault("EndDate")?.ToString() ?? DateTime.Now.ToString()),
                    TotalDays = int.Parse(instance.Parameters.GetValueOrDefault("TotalDays")?.ToString() ?? "0"),
                    LeaveType = instance.Parameters.GetValueOrDefault("LeaveType")?.ToString() ?? "",
                    Reason = instance.Parameters.GetValueOrDefault("Reason")?.ToString() ?? "",
                    CurrentState = instance.StateName,
                    SchemeVersion = instance.SchemeCode,
                    CreatedDate = instance.CreatedDate,
                    UpdatedDate = instance.UpdatedDate
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

                var message = request.Approved ? "Leave approved by manager" : "Leave rejected by manager";
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

                var message = request.Approved ? "Leave approved by HR" : "Leave rejected by HR";
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
