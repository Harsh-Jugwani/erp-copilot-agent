using ERP.API.Data;
using ERP.API.Models;
using ERP.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace ERP.API.ERP.Agents.Tools.Finance
{
    public sealed class GetInvoiceDetailsTool
    {
        private readonly IFinanceService _financeService;

        public GetInvoiceDetailsTool(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        public AIFunction ToAIFunction() => AIFunctionFactory.Create(GetAsync, nameof(GetInvoiceDetailsTool), "Get invoice details by invoice identifier.", null);

        public async Task<Invoice?> GetAsync(int invoiceId)
        {
            return await _financeService.GetInvoiceByIdAsync(invoiceId);
        }
    }
}
