using System.Text.Json;
using workflowAPI.Models.Callbacks;
using workflowAPI.Models.Callbacks.Requests;
using workflowAPI.Models.Callbacks.Responses;
using workflowAPI.Services.Callback.ActionHandler;

namespace workflowAPI.Services.Callback.ConditionHandler
{
    public class LeaveApprovalConditionHandler : IConditionHandler
    {
        private readonly ILogger<LeaveApprovalActionHandler> _logger;

        public LeaveApprovalConditionHandler(ILogger<LeaveApprovalActionHandler> logger)
        {
            _logger = logger;
        }

        public string WorkflowType => "LeaveApproval";

        public bool CanHandle(string workflowType) => workflowType == WorkflowType;

        public Task<ExecuteConditionResponse> ExecuteConditionAsync(ExecuteConditionRequest callback)
        {
            _logger.LogInformation(
                "Executing condition: {ConditionName}",
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
                    return Task.FromResult(new ExecuteConditionResponse
                    {
                        Data = false,
                        Success = false,
                        Code = "ERROR",
                        Messages = new List<string> { "Failed to deserialize ProcessInstance" }
                    });
                }

                // Get leave status from ProcessParameters
                var leaveStatus = GetParameterValue(processInstance.ProcessParameters, "Status") ?? "Unknown";
                var requiresManagerApproval = GetParameterValue(processInstance.ProcessParameters, "RequiresManagerApproval") ?? "false";
                var currentState = processInstance.StateName ?? "Unknown";
                var totalDaysStr = GetParameterValue(processInstance.ProcessParameters, "TotalDays") ?? "0";

                // Parse total days
                decimal totalDays = 0;
                if (!decimal.TryParse(totalDaysStr, out totalDays))
                {
                    _logger.LogWarning("Failed to parse TotalDays value: {TotalDaysStr}", totalDaysStr);
                }

                _logger.LogInformation(
                    "Evaluating condition {ConditionName} for leave with Status: {Status}, State: {State}, TotalDays: {TotalDays}",
                    callback.Name,
                    leaveStatus,
                    currentState,
                    totalDays);

                bool conditionResult = callback.Name switch
                {
                    "IsLeaveApproved" => leaveStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase) ||
                                         currentState.Equals("Approved", StringComparison.OrdinalIgnoreCase),

                    "IsLeaveRejected" => leaveStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase) ||
                                         currentState.Equals("Rejected", StringComparison.OrdinalIgnoreCase),

                    "IsLeavePending" => leaveStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                                        currentState.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                                        currentState.Equals("Draft", StringComparison.OrdinalIgnoreCase),

                    "IsLeaveNeedManagerApproval" => totalDays > 3,

                    _ => false
                };

                _logger.LogInformation(
                    "Condition {ConditionName} evaluated to: {Result}",
                    callback.Name,
                    conditionResult);

                return Task.FromResult(new ExecuteConditionResponse
                {
                    Data = conditionResult,
                    Success = true,
                    Code = "SUCCESS",
                    Messages = new List<string> { $"Condition '{callback.Name}' evaluated successfully" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing condition {ConditionName}", callback.Name);
                return Task.FromResult(new ExecuteConditionResponse
                {
                    Data = false,
                    Success = false,
                    Code = "ERROR",
                    Messages = new List<string> { ex.Message }
                });
            }
        }

        private string? GetParameterValue(Dictionary<string, object>? parameters, string key)
        {
            if (parameters == null || !parameters.ContainsKey(key))
                return null;

            return parameters[key]?.ToString();
        }

      
        public Task<GetConditionResponse> GetConditionsAsync(string schemeCode)
        {
            _logger.LogInformation(
               "Getting leave approval condition for scheme code {Scheme Code}",
                schemeCode);

            var conditions = new List<string>
            {
                "IsLeaveApproved",
                "IsLeaveRejected",
                "IsLeavePending",
                "IsLeaveNeedManagerApproval"
            };

            var response = new GetConditionResponse
            {
                Data = conditions,
                Success = true,
            };

            return Task.FromResult(response);
        }
    }
}
