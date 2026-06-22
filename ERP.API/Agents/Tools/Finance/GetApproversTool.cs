using ERP.API.Data;
using ERP.API.Models;
using ERP.API.Services;
using ERP.API.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace ERP.Agents.Tools.Finance
{
    public sealed class GetApproversTool
    {
        private readonly IFinanceService _financeService;

        public GetApproversTool(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        public AIFunction ToAIFunction() => AIFunctionFactory.Create(GetAsync, nameof(GetApproversTool), "Get approvers available for finance workflow.", null);

        public async Task<List<ApproverDTO>> GetAsync()
        {
            return await _financeService.GetAdminApproversAsync();
        }
    }
}
