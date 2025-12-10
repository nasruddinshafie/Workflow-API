using Microsoft.AspNetCore.Mvc;
using workflowAPI.Models.Identity;
using workflowAPI.Models.Responses;
using workflowAPI.Services;

namespace workflowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(
            IIdentityService identityService,
            ILogger<IdentityController> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<List<User>>>> GetAllUsers()
        {
            _logger.LogInformation("Getting all users");
            var users = await _identityService.GetAllUsersAsync();
            return Ok(ApiResponse<List<User>>.SuccessResponse(users));
        }

        [HttpGet("users/{userId}")]
        public async Task<ActionResult<ApiResponse<User>>> GetUserById(string userId)
        {
            _logger.LogInformation("Getting user by ID: {UserId}", userId);
            var user = await _identityService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(ApiResponse<User>.ErrorResponse($"User with ID '{userId}' not found"));
            }

            return Ok(ApiResponse<User>.SuccessResponse(user));
        }

        [HttpGet("users/username/{username}")]
        public async Task<ActionResult<ApiResponse<User>>> GetUserByUsername(string username)
        {
            _logger.LogInformation("Getting user by username: {Username}", username);
            var user = await _identityService.GetUserByUsernameAsync(username);

            if (user == null)
            {
                return NotFound(ApiResponse<User>.ErrorResponse($"User with username '{username}' not found"));
            }

            return Ok(ApiResponse<User>.SuccessResponse(user));
        }

        [HttpGet("users/email/{email}")]
        public async Task<ActionResult<ApiResponse<User>>> GetUserByEmail(string email)
        {
            _logger.LogInformation("Getting user by email: {Email}", email);
            var user = await _identityService.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound(ApiResponse<User>.ErrorResponse($"User with email '{email}' not found"));
            }

            return Ok(ApiResponse<User>.SuccessResponse(user));
        }

        [HttpGet("users/role/{role}")]
        public async Task<ActionResult<ApiResponse<List<User>>>> GetUsersByRole(Role role)
        {
            _logger.LogInformation("Getting users by role: {Role}", role);
            var users = await _identityService.GetUsersByRoleAsync(role);
            return Ok(ApiResponse<List<User>>.SuccessResponse(users));
        }

        [HttpGet("users/department/{department}")]
        public async Task<ActionResult<ApiResponse<List<User>>>> GetUsersByDepartment(string department)
        {
            _logger.LogInformation("Getting users by department: {Department}", department);
            var users = await _identityService.GetUsersByDepartmentAsync(department);
            return Ok(ApiResponse<List<User>>.SuccessResponse(users));
        }

        [HttpGet("users/{userId}/manager")]
        public async Task<ActionResult<ApiResponse<User>>> GetManager(string userId)
        {
            _logger.LogInformation("Getting manager for user: {UserId}", userId);
            var manager = await _identityService.GetManagerAsync(userId);

            if (manager == null)
            {
                return NotFound(ApiResponse<User>.ErrorResponse($"Manager not found for user '{userId}'"));
            }

            return Ok(ApiResponse<User>.SuccessResponse(manager));
        }

        [HttpGet("users/{managerId}/direct-reports")]
        public async Task<ActionResult<ApiResponse<List<User>>>> GetDirectReports(string managerId)
        {
            _logger.LogInformation("Getting direct reports for manager: {ManagerId}", managerId);
            var reports = await _identityService.GetDirectReportsAsync(managerId);
            return Ok(ApiResponse<List<User>>.SuccessResponse(reports));
        }

        [HttpGet("users/{userId}/validate")]
        public async Task<ActionResult<ApiResponse<bool>>> ValidateUser(string userId)
        {
            _logger.LogInformation("Validating user: {UserId}", userId);
            var isValid = await _identityService.ValidateUserAsync(userId);
            return Ok(ApiResponse<bool>.SuccessResponse(isValid));
        }

        [HttpGet("users/{userId}/has-role/{role}")]
        public async Task<ActionResult<ApiResponse<bool>>> HasRole(string userId, Role role)
        {
            _logger.LogInformation("Checking if user {UserId} has role {Role}", userId, role);
            var hasRole = await _identityService.HasRoleAsync(userId, role);
            return Ok(ApiResponse<bool>.SuccessResponse(hasRole));
        }
    }
}
