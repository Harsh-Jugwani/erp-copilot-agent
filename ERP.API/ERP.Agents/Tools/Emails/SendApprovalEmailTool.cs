using ERP.API.Data;
using ERP.API.Models;
using ERP.API.Services;
using ERP.API.Utilities;
using Microsoft.Extensions.AI;

namespace ERP.API.ERP.Agents.Tools.Emails
{
    public sealed class SendApprovalEmailTool
    {
        private readonly IEmailService _emailService;
        private readonly IFinanceService _financeService;

        public SendApprovalEmailTool(IEmailService emailService, IFinanceService financeService)
        {
            _emailService = emailService;
            _financeService = financeService;
        }
        public AIFunction ToAIFunction() => AIFunctionFactory.Create(SendAsync, nameof(SendApprovalEmailTool), "Send an approval email notification.", null);

        public async Task<bool> SendAsync(int invoiceId,string remarks)
        {
            var request= await _financeService.CreateApprovalRequestAsync(invoiceId, remarks);

            _emailService.SendApprovalEmailAsync(request, ApplicationConstants.AdminAccount.UserName);

            return true;
        }
    }
}
