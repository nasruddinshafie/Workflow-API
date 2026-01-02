using Azure.Core;
using System.Text.Json;
using System.Text.Json.Nodes;
using workflowAPI.Data.UnitOfWork;
using workflowAPI.Models.Callbacks;
using workflowAPI.Models.Callbacks.Requests;
using workflowAPI.Models.Callbacks.Responses;
using workflowAPI.Models.Entities;

namespace workflowAPI.Services.Callback.ActionHandler
{
    public class LeaveApprovalActionHandler : IActionHandler
    {
        private readonly ILogger<LeaveApprovalActionHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILeaveBalanceService _leaveBalanceService;

        public string WorkflowType => "LeaveApproval";

        public LeaveApprovalActionHandler(
            ILogger<LeaveApprovalActionHandler> logger,
            IUnitOfWork unitOfWork,
            ILeaveBalanceService leaveBalanceService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _leaveBalanceService = leaveBalanceService;
        }

        public bool CanHandle(string workflowType) => workflowType == WorkflowType;


        public async Task<ExecuteActionResponse> ExecuteActionAsync(ExecuteActionRequest callback)
        {
            _logger.LogInformation(
                "Executing leave action {ActionName}",
                callback.Name);

            try
            {
                // Parse ProcessInstance from object
                var processInstanceJson = JsonSerializer.Serialize(callback.ProcessInstance);
                var processInstance = JsonSerializer.Deserialize<ProcessInstance>(processInstanceJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (processInstance == null)
                {
                    _logger.LogError("Failed to deserialize ProcessInstance");
                    return new ExecuteActionResponse
                    {
                        Success = false,
                        Code = "ERROR",
                        Messages = new List<string> { "Failed to deserialize ProcessInstance" },
                        Data = new ExecuteActionResponseData
                        {
                            UpdatedParameters = new Dictionary<string, object>()
                        }
                    };
                }

                var updatedParameters = new Dictionary<string, object>
                {
                    { "LastActionExecuted", callback.Name },
                    { "ActionTimestamp", DateTime.UtcNow }
                };

                // Handle different actions
                switch (callback.Name)
                {
                    case "Submit":
                        await HandleSubmitAsync(processInstance, updatedParameters);
                        break;

                    case "Approve":
                        _logger.LogInformation("Approve action executed");
                        updatedParameters["Status"] = "Approved";
                        break;

                    case "Reject":

                        await HandleRejectAsync(processInstance);

                        _logger.LogInformation("Reject action executed");
                        updatedParameters["Status"] = "Rejected";
                        break;

                    case "Cancel":
                        await HandleCancelAsync(processInstance);
                        break;


                    case "Request More Info":
                        _logger.LogInformation("Request More Info action executed");
                        updatedParameters["Status"] = "PendingInfo";
                        break;

                    default:
                        _logger.LogWarning("Unknown action: {ActionName}", callback.Name);
                        break;
                }

                var response = new ExecuteActionResponse
                {
                    Success = true,
                    Data = new ExecuteActionResponseData
                    {
                        UpdatedParameters = updatedParameters
                    }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action {ActionName}", callback.Name);
                return new ExecuteActionResponse
                {
                    Success = false,
                    Messages = new List<string> { ex.Message },
                    Data = new ExecuteActionResponseData
                    {
                        UpdatedParameters = new Dictionary<string, object>()
                    }
                };
            }
        }


        private async Task HandleCancelAsync(ProcessInstance processInstance)
        {
            _logger.LogInformation("Handling cancel action for process {ProcessId}", processInstance.Id);

            try
            {
                // Get LeaveRequestId from ProcessParameters
                var leaveRequestId = GetParameterValue(processInstance.ProcessParameters, "LeaveRequestId");
                var employeeId = GetParameterValue(processInstance.ProcessParameters, "EmployeeId");
                var leaveTypeId = GetParameterValue(processInstance.ProcessParameters, "LeaveTypeCode");
                var TotalDays = Convert.ToDecimal(GetParameterValue(processInstance.ProcessParameters, "TotalDays"));
                var Year = GetParameterValue(processInstance.ProcessParameters, "Year");


                await _leaveBalanceService.ReleasePendingDaysAsync(employeeId, leaveTypeId, 2025, TotalDays);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Reject action");
                throw;

            }
        }

        private async Task HandleRejectAsync(ProcessInstance processInstance)
        {
            _logger.LogInformation("Handling Reject action for process {ProcessId}", processInstance.Id);

            try
            {
                // Get LeaveRequestId from ProcessParameters
                var leaveRequestId = GetParameterValue(processInstance.ProcessParameters, "LeaveRequestId");
                var employeeId = GetParameterValue(processInstance.ProcessParameters, "EmployeeId");
                var leaveTypeId = GetParameterValue(processInstance.ProcessParameters, "LeaveTypeId");
                var TotalDays = Convert.ToDecimal( GetParameterValue(processInstance.ProcessParameters, "TotalDays"));
                var Year = GetParameterValue(processInstance.ProcessParameters, "Year");

                await _leaveBalanceService.ReleasePendingDaysAsync(employeeId,leaveTypeId,2025,TotalDays);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Reject action");
                throw;

            }
        }

        private async Task HandleSubmitAsync(ProcessInstance processInstance, Dictionary<string, object> updatedParameters)
        {
            _logger.LogInformation("Handling Submit action for process {ProcessId}", processInstance.Id);

            try
            {
                // Get LeaveRequestId from ProcessParameters
                var leaveRequestId = GetParameterValue(processInstance.ProcessParameters, "LeaveRequestId");

                if (string.IsNullOrEmpty(leaveRequestId))
                {
                    _logger.LogWarning("LeaveRequestId not found in ProcessParameters");
                    return;
                }

                // Get leave request from database
                var leaveRequest = await _unitOfWork.Leaves.GetByLeaveRequestIdAsync(leaveRequestId);

                if (leaveRequest == null)
                {
                    _logger.LogWarning("Leave request not found: {LeaveRequestId}", leaveRequestId);
                    return;
                }


                // Update status from Draft to Pending

                //var newStatus = MapActivityToStatus(processInstance.ActivityName);

                //leaveRequest.Status = newStatus.Value;
                //leaveRequest.SubmittedDate = DateTime.UtcNow;

                //_unitOfWork.Leaves.Update(leaveRequest);
                //await _unitOfWork.SaveChangesAsync();

                //_logger.LogInformation(
                //    $"Leave request {leaveRequest.Id} status updated to {newStatus.Value}",
                //    leaveRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Submit action");
                throw;
            }
        }

        private string? GetParameterValue(Dictionary<string, object>? parameters, string key)
        {
            if (parameters == null || !parameters.ContainsKey(key))
                return null;

            return parameters[key]?.ToString();
        }



        public async Task<GetActionResponse> GetActionsAsync(string schemeCode)
        {
            _logger.LogInformation(
                "Getting leave approval actions for scheme code {Scheme Code}",
                 schemeCode);


            var actions = new GetActionResponse
            {
                Data = new List<string> { "Submit","Approve", "Reject", "Request More Info", "Cancel"}
            };

            actions.Success = true;

            return await Task.FromResult(actions);
        }

    }
}
