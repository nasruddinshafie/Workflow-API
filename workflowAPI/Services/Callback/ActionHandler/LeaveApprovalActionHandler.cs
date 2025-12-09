using workflowAPI.Models.Callbacks.Requests;
using workflowAPI.Models.Callbacks.Responses;

namespace workflowAPI.Services.Callback.ActionHandler
{
    public class LeaveApprovalActionHandler : IActionHandler
    {
        private readonly ILogger<LeaveApprovalActionHandler> _logger;

        public string WorkflowType => "LeaveApproval";

        public LeaveApprovalActionHandler(ILogger<LeaveApprovalActionHandler> logger)
        {
            _logger = logger;
        }

        public bool CanHandle(string workflowType) => workflowType == WorkflowType;


        public async Task<ExecuteActionResponse> ExecuteActionAsync(ExecuteActionRequest callback)
        {
            //_logger.LogInformation(
            //    "Executing leave action {ActionName} for process {ProcessId}",
            //    callback.Name,
            //    callback.ProcessInstance.ParentProcessId);


            //switch (callback.ActionName)
            //{
            //    case "Submit":
            //        await HandleSubmitAsync(callback);
            //        break;

            //    case "Approve":
            //        await HandleApproveAsync(callback);
            //        break;

            //    case "Reject":
            //        await HandleRejectAsync(callback);
            //        break;

            //    case "Cancel":
            //        await HandleCancelAsync(callback);
            //        break;

            //    case "RequestMoreInfo":
            //        await HandleRequestMoreInfoAsync(callback);
            //        break;

            //    case "SaveDraft":
            //        await HandleSaveDraftAsync(callback);
            //        break;

            //    default:
            //        _logger.LogWarning("Unknown action: {ActionName}", callback.ActionName);
            //        break;
            //}

            var response = new ExecuteActionResponse
            {
                Data = new ExecuteActionResponseData
                {
                    UpdatedParameters = new Dictionary<string, object>
                    {
                        { "LastActionExecuted", callback.Name },
                        { "ActionTimestamp", DateTime.UtcNow }
                    }
                }
            };

            return await Task.FromResult(response);
        }

        public async Task<GetActionResponse> GetActionsAsync(string schemeCode)
        {
            _logger.LogInformation(
                "Getting leave approval actions for scheme code {Scheme Code}",
                 schemeCode);


            var actions = new GetActionResponse
            {
                Data = new List<string> { "Submit","Approve", "Reject", "Request More Info" }
            };

            actions.Success = true;

            return await Task.FromResult(actions);
        }
    }
}
