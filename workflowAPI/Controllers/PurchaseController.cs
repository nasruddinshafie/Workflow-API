using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using workflowAPI.Models.DTOs;
using workflowAPI.Models.Requests;
using workflowAPI.Models.Responses;
using workflowAPI.Services;

namespace workflowAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {

        private readonly IWorkflowService _workflowService;
        private readonly ILogger<PurchaseController> _logger;

        public PurchaseController(
            IWorkflowService workflowService,
            ILogger<PurchaseController> logger)
        {
            _workflowService = workflowService;
            _logger = logger;
        }

        /// <summary>
        /// Submit a new purchase order
        /// </summary>
        [HttpPost("submit")]
        public async Task<ActionResult<ApiResponse<string>>> SubmitPurchaseOrder(
            [FromBody] PurchaseOrderDto request)
        {
            try
            {
                var parameters = new Dictionary<string, object>
            {
                { "RequestorId", request.RequestorId },
                { "RequestorName", request.RequestorName },
                { "Amount", request.Amount },
                { "Vendor", request.Vendor },
                { "Items", request.Items },
                { "Description", request.Description },
                { "SubmittedDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
            };

                var processId = await _workflowService.CreateInstanceAsync(
                    "PurchaseOrder",
                    request.RequestorId,
                    "PurchaseOrId",
                    parameters);


                var proccessInstances = await _workflowService.GetInstanceAsync(processId);


                return Ok(ApiResponse<GetInstanceInfoResponse>.SuccessResponse(
                    proccessInstances,
                    "Purchase order submitted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit purchase order");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Approve or reject purchase order
        /// </summary>
        [HttpPost("{orderId}/approve")]
        public async Task<ActionResult<ApiResponse<bool>>> ApprovePurchaseOrder(
            string orderId,
            [FromBody] LeaveApprovalDto request)
        {
            try
            {
                var command = request.Approved ? "Approve" : "Reject";

                await _workflowService.ExecuteCommandAsync(new ExecuteCommandRequest
                {
                    ProcessId = orderId,
                    Command = command,
                    IdentityId = request.ApproverId,
                    Parameters = new Dictionary<string, object>
                {
                    { "ApproverId", request.ApproverId },
                    { "ApproverName", request.ApproverName },
                    { "Comments", request.Comments }
                }
                });

                var message = request.Approved ? "Purchase order approved" : "Purchase order rejected";
                return Ok(ApiResponse<bool>.SuccessResponse(true, message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to approve/reject purchase order {OrderId}", orderId);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }
    }
}
