using ERP.API.Services;
using ERP.API.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class InvoiceApprovalController : ControllerBase
    {
        private readonly IFinanceService _financeService;
        private readonly ILogger<InvoiceApprovalController> _logger;
        private readonly HttpContextAccessor _httpContextAccessor;

        public InvoiceApprovalController(IFinanceService financeService, ILogger<InvoiceApprovalController> logger, HttpContextAccessor httpContextAccessor)
        {
            _financeService = financeService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get all pending approval requests for the current admin user
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
               ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name
               ?? string.Empty;

                var pending = await _financeService.GetPendingApprovalsAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = pending.Select(a => new
                    {
                        a.Id,
                        a.ApprovalToken,
                        invoiceNumber = a.Invoice.InvoiceNumber,
                        vendorName = a.Invoice.VendorName,
                        amount = a.Invoice.Amount,
                        invoiceDate = a.Invoice.InvoiceDate,
                        requestedBy = a.RequestedBy,
                        status = a.Status.ToString(),
                        createdOn = a.CreatedOn,
                        expiryDate = a.ExpiryDate,
                        timeRemaining = a.ExpiryDate - DateTime.UtcNow,
                        remarks = a.Remarks
                    }),
                    count = pending.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending approvals");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving pending approvals.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get details of a specific approval request
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApprovalDetails(int id)
        {
            try
            {
                var approval = await _financeService.GetPendingApprovalsAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                var specificApproval = approval.FirstOrDefault(a => a.Id == id);

                if (specificApproval == null)
                    return NotFound(new { success = false, message = "Approval request not found." });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        specificApproval.Id,
                        specificApproval.ApprovalToken,
                        status = specificApproval.Status.ToString(),
                        createdOn = specificApproval.CreatedOn,
                        expiryDate = specificApproval.ExpiryDate,
                        approvedOn = specificApproval.ApprovedOn,
                        remarks = specificApproval.Remarks,
                        requestedBy = specificApproval.RequestedBy,
                        invoice = new
                        {
                            specificApproval.Invoice.Id,
                            specificApproval.Invoice.InvoiceNumber,
                            specificApproval.Invoice.VendorName,
                            specificApproval.Invoice.Amount,
                            specificApproval.Invoice.InvoiceDate,
                            specificApproval.Invoice.CreatedBy,
                            specificApproval.Invoice.CreatedOn
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approval details for {ApprovalId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving approval details.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Approve an invoice approval request
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveInvoice(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized("User context not found.");

                await _financeService.ApproveInvoiceAsync(id);

                _logger.LogInformation("Invoice approval request {ApprovalId} approved by {UserId}", id, userId);

                return Ok(new
                {
                    success = true,
                    message = "Invoice approved successfully.",
                    approvalId = id,
                    status = "Approved",
                    approvedOn = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation on approval {ApprovalId}: {Message}", id, ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving invoice {ApprovalId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while approving the invoice.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Reject an invoice approval request
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectInvoice(int id, [FromBody] RejectApprovalRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Remarks))
                    return BadRequest(new { success = false, message = "Rejection remarks are required." });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized("User context not found.");

                await _financeService.RejectInvoiceAsync(id, request.Remarks);

                _logger.LogInformation("Invoice approval request {ApprovalId} rejected by {UserId} with remarks: {Remarks}", id, userId, request.Remarks);

                return Ok(new
                {
                    success = true,
                    message = "Invoice rejected successfully.",
                    approvalId = id,
                    status = "Rejected",
                    remarks = request.Remarks,
                    rejectedOn = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation on approval {ApprovalId}: {Message}", id, ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting invoice {ApprovalId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while rejecting the invoice.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Process approval request via token (anonymous access for email link)
        /// </summary>
        [HttpPost("process-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessApprovalByToken([FromBody] ProcessApprovalByTokenRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Token))
                    return BadRequest(new { success = false, message = "Approval token is required." });

                if (!request.Approve && string.IsNullOrWhiteSpace(request.Remarks))
                    return BadRequest(new { success = false, message = "Rejection remarks are required." });

                // For demo purposes, you'd typically verify the token against the database
                // This endpoint is meant to be called from email links

                return Ok(new
                {
                    success = true,
                    message = "This endpoint is for email link processing. Please use the /approve or /reject endpoints with proper authentication.",
                    instruction = "To approve/reject, authenticate as an Admin user and use the POST endpoints with the approval ID."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing approval by token");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while processing the approval.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all admin approvers in the system
        /// </summary>
        [HttpGet("approvers/list")]
        public async Task<IActionResult> GetAdminApprovers()
        {
            try
            {
                var approvers = await _financeService.GetAdminApproversAsync();

                return Ok(new
                {
                    success = true,
                    data = approvers,
                    count = approvers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin approvers");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving approvers.",
                    error = ex.Message
                });
            }
        }
    }

    public class RejectApprovalRequest
    {
        public string Remarks { get; set; } = string.Empty;
    }

    public class ProcessApprovalByTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public bool Approve { get; set; } = true;
        public string Remarks { get; set; } = string.Empty;
    }
}