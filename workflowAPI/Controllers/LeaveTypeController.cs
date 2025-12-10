using Microsoft.AspNetCore.Mvc;
using workflowAPI.Models.Entities;
using workflowAPI.Models.Responses;
using workflowAPI.Services;

namespace workflowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveTypeController : ControllerBase
    {
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly ILogger<LeaveTypeController> _logger;

        public LeaveTypeController(
            ILeaveTypeService leaveTypeService,
            ILogger<LeaveTypeController> logger)
        {
            _leaveTypeService = leaveTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active leave types
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveTypeEntity>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveTypeEntity>>), 500)]
        public async Task<ActionResult<ApiResponse<List<LeaveTypeEntity>>>> GetAllLeaveTypes()
        {
            try
            {
                _logger.LogInformation("Getting all active leave types");
                var leaveTypes = await _leaveTypeService.GetAllActiveLeaveTypesAsync();
                return Ok(ApiResponse<List<LeaveTypeEntity>>.SuccessResponse(leaveTypes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all leave types");
                return StatusCode(500, ApiResponse<List<LeaveTypeEntity>>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get leave type by code
        /// </summary>
        [HttpGet("{code}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeEntity>), 200)]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeEntity>), 404)]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeEntity>), 500)]
        public async Task<ActionResult<ApiResponse<LeaveTypeEntity>>> GetLeaveTypeByCode(string code)
        {
            try
            {
                _logger.LogInformation("Getting leave type by code: {Code}", code);
                var leaveType = await _leaveTypeService.GetLeaveTypeByCodeAsync(code);

                if (leaveType == null)
                {
                    return NotFound(ApiResponse<LeaveTypeEntity>.ErrorResponse($"Leave type with code '{code}' not found"));
                }

                return Ok(ApiResponse<LeaveTypeEntity>.SuccessResponse(leaveType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get leave type by code: {Code}", code);
                return StatusCode(500, ApiResponse<LeaveTypeEntity>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get leave type by ID
        /// </summary>
        [HttpGet("id/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeEntity>), 200)]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeEntity>), 404)]
        [ProducesResponseType(typeof(ApiResponse<LeaveTypeEntity>), 500)]
        public async Task<ActionResult<ApiResponse<LeaveTypeEntity>>> GetLeaveTypeById(int id)
        {
            try
            {
                _logger.LogInformation("Getting leave type by ID: {Id}", id);
                var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(id);

                if (leaveType == null)
                {
                    return NotFound(ApiResponse<LeaveTypeEntity>.ErrorResponse($"Leave type with ID '{id}' not found"));
                }

                return Ok(ApiResponse<LeaveTypeEntity>.SuccessResponse(leaveType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get leave type by ID: {Id}", id);
                return StatusCode(500, ApiResponse<LeaveTypeEntity>.ErrorResponse(ex.Message));
            }
        }
    }
}
