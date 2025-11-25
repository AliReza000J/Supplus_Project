using _0_Framework.Application;

namespace AccountManagement.Application.Contracts.Role
{
    public interface IRoleApplication
    {
        OperationResult Create(CreateRole command);
        OperationResult Edit(EditRole command);
        Task<IEnumerable<RoleViewModel>> ListAsync();
        EditRole GetDetails(long id);
    }
}
