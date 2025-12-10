using _0_Framework.Application;

namespace AccountManagement.Application.Contracts.Role
{
    public interface IRoleApplication
    {
        Task<OperationResult> Create(CreateRole command);
        Task<OperationResult> Edit(EditRole command);
        Task<IEnumerable<RoleViewModel>> ListAsync();
        Task<EditRole> GetDetails(long id);
    }
}
