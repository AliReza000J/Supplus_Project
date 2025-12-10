using _0_Framework.Application;
using AccountManagement.Application.Contracts.Role;
using AccountManagement.Domain.RoleAgg;

namespace AccountManagement.Application
{
    public class RoleApplication : IRoleApplication
    {
        private readonly IRoleRepository _roleRepository;

        public RoleApplication(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<OperationResult> Create(CreateRole command)
        {
            var operation = new OperationResult();
            if (await _roleRepository.ExistsAsync(x => x.Name == command.Name))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var role = new Role(command.Name, new List<Permission>());
            _roleRepository.Create(role);
            await _roleRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditRole command)
        {
            var operation = new OperationResult();
            var role = await _roleRepository.GetAsync(command.Id);
            if (role == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _roleRepository.ExistsAsync(x => x.Name == command.Name && x.Id != command.Id))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var permissions = new List<Permission>();
            command.Permissions.ForEach(code => permissions.Add(new Permission(code)));

            role.Edit(command.Name, permissions);
            await _roleRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<EditRole> GetDetails(long id)
        {
            return await _roleRepository.GetDetails(id);
        }

        public async Task<IEnumerable<RoleViewModel>> ListAsync()
        {
            return await _roleRepository.ListAsync();
        }
    }
}