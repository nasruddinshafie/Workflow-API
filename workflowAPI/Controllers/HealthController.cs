using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using workflowAPI.Models.Responses;
using workflowAPI.Services;

namespace workflowAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Service = "Workflow API"
            });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IWorkflowSchemeRegistry _schemeRegistry;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IWorkflowSchemeRegistry schemeRegistry,
            ILogger<AdminController> logger)
        {
            _schemeRegistry = schemeRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Get all registered workflow types
        /// </summary>
        [HttpGet("workflows")]
        public ActionResult<ApiResponse<List<string>>> GetAllWorkflows()
        {
            var workflows = _schemeRegistry.GetAllWorkflowTypes();
            return Ok(ApiResponse<List<string>>.SuccessResponse(workflows));
        }

        /// <summary>
        /// Get scheme configuration for a workflow type
        /// </summary>
        [HttpGet("workflows/{workflowType}")]
        public ActionResult<ApiResponse<object>> GetWorkflowConfig(string workflowType)
        {
            if (!_schemeRegistry.IsValidWorkflowType(workflowType))
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Workflow type '{workflowType}' not found"));
            }

            var config = new
            {
                WorkflowType = workflowType,
                ActiveScheme = _schemeRegistry.GetActiveScheme(workflowType),
                AvailableVersions = _schemeRegistry.GetAvailableVersions(workflowType)
            };

            return Ok(ApiResponse<object>.SuccessResponse(config));
        }
    }
}
