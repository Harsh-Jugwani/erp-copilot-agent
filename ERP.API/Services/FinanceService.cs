using ERP.API.Data;
using ERP.API.Models;
using ERP.API.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Services
{
    public interface IFinanceService
    {
        Task<List<Invoice>> GetPendingInvoicesAsync();
        Task<Invoice?> GetInvoiceByIdAsync(string invoiceNumber);
        Task<List<ApprovalRequest>> GetPendingApprovalsAsync(string approverId);
        Task<List<ApproverDTO>> GetAdminApproversAsync();
        Task<int> CreateInvoiceAsync(Invoice invoice);
        Task<ApprovalRequest> CreateApprovalRequestAsync(string invoiceNumber, string remarks);
        Task<bool> ApproveInvoiceAsync(int approvalId);
        Task<bool> RejectInvoiceAsync(int approvalId, string remarks);
        Task<bool> ProcessApprovalByTokenAsync(string token, bool approve, string remarks = "");
    }

    public class FinanceService : IFinanceService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ILogger<FinanceService> _logger;
        private readonly string userId;

        public FinanceService(AppDbContext db, IEmailService emailService, ILogger<FinanceService> logger, ICurrentUserService currentUser)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
            userId = currentUser.UserId ?? string.Empty;
        }

        public async Task<List<Invoice>> GetPendingInvoicesAsync()
        {
            return await _db.Invoices
                .Where(i => i.Status == InvoiceStatus.Pending)
                .OrderByDescending(i => i.CreatedOn)
                .ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(string invoiceNumber)
        {
            return await _db.Invoices
                .Include(i => i.ApprovalRequests)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        public async Task<List<ApprovalRequest>> GetPendingApprovalsAsync(string approverId)
        {
            return await _db.ApprovalRequests
                .Include(a => a.Invoice)
                .Where(a => a.ApproverId == approverId && a.Status == ApprovalStatus.Pending && a.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(a => a.CreatedOn)
                .ToListAsync();
        }

        public async Task<List<ApproverDTO>> GetAdminApproversAsync()
        {
            return await (
                from ur in _db.UserRoles
                join r in _db.Roles on ur.RoleId equals r.Id
                join u in _db.Users on ur.UserId equals u.Id
                where r.Name == ApplicationConstants.RolesTypes.Admin
                select new ApproverDTO
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName
                }
            ).ToListAsync();
        }

        public async Task<int> CreateInvoiceAsync(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Invoice {InvoiceNumber} created by {CreatedBy}", invoice.InvoiceNumber, invoice.CreatedBy);
            return invoice.Id;
        }

        public async Task<ApprovalRequest> CreateApprovalRequestAsync(string invoiceNumber, string remarks)
        {
            // Validate invoice exists
            var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
            if (invoice == null)
                throw new InvalidOperationException($"Invoice {invoiceNumber} not found.");

            List<ApproverDTO> approvers = await GetAdminApproversAsync();


            // Create approval request
            var approval = new ApprovalRequest
            {
                InvoiceId = invoice.Id,
                RequestedBy = userId,
                ApproverId = approvers.LastOrDefault()?.Id,
                Status = ApprovalStatus.Pending,
                ApprovalToken = Guid.NewGuid().ToString("N"),
                ExpiryDate = DateTime.UtcNow.AddDays(2),
                Remarks = remarks,
                CreatedOn = DateTime.UtcNow
            };

            _db.ApprovalRequests.Add(approval);
            await _db.SaveChangesAsync();

            // Send approval email
            try
            {
                await _emailService.SendApprovalEmailAsync(
                    approval, approvers.LastOrDefault()?.Email);

                _logger.LogInformation(
                    "Approval request {ApprovalId} created and email sent to {Approver}",
                    approval.Id, approvers.FirstOrDefault()?.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send approval email for request {ApprovalId}", approval.Id);
                // Don't throw - approval was created, email failure shouldn't block the process
            }

            return approval;
        }

        public async Task<bool> ApproveInvoiceAsync(int approvalId)
        {
            var approval = await _db.ApprovalRequests
                .Include(a => a.Invoice)
                .FirstOrDefaultAsync(a => a.Id == approvalId);

            if (approval == null)
                throw new InvalidOperationException($"Approval request {approvalId} not found.");

            if (approval.Status != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Approval request is already {approval.Status}.");

            if (DateTime.UtcNow > approval.ExpiryDate)
                throw new InvalidOperationException("Approval request has expired.");

            approval.Status = ApprovalStatus.Approved;
            approval.ApprovedOn = DateTime.UtcNow;
            approval.Invoice.Status = InvoiceStatus.Approved;
            approval.Invoice.LastModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Approval request {ApprovalId} approved by {ApproverId}", approvalId, approval.ApproverId);
            return true;
        }

        public async Task<bool> RejectInvoiceAsync(int approvalId, string remarks)
        {
            var approval = await _db.ApprovalRequests
                .Include(a => a.Invoice)
                .FirstOrDefaultAsync(a => a.Id == approvalId);

            if (approval == null)
                throw new InvalidOperationException($"Approval request {approvalId} not found.");

            if (approval.Status != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Approval request is already {approval.Status}.");

            if (DateTime.UtcNow > approval.ExpiryDate)
                throw new InvalidOperationException("Approval request has expired.");

            approval.Status = ApprovalStatus.Rejected;
            approval.ApprovedOn = DateTime.UtcNow;
            approval.Remarks = remarks;
            approval.Invoice.Status = InvoiceStatus.Rejected;
            approval.Invoice.LastModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Approval request {ApprovalId} rejected by {ApproverId}", approvalId, approval.ApproverId);
            return true;
        }

        public async Task<bool> ProcessApprovalByTokenAsync(string token, bool approve, string remarks = "")
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            var approval = await _db.ApprovalRequests
                .Include(a => a.Invoice)
                .FirstOrDefaultAsync(a => a.ApprovalToken == token);

            if (approval == null)
                throw new InvalidOperationException("Approval request not found for the provided token.");

            if (approval.Status != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Approval request is already {approval.Status}.");

            if (DateTime.UtcNow > approval.ExpiryDate)
                throw new InvalidOperationException("Approval request has expired.");

            if (approve)
            {
                return await ApproveInvoiceAsync(approval.Id);
            }
            else
            {
                var useRemarks = string.IsNullOrWhiteSpace(remarks) ? "Rejected via email" : remarks;
                return await RejectInvoiceAsync(approval.Id, useRemarks);
            }
        }

        public Task<int> CreateApprovalRequestAsync(int invoiceId, string requestedBy, string approverId, string remarks)
        {
            throw new NotImplementedException();
        }
    }
}