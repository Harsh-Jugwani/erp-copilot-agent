using ERP.API.Data;
using ERP.API.Models;
using ERP.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace ERP.API.ERP.Agents.Tools.Finance
{
    public sealed class GetPendingInvoicesTool
    {
        private readonly IFinanceService _financeService;

        public GetPendingInvoicesTool(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        public AIFunction ToAIFunction() => AIFunctionFactory.Create(GetAsync, nameof(GetPendingInvoicesTool), "Get invoices that are pending approval.", null);

        public async Task<List<Invoice>> GetAsync()
        {
            return await _financeService.GetPendingInvoicesAsync();
        }
        
    }
}
