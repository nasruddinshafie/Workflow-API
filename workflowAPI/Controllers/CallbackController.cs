using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using workflowAPI.Models.Callbacks.Events;
using workflowAPI.Models.Callbacks.Requests;
using workflowAPI.Models.Callbacks.Responses;
using workflowAPI.Services;
using workflowAPI.Services.Callback;

namespace workflowAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        private readonly ICallbackHandler _callbackHandler;
        private readonly IIdentityService _identityService;
        private readonly ILogger<CallbackController> _logger;

        public CallbackController(
           ICallbackHandler callbackHandler,
           IIdentityService identityService,
           ILogger<CallbackController> logger)
        {
            _callbackHandler = callbackHandler;
            _identityService = identityService;
            _logger = logger;
        }

        /// <summary>
        /// Process Status Changed Callback
        /// Called when workflow instance moves to new state
        /// Configure in Workflow Server: processstatuschanged → /api/callback/status-changed
        /// </summary>
        [HttpPost("status-changed")]
        public async Task<IActionResult> ProcessStatusChanged([FromBody] ProcessStatusChangedRequest callback)
        {
            try
            {
                _logger.LogInformation(
                    "Received status changed callback for process {ProcessId}: {PreviousState} → {CurrentState}",
                    callback.ProcessId,
                    callback.OldStatus,
                    callback.NewStatus);

                await _callbackHandler.HandleStatusChangedAsync(callback);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling status changed callback");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Process Activity Changed Callback
        /// Called when workflow activity changes
        /// Configure in Workflow Server: processactivitychanged → /api/callback/activity-changed
        /// </summary>
        [HttpPost("activity-changed")]
        public async Task<IActionResult> ProcessActivityChanged([FromBody] ProcessActivityChangedRequest callback)
        {
            try
            {
                _logger.LogInformation(
                    "Received activity changed callback for process {ProcessId}",
                    callback.ProcessId);

                await _callbackHandler.HandleActivityChangedAsync(callback);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling activity changed callback");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get Actions Callback
        /// Called to get available actions for a process
        /// Configure in Workflow Server: getactions → /api/callback/get-actions
        /// </summary>
        [HttpGet("get-actions")]
        public async Task<IActionResult> GetActions(string SchemeCode)
        {
            try
            {
                _logger.LogInformation(
                    "Received get actions callback for scheme code {SchemeCode}",
                    SchemeCode);

                var actions = await _callbackHandler.GetAvailableActionsAsync(SchemeCode);

                return Ok(actions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling get actions callback");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Execute Action Callback
        /// Called when action is executed
        /// Configure in Workflow Server: executeaction → /api/callback/execute-action
        /// </summary>
        [HttpPost("execute-action")]
        public async Task<IActionResult> ExecuteAction([FromBody] ExecuteActionRequest callback)
        {
            try
            {
                _logger.LogInformation(
                    "Received execute action callback: {ActionName} on process {ProcessId}",
                    callback.Name,
                    callback.ProcessInstance);

                var data =  await _callbackHandler.HandleActionExecutedAsync(callback);

                return Ok(new { success = true  , data = data});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling execute action callback");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get Identities Callback
        /// Called to get list of user identities
        /// Configure in Workflow Server: getidentities → /api/callback/get-identities
        /// </summary>
        [HttpPost("get-identities")]
        public async Task<IActionResult> GetIdentities([FromBody] GetIdentitiesRequest request)
        {
            try
            {
                _logger.LogInformation("Received get identities callback");

                // Get all user IDs from database
                var users = await _identityService.GetAllUsersAsync();
                var identityIds = users.Select(u => u.Id).ToList();

                var response = new GetIdentitiesResponse
                {
                    Success = true,
                    Data = identityIds,
                    Code = "200",
                    Messages = new List<string>()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling get identities callback");

                var errorResponse = new GetIdentitiesResponse
                {
                    Success = false,
                    Data = new List<string>(),
                    Code = "500",
                    Messages = new List<string> { ex.Message }
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Check Rule Callback
        /// Called to validate a rule for a specific identity
        /// Configure in Workflow Server: checkrule → /api/callback/check-rule
        /// </summary>
        [HttpPost("check-rule")]
        public async Task<IActionResult> CheckRule([FromBody] RuleCheckRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Received check rule callback for identity {IdentityId}",
                    request.IdentityId);

                // Validate user exists and is active
                bool ruleCheckResult = await _identityService.ValidateUserAsync(request.IdentityId);


                if (ruleCheckResult)
                {
                    ruleCheckResult = request.IdentityId == request.Parameter.ToString();
                }


                var response = new RuleCheckResponse
                {
                    Success = true,
                    Data = ruleCheckResult,
                    Code = "200",
                    Messages = new List<string>()
                };

                _logger.LogInformation(
                    "Rule check result for identity {IdentityId}: {Result}",
                    request.IdentityId,
                    ruleCheckResult);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling check rule callback for identity {IdentityId}",
                    request.IdentityId);

                var errorResponse = new RuleCheckResponse
                {
                    Success = false,
                    Data = false,
                    Code = "500",
                    Messages = new List<string> { ex.Message }
                };

                return StatusCode(500, errorResponse);
            }
        }


        /// <summary>
        /// Process Log Callback
        /// Called to write process logs
        /// Configure in Workflow Server: processlog → /api/callback/process-log
        /// </summary>
        [HttpPost("process-log")]
        public async Task<IActionResult> ProcessLog([FromBody] ProcessLogsRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Received process log callback with {LogCount} log entries",
                    request.ProcessLogs.Count);

                await _callbackHandler.HandleProcessLogAsync(request);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling process log callback");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get Conditions Callback
        /// Called to get available condition for a process
        /// Configure in Workflow Server: getcondition → /api/callback/get-conditions
        /// </summary>
        [HttpGet("get-conditions")]
        public async Task<IActionResult> GetConditions(string SchemeCode)
        {
            try
            {
                _logger.LogInformation(
                    "Received get conditions callback for scheme code {SchemeCode}",
                    SchemeCode);

                var conditions = await _callbackHandler.GetAvailableConditionAsync(SchemeCode);

                return Ok(conditions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling get actions callback");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }



        // <summary>
        // Execute Condition Callback
        // Called to check if condition is true
        // Configure in Workflow Server: executecondition → /api/callback/execute-condition
        // </summary>
        [HttpPost("execute-condition")]
        public async Task<IActionResult> ExecuteCondition([FromBody] ExecuteConditionRequest callback)
        {
            try
            {
                _logger.LogInformation(
                    "Received execute condition callback: {ConditionName} for process {ProcessId}",
                    callback.Name,
                    callback.ProcessInstance);

                var result = await _callbackHandler.HandleConditionExecutedAsync(callback);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling execute condition callback");
                return StatusCode(500, new { result = false, message = ex.Message });
            }
        }


        /// <summary>
        /// Get Rules Callback
        /// Called to get rules for a specific scheme
        /// Configure in Workflow Server: getrules → /api/callback/get-rules
        /// </summary>
        [HttpGet("get-rules")]
        public async Task<IActionResult> GetRules([FromQuery] string schemeCode)
        {
            try
            {
                _logger.LogInformation(
                    "Received get rules callback for scheme code {SchemeCode}",
                    schemeCode);

                // TODO: Implement actual rules retrieval logic
                var rules = new List<string>
                {
                    "CheckRoleCallBack",

                };

                return Ok(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling get rules callback");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
