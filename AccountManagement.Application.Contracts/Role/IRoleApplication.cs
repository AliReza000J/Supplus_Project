using _0_Framework.Application;

namespace AccountManagement.Application.Contracts.Role
{
    public interface IRoleApplication
    {
        Task<OperationResult> Create(CreateRole command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditRole command, CancellationToken cancellation);
        Task<IEnumerable<RoleViewModel>> ListAsync(CancellationToken cancellation);
        Task<EditRole?> GetDetails(long id, CancellationToken cancellation);
    }
}
