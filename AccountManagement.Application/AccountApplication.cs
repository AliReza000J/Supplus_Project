using _0_Framework.Application;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Domain.AccountAgg;
using AccountManagement.Domain.RoleAgg;
using AccountManagement.Domain.TokenAgg;
using Microsoft.Extensions.Configuration;

namespace AccountManagement.Application
{
    public class AccountApplication : IAccountApplication
    {
        private readonly IFileUploader _fileUploader;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAccountRepository _accountRepository;
        private readonly IAuthHelper _authHelper;
        private readonly IRoleRepository _roleRepository;
        private readonly IConfiguration _config;
        private readonly ITokenRepository _tokenRepository;
        private readonly IJwtService _jwtService;

        public AccountApplication(IAccountRepository accountRepository, IPasswordHasher passwordHasher,
            IFileUploader fileUploader, IAuthHelper authHelper, IRoleRepository roleRepository, IConfiguration config, ITokenRepository tokenRepository, IJwtService jwtService)
        {
            _authHelper = authHelper;
            _roleRepository = roleRepository;
            _fileUploader = fileUploader;
            _passwordHasher = passwordHasher;
            _accountRepository = accountRepository;
            _config = config;
            _tokenRepository = tokenRepository;
            _jwtService = jwtService;
        }

        public async Task<OperationResult> Activate(long id)
        {
            var op = new OperationResult();
            var acc = await _accountRepository.GetAsync(id);

            if (acc == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            acc.Activate();
            await _accountRepository.SaveChangesAsync();
            return op.Succeeded();
        }

        public async Task<OperationResult> ChangePassword(ChangePassword command, string userId)
        {
            var operation = new OperationResult();
            var account = await _accountRepository.GetAsync(command.Id);
            if (account == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (command.Password != command.RePassword)
                return operation.Failed(ApplicationMessages.PasswordsNotMatch);

            if (command.Id == long.Parse(userId) || _authHelper.IsSuperAdmin())
            {
                var password = _passwordHasher.Hash(command.Password);
                account.ChangePassword(password);
                await _accountRepository.SaveChangesAsync();
                return operation.Succeeded("رمز عبور تغییر یافت.");
            }

            return operation.Failed("شما مجاز به انجام این کار نیستید.");
        }

        public async Task<OperationResult> Delete(long id)
        {
            var op = new OperationResult();
            var acc = await _accountRepository.GetAsync(id);

            if (acc == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            if (id != 1)
                acc.Deactivate();

            await _accountRepository.SaveChangesAsync();
            return op.Succeeded();
        }

        public async Task<OperationResult> Edit(EditAccount command, string userId)
        {
            var operation = new OperationResult();
            var account = await _accountRepository.GetAsync(command.Id);
            if (account == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _accountRepository.ExistsAsync(x =>
                (x.Username.ToLower() == command.Username.ToLower() || x.Mobile == command.Mobile) && x.Id != command.Id))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            if (command.Id == long.Parse(userId) || _authHelper.IsSuperAdmin())
            {
                var roleName = Roles.GetRoleBy(command.RoleId);

                var path = $"ProfilePhotos/{roleName}";
                var picturePath = _fileUploader.Upload(command.ProfilePhoto, path);

                account.Edit(
                    command.Fullname, command.Username.ToLower(), command.Email, command.Mobile, command.RoleId, picturePath);

                await _accountRepository.SaveChangesAsync();
                return operation.Succeeded("ویرایش انجام شد.");
            }

            return operation.Failed("شما مجاز به انجام این کار نیستید.");
        }

        public async Task<AccountViewModel> GetAccountBy(long id)
        {
            var account = await _accountRepository.GetAsync(id);

            return new AccountViewModel()
            {
                Id = id,
                Fullname = account.Fullname,
                Mobile = account.Mobile,
                Username = account.Username.ToLower(),
                RoleName = Roles.GetRoleBy(account.RoleId),
                IsActive = account.IsActive,
                ProfilePhoto = account.ProfilePhoto,
                CreationDate = account.CreationDate.ToFarsi(),
            };
        }

        public async Task<List<AccountViewModel>> GetAccounts()
        {
            return await _accountRepository.GetAccountsAsync();
        }

        public async Task<EditAccount> GetDetails(long id)
        {
            return await _accountRepository.GetDetailsAsync(id);
        }

        public async Task<long> GetRoleIdByUsername(string username)
        {
            var acc = await _accountRepository.GetByUsernameAsync(username.ToLower());
            return acc.RoleId;
        }

        public async Task<OperationResult> Login(Login command)
        {
            var operation = new OperationResult();
            var account = await _accountRepository.GetByUsernameAsync(command.Username.ToLower());
            if (account == null)
                return operation.Failed(ApplicationMessages.WrongUserPass);

            if (!account.IsActive)
                return operation.Failed("حساب کاربری غیرفعال است. با پشتیبانی تماس بگیرید.");

            var result = _passwordHasher.Check(account.Password, command.Password);
            if (!result.Verified)
                return operation.Failed(ApplicationMessages.WrongUserPass);

            if (!account.IsActive)
                return operation.Failed("حساب کاربری غیرفعال است.");

            var role = await _roleRepository.GetAsync(account.RoleId);
            var permissions = role?.Permissions
            .Select(x => x.Code)
            .ToList();

            var authViewModel = new AuthViewModel(account.Id, account.RoleId, account.Fullname, account.Username.ToLower(), permissions);
            authViewModel.Role = Roles.GetRoleBy(account.RoleId);

            _authHelper.SignIn(authViewModel);
            return operation.Succeeded();
        }

        public async Task<(OperationResult, JwtResponseDto? Data)> LoginWithJwt(Login command)
        {
            var op = new OperationResult();
            var account = await _accountRepository.GetByUsernameAsync(command.Username.ToLower());

            if (account == null)
                return (op.Failed(ApplicationMessages.RecordNotFound), null);

            if (!account.IsActive)
                return (op.Failed("حساب کاربری غیرفعال است."), null);

            var check = _passwordHasher.Check(account.Password, command.Password);
            if (!check.Verified)
                return (op.Failed(ApplicationMessages.WrongUserPass), null);

            var role = await _roleRepository.GetAsync(account.RoleId);
            var permissions = role?.Permissions?.Select(p => p.Code).ToList()
                              ?? new List<int>();

            var accessMinutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var am) ? am : 15;
            var refreshDays = int.TryParse(_config["Jwt:RefreshTokenDays"], out var rd) ? rd : 7;

            var accessToken = _jwtService.GenerateAccessToken(account.Id, account.Username.ToLower(), account.RoleId, permissions, accessMinutes);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var accessExpiry = _jwtService.GetExpiration(accessToken);
            var refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);

            var tokenEntity = new Token(account.Id, accessToken, accessExpiry, refreshToken, refreshExpiry);
            await _tokenRepository.AddAsync(tokenEntity);

            var dto = new JwtResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = accessExpiry,
                RefreshTokenExpiry = refreshExpiry
            };

            return (op.Succeeded("ورود موفق."), dto);
        }


