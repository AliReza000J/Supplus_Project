using _0_Framework.Domain;
using AccountManagement.Application.Contracts.Role;
using System.Collections.Generic;

namespace AccountManagement.Domain.RoleAgg
{
    public interface IRoleRepository : IRepository<long, Role>
    {
        Task<IEnumerable<RoleViewModel>> ListAsync();
        EditRole GetDetails(long id);
    }
}
