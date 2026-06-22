namespace ERP.API.Services
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        bool IsAuthenticated { get; }
        IEnumerable<string> GetRoles();
    }
}
