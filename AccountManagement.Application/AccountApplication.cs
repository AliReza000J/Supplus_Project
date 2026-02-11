using _0_Framework.Application;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Domain.AccountAgg;
using AccountManagement.Domain.RoleAgg;
using AccountManagement.Domain.TokenAgg;
using Microsoft.Extensions.Configuration;
using ShopManagement.Application.Contracts.Cart;

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
        private readonly ICartApplication _cartApplication;


        public AccountApplication(IAccountRepository accountRepository, IPasswordHasher passwordHasher,
            IFileUploader fileUploader, IAuthHelper authHelper, IRoleRepository roleRepository, IConfiguration config, ITokenRepository tokenRepository, IJwtService jwtService, ICartApplication cartApplication)
        {
            _authHelper = authHelper;
            _roleRepository = roleRepository;
            _fileUploader = fileUploader;
            _passwordHasher = passwordHasher;
            _accountRepository = accountRepository;
            _config = config;
            _tokenRepository = tokenRepository;
            _jwtService = jwtService;
            _cartApplication = cartApplication;
        }

        public async Task<OperationResult> Activate(long id, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var acc = await _accountRepository.GetAsync(id, cancellation);

            if (acc == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            acc.Activate();
            await _accountRepository.SaveChangesAsync(cancellation);
            return op.Succeeded();
        }

        public async Task<OperationResult> ChangePassword(ChangePassword command, long userId, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var account = await _accountRepository.GetAsync(command.Id, cancellation);
            if (account == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (command.Password != command.RePassword)
                return operation.Failed(ApplicationMessages.PasswordsNotMatch);

            if (command.Id == userId || _authHelper.IsSuperAdmin())
            {
                var password = _passwordHasher.Hash(command.Password);
                account.ChangePassword(password);
                await _accountRepository.SaveChangesAsync(cancellation);
                return operation.Succeeded("رمز عبور تغییر یافت.");
            }

            return operation.Failed("شما مجاز به انجام این کار نیستید.");
        }

        public async Task<OperationResult> Deactivate(long id, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var acc = await _accountRepository.GetAsync(id, cancellation);

            if (acc == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            var currentUserId = _authHelper.CurrentUserId();
            if (id != 1 && id != currentUserId)
                acc.Deactivate();
            else
                return op.Failed("امکان غیرفعال‌سازی این اکانت وجود ندارد.");

            await _accountRepository.SaveChangesAsync(cancellation);
            return op.Succeeded();
        }

        public async Task<OperationResult> Edit(EditAccount command, long userId, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var account = await _accountRepository.GetAsync(command.Id, cancellation);
            if (account == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _accountRepository.ExistsAsync(x =>
                (x.Username.ToLower() == command.Username.ToLower() || x.Mobile == command.Mobile) && x.Id != command.Id, cancellation))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            if ((command.Id == userId || _authHelper.IsSuperAdmin())
                && account.IsCompleted())
            {
                var roleName = Roles.GetRoleBy(command.RoleId);

                var path = $"ProfilePhotos/{roleName}";
                var picturePath = "";

                if (!(command.ProfilePhoto == null && string.IsNullOrEmpty(command.ProfilePhoto?.FileName)))
                {
                    _fileUploader.DeleteFile(account.ProfilePhoto);
                    picturePath = await _fileUploader.UploadAsync(command.ProfilePhoto, path, cancellation);
                }

                account.Edit(
                    command.Fullname, command.Username.ToLower(), command.Email, command.Mobile, command.RoleId, picturePath);

                await _accountRepository.SaveChangesAsync(cancellation);
                return operation.Succeeded("ویرایش انجام شد.");
            }

            return operation.Failed("شما مجاز به انجام این کار نیستید.");
        }

        public async Task<AccountViewModel> GetAccountBy(long id, CancellationToken cancellation)
        {
            var account = await _accountRepository.GetAsync(id, cancellation);

            if (account == null)
                return new();

            return new AccountViewModel()
            {
                Id = id,
                Fullname = account.Fullname,
                Mobile = account.Mobile.ToPersianNumber(),
                Username = account.Username.ToLower(),
                RoleName = Roles.GetRoleBy(account.RoleId),
                IsActive = account.IsActive,
                IsCompleted = account.IsCompleted(),
                ProfilePhoto = account.ProfilePhoto,
                CreationDate = account.CreationDate.ToFarsi().ToPersianNumber(),
            };
        }

        public async Task<bool> ExistsAccountByMobile(string mobile, CancellationToken cancellation)
        {
            var acc = await _accountRepository.GetAccountByMobile(mobile, cancellation);
            if (acc == null)
                return false;
            return true;
        }

        public async Task<List<AccountViewModel>> GetAccounts(CancellationToken cancellation)
        {
            return await _accountRepository.GetAccountsAsync(cancellation);
        }

        public async Task<EditAccount?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _accountRepository.GetDetailsAsync(id, cancellation);
        }

        public async Task<long> GetRoleIdByUsername(string username, CancellationToken cancellation)
        {
            var acc = await _accountRepository.GetByUsernameAsync(username.ToLower(), cancellation);
            if (acc == null)
                return 0;
            return acc.RoleId;
        }

        public async Task<OperationResult> Login(Login command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var account = await _accountRepository.GetByUsernameAsync(command.Username.ToLower(), cancellation);
            if (account == null)
                return operation.Failed(ApplicationMessages.WrongUserPass);

            if (!account.IsActive)
                return operation.Failed("حساب کاربری غیرفعال است. با پشتیبانی تماس بگیرید.");

            if (!account.IsCompleted() || string.IsNullOrWhiteSpace(account.Password))
                return operation.Failed("لطفا با تلفن همراه خود وارد شوید و سپس پروفایل خود را تکمیل نمایید.");

            var (Verified, NeedsUpgrade) = _passwordHasher.Check(account.Password, command.Password);
            if (!Verified)
                return operation.Failed(ApplicationMessages.WrongUserPass);

            if (!account.IsActive)
                return operation.Failed("حساب کاربری غیرفعال است.");

            var role = await _roleRepository.GetAsync(account.RoleId, cancellation);
            var permissions = role?.Permissions
            .Select(x => x.Code)
            .ToList();

            var authViewModel = new AuthViewModel(account.Id, account.RoleId, account.Fullname, account.Username.ToLower(), permissions)
            {
                Role = Roles.GetRoleBy(account.RoleId)
            };

            await _authHelper.SignIn(authViewModel, command.RememberMe);
            return operation.Succeeded();
        }

        public async Task<(OperationResult, JwtResponseDto? Data)> LoginWithJwt(Login command, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var account = await _accountRepository.GetByUsernameAsync(command.Username.ToLower(), cancellation);

            if (account == null)
                return (op.Failed(ApplicationMessages.RecordNotFound), null);

            if (!account.IsActive)
                return (op.Failed("حساب کاربری غیرفعال است."), null);

            if (!account.IsCompleted() || string.IsNullOrWhiteSpace(account.Password))
                return (op.Failed("لطفا با تلفن همراه خود وارد شوید و سپس پروفایل خود را تکمیل نمایید."), null);

            var (Verified, NeedsUpgrade) = _passwordHasher.Check(account.Password, command.Password);
            if (!Verified)
                return (op.Failed(ApplicationMessages.WrongUserPass), null);

            var role = await _roleRepository.GetAsync(account.RoleId, cancellation);
            var permissions = role?.Permissions?.Select(p => p.Code).ToList()
                              ?? new List<int>();

            var accessMinutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var am) ? am : 45;
            var refreshDays = int.TryParse(_config["Jwt:RefreshTokenDays"], out var rd) ? rd : 3;

            var accessToken = _jwtService.GenerateAccessToken(account.Id, account.Username.ToLower(), account.RoleId, permissions, accessMinutes);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var accessExpiry = _jwtService.GetExpiration(accessToken);
            var refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);

            var tokenEntity = new Token(account.Id, accessToken, accessExpiry, refreshToken, refreshExpiry);
            await _tokenRepository.AddAsync(tokenEntity, cancellation);

            var dto = new JwtResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = accessExpiry,
                RefreshTokenExpiry = refreshExpiry
            };

            return (op.Succeeded("ورود موفق."), dto);
        }

        public async Task<(OperationResult, JwtResponseDto? Data)> LoginByMobileWithJwt(string mobile, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var account = await _accountRepository.GetAccountByMobile(mobile, cancellation);

            if (account == null)
                return (op.Failed(ApplicationMessages.RecordNotFound), null);

            if (!account.IsActive)
                return (op.Failed("حساب کاربری غیرفعال است."), null);

            var role = await _roleRepository.GetAsync(account.RoleId, cancellation);
            var permissions = role?.Permissions?.Select(p => p.Code).ToList()
                              ?? new List<int>();

            var accessMinutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var am) ? am : 45;
            var refreshDays = int.TryParse(_config["Jwt:RefreshTokenDays"], out var rd) ? rd : 3;

            var accessToken = _jwtService.GenerateAccessToken(account.Id, account.Username.ToLower(), account.RoleId, permissions, accessMinutes);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var accessExpiry = _jwtService.GetExpiration(accessToken);
            var refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);

            var tokenEntity = new Token(account.Id, accessToken, accessExpiry, refreshToken, refreshExpiry);
            await _tokenRepository.AddAsync(tokenEntity, cancellation);

            var dto = new JwtResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = accessExpiry,
                RefreshTokenExpiry = refreshExpiry
            };

            return (op.Succeeded("ورود موفق."), dto);
        }

        public async Task Logout(CancellationToken cancellation = default)
        {
            await _authHelper.SignOut();
        }

        public async Task<OperationResult> LogoutWithJwt(string accessToken, CancellationToken cancellation)
        {
            var op = new OperationResult();
            if (string.IsNullOrWhiteSpace(accessToken)) return op.Failed("توکن نامعتبر است");

            var tokenRecord = await _tokenRepository.GetByAccessTokenAsync(accessToken, cancellation);
            if (tokenRecord == null) return op.Failed("توکن یافت نشد");

            await _tokenRepository.RevokeAsync(tokenRecord, cancellation);
            return op.Succeeded("خروج انجام شد.");
        }

        public async Task<(OperationResult, JwtResponseDto? Data)> RefreshToken(string refreshToken, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (string.IsNullOrWhiteSpace(refreshToken))
                return (op.Failed("توکن معتبر نیست"), null);

            var tokenRecord = await _tokenRepository.GetByRefreshTokenAsync(refreshToken, cancellation);

            if (tokenRecord == null)
                return (op.Failed("توکن بازنشانی یافت نشد یا باطل شده"), null);

            if (tokenRecord.IsRefreshExpired())
                return (op.Failed("Refresh token منقضی شده"), null);

            if (tokenRecord.IsRevoked)
                return (op.Failed("Refresh token باطل شده است"), null);

            var account = await _accountRepository.GetAsync(tokenRecord.UserId, cancellation);
            if (account == null)
                return (op.Failed("کاربر مرتبط با توکن یافت نشد"), null);

            var role = await _roleRepository.GetAsync(account.RoleId, cancellation);

            var permissions = role?.Permissions?.Select(p => p.Code).ToList() ?? new List<int>();

            var accessMinutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var am) ? am : 45;
            var refreshDays = int.TryParse(_config["Jwt:RefreshTokenDays"], out var rd) ? rd : 3;

            // revoke old token
            tokenRecord.Revoke();
            await _tokenRepository.RevokeAsync(tokenRecord, cancellation);

            // generate new tokens
            var newAccess = _jwtService.GenerateAccessToken(account.Id, account.Username.ToLower(), account.RoleId, permissions, accessMinutes);
            var newRefresh = _jwtService.GenerateRefreshToken();
            var accessExpiry = _jwtService.GetExpiration(newAccess);
            var refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);

            var newTokenEntity = new Token(account.Id, newAccess, accessExpiry, newRefresh, refreshExpiry);
            await _tokenRepository.AddAsync(newTokenEntity, cancellation);

            var dto = new JwtResponseDto
            {
                AccessToken = newAccess,
                RefreshToken = newRefresh,
                AccessTokenExpiry = accessExpiry,
                RefreshTokenExpiry = refreshExpiry
            };

            return (op.Succeeded("توکن تجدید شد"), dto);
        }

        public async Task<OperationResult> Register(RegisterAccount command, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            if (command == null)
                return operation.Failed("آیتم ثبت‌نام خالی است!");

            if (command.Password != command.RePassword)
                return operation.Failed(ApplicationMessages.PasswordsNotMatch);

            if (await _accountRepository.ExistsAsync(x => x.Username.ToLower() == command.Username.ToLower() || x.Mobile == command.Mobile, cancellation))
                return operation.Failed
                    ($"کاربری با نام {command.Username} و یا شماره موبایل {command.Mobile} وجود دارد.");

            var password = _passwordHasher.Hash(command.Password);

            var roleName = Roles.GetRoleBy(command.RoleId);

            var path = $"ProfilePhotos/{roleName}";

            string picturePath = "";
            if (command.ProfilePhoto != null)
            {
                picturePath = await _fileUploader.UploadAsync(command.ProfilePhoto, path, cancellation);
            }
            else
            {
                var seed = Guid.NewGuid().ToString("N");
                command.ProfilePhoto = _fileUploader.GenerateIdenticonAsFormFile(seed);
                picturePath = await _fileUploader.UploadAsync(command.ProfilePhoto, path, cancellation);
            }

            var account = new Account(command.Fullname,
                command.Username.ToLower(),
                command.Mobile,
                command.Email,
                password,
                command.RoleId,
                picturePath);

            _accountRepository.Create(account);
            await _accountRepository.SaveChangesAsync(cancellation);
            await _cartApplication.CreateCartAsync(account.Id, cancellation);
            return operation.Succeeded("ثبت‌نام با موفقیت انجام شد.");
        }

        public async Task<List<AccountViewModel>> Search(AccountSearchModel searchModel, CancellationToken cancellation)
        {
            return await _accountRepository.SearchAsync(searchModel, cancellation);
        }

        public async Task<OperationResult> RegisterByMobile(RegisterByMobile command, CancellationToken cancellation)
        {
            var result = new OperationResult();

            if (await _accountRepository.ExistsAsync(x => x.Mobile == command.Mobile, cancellation))
                return result.Failed("این شماره قبلاً ثبت شده است.");

            var roleName = Roles.GetRoleBy(Roles.Customer);

            var path = $"ProfilePhotos/{roleName}";

            var seed = Guid.NewGuid().ToString("N");
            var photo = _fileUploader.GenerateIdenticonAsFormFile(seed);
            var picturePath = await _fileUploader.UploadAsync(photo, path, cancellation);

            var account = new Account(
                mobile: command.Mobile,
                photo: picturePath);
            _accountRepository.Create(account);

            await _accountRepository.SaveChangesAsync(cancellation);
            await _cartApplication.CreateCartAsync(account.Id, cancellation);
            return result.Succeeded("ثبت‌نام با موفقیت انجام شد.");
        }

        public async Task<OperationResult> CompleteProfile(CompleteProfile command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var acc = await _accountRepository.GetAsync(command.Id, cancellation);

            if (acc == null || acc.Id <= 0)
                return op.Failed("کاربر یافت نشد!");

            if (command.Password != command.RePassword)
                return op.Failed(ApplicationMessages.PasswordsNotMatch);

            if (await _accountRepository.ExistsAsync(x => x.Username.ToLower() == command.Username.ToLower(), cancellation))
                return op.Failed
                    ($"کاربری با نام {command.Username} وجود دارد.");

            if (acc.IsCompleted())
                return op.Failed("شما قبلا پروفایل خود را تکمیل کرده‌اید.");

            var roleName = Roles.GetRoleBy(Roles.Customer);
            var path = $"ProfilePhotos/{roleName}";
            var picturePath = "";
            if (command.ProfilePhoto != null && string.IsNullOrEmpty(command.ProfilePhoto.FileName))
            {
                _fileUploader.DeleteFile(acc?.ProfilePhoto);
                picturePath = await _fileUploader.UploadAsync(command.ProfilePhoto, path, cancellation);
            }

            acc?.CompleteProfile(command.Fullname, command.Username, command.Email, command.Password, picturePath);
            await _accountRepository.SaveChangesAsync(cancellation);
            return op.Succeeded("تکمیل پروفایل با موفقیت انجام شد.");
        }
    }
}