        public void Logout()
        {
            _authHelper.SignOut();
        }

        public async Task<OperationResult> LogoutWithJwt(string accessToken)
        {
            var op = new OperationResult();
            if (string.IsNullOrWhiteSpace(accessToken)) return op.Failed("توکن نامعتبر است");

            var tokenRecord = await _tokenRepository.GetByAccessTokenAsync(accessToken);
            if (tokenRecord == null) return op.Failed("توکن یافت نشد");

            await _tokenRepository.RevokeAsync(tokenRecord);
            return op.Succeeded("خروج انجام شد.");
        }

        public async Task<(OperationResult, JwtResponseDto? Data)> RefreshToken(string refreshToken)
        {
            var op = new OperationResult();

            if (string.IsNullOrWhiteSpace(refreshToken))
                return (op.Failed("توکن معتبر نیست"), null);

            var tokenRecord = await _tokenRepository.GetByRefreshTokenAsync(refreshToken);

            if (tokenRecord == null)
                return (op.Failed("توکن بازنشانی یافت نشد یا باطل شده"), null);

            if (tokenRecord.IsRefreshExpired())
                return (op.Failed("Refresh token منقضی شده"), null);

            if (tokenRecord.IsRevoked)
                return (op.Failed("Refresh token باطل شده است"), null);

            var account = await _accountRepository.GetAsync(tokenRecord.AccountId);
            if (account == null)
                return (op.Failed("کاربر مرتبط با توکن یافت نشد"), null);

            var role = await _roleRepository.GetAsync(account.RoleId);

            var permissions = role?.Permissions?.Select(p => p.Code).ToList() ?? new List<int>();

            var accessMinutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var am) ? am : 15;
            var refreshDays = int.TryParse(_config["Jwt:RefreshTokenDays"], out var rd) ? rd : 7;

            // revoke old token
            tokenRecord.Revoke();
            await _tokenRepository.RevokeAsync(tokenRecord);

            // generate new tokens
            var newAccess = _jwtService.GenerateAccessToken(account.Id, account.Username.ToLower(), account.RoleId, permissions, accessMinutes);
            var newRefresh = _jwtService.GenerateRefreshToken();
            var accessExpiry = _jwtService.GetExpiration(newAccess);
            var refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);

            var newTokenEntity = new Token(account.Id, newAccess, accessExpiry, newRefresh, refreshExpiry);
            await _tokenRepository.AddAsync(newTokenEntity);

            var dto = new JwtResponseDto
            {
                AccessToken = newAccess,
                RefreshToken = newRefresh,
                AccessTokenExpiry = accessExpiry,
                RefreshTokenExpiry = refreshExpiry
            };

            return (op.Succeeded("توکن تجدید شد"), dto);
        }

        public async Task<OperationResult> Register(RegisterAccount command)
        {
            var operation = new OperationResult();

            if (await _accountRepository.ExistsAsync(x => x.Username.ToLower() == command.Username.ToLower() || x.Mobile == command.Mobile))
                return operation.Failed
                    ($"کاربری با نام {command.Username} و یا شماره موبایل {command.Mobile} وجود دارد.");

            var password = _passwordHasher.Hash(command.Password);

            var roleName = Roles.GetRoleBy(command.RoleId);

            var path = $"ProfilePhotos/{roleName}";

            string picturePath = "";
            if (command.ProfilePhoto != null)
            {
                picturePath = _fileUploader.Upload(command.ProfilePhoto, path);
            }
            else
            {
                var seed = Guid.NewGuid().ToString("N");
                command.ProfilePhoto = _fileUploader.GenerateIdenticonAsFormFile(seed);
                picturePath = _fileUploader.Upload(command.ProfilePhoto, path);
            }

            var account = new Account(command.Fullname,
                command.Username.ToLower(),
                command.Mobile,
                command.Email,
                password,
                command.RoleId,
                picturePath);

            _accountRepository.Create(account);
            await _accountRepository.SaveChangesAsync();
            return operation.Succeeded("ثبت‌نام با موفقیت انجام شد.");
        }

        public async Task<List<AccountViewModel>> Search(AccountSearchModel searchModel)
        {
            return await _accountRepository.SearchAsync(searchModel);
        }
    }
}