using _0_Framework.Application;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Application.Contracts.Account.ApiDTOs;
using AccountManagement.Application.Contracts.Account.ApplicationDTOs;
using AccountManagement.Domain.AccountAgg;
using AccountManagement.Domain.RoleAgg;
using AccountManagement.Domain.TokenAgg;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;


        public AccountApplication(IAccountRepository accountRepository, IPasswordHasher passwordHasher,
            IFileUploader fileUploader, IAuthHelper authHelper, IRoleRepository roleRepository, IConfiguration config, ITokenRepository tokenRepository, IJwtService jwtService, ICartApplication cartApplication, IMemoryCache cache, ISmsService smsService, IEmailService emailService)
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
            _cache = cache;
            _smsService = smsService;
            _emailService = emailService;
        }

        // ==========================================
        // Admin Panel (Cookie-Based)
        // ==========================================
        public async Task<OperationResult> LoginToAdminPanel(LoginViaUserPass command, CancellationToken cancellation)
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

        public async Task LogoutFromAdminPanel(CancellationToken cancellation = default)
        {
            await _authHelper.SignOut();
        }

        public async Task<OperationResult> CreateAccountByAdmin(CreateAccount command, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            if (!_authHelper.IsSuperAdmin())
                return operation.Failed("فقط سوپر ادمین مجاز به ثبت کاربر جدید‌ است.");

            if (command == null)
                return operation.Failed("اطلاعات ارسالی نامعتبر است.");

            if (await _accountRepository.ExistsAsync(x => x.Username.ToLower() == command.Username.ToLower() || x.Mobile == command.Mobile, cancellation))
                return operation.Failed($"کاربری با نام کاربری {command.Username} یا شماره موبایل {command.Mobile} از قبل وجود دارد.");

            if (!string.IsNullOrWhiteSpace(command.Email))
            {
                if (await _accountRepository.ExistsAsync(x => x.Email.ToLower() == command.Email.ToLower(), cancellation))
                    return operation.Failed("این ایمیل قبلاً توسط کاربر دیگری ثبت شده است.");
            }

            var password = _passwordHasher.Hash(command.Password);
            var roleName = Roles.GetRoleBy(command.RoleId);
            var path = $"ProfilePhotos/{roleName}";

            string picturePath;
            if (command.ProfilePhoto != null)
            {
                picturePath = await _fileUploader.UploadAsync(command.ProfilePhoto, path, cancellation);
            }
            else
            {
                var seed = Guid.NewGuid().ToString("N");
                var generatedPhoto = _fileUploader.GenerateIdenticonAsFormFile(seed);
                picturePath = await _fileUploader.UploadAsync(generatedPhoto, path, cancellation);
            }

            var account = new Account(
                command.Fullname,
                command.Username.ToLower(),
                command.Mobile,
                command.Email,
                password,
                command.RoleId,
                picturePath);

            _accountRepository.Create(account);
            await _accountRepository.SaveChangesAsync(cancellation);

            if (command.RoleId == 5)
            {
                var cartResult = await _cartApplication.CreateCartAsync(account.Id, cancellation);
                if (!cartResult.IsSucceeded)
                    return operation.Failed(cartResult.Message);
            }

            return operation.Succeeded("کاربر با موفقیت در سیستم ثبت شد.");
        }

        public async Task<OperationResult> EditAccountByAdmin(EditAccount command, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            var account = await _accountRepository.GetAsync(command.Id, cancellation);
            if (account == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (!_authHelper.IsSuperAdmin())
                return operation.Failed("شما مجاز به انجام این کار نیستید.");

            if (await _accountRepository.ExistsAsync(x =>
                (x.Username.ToLower() == command.Username.ToLower() || x.Mobile == command.Mobile) && x.Id != command.Id, cancellation))
            {
                return operation.Failed(ApplicationMessages.DuplicatedRecord);
            }

            if (!string.IsNullOrWhiteSpace(command.Email))
            {
                if (await _accountRepository.ExistsAsync(x => x.Email.ToLower() == command.Email.ToLower() && x.Id != command.Id, cancellation))
                    return operation.Failed("این ایمیل قبلاً توسط کاربر دیگری ثبت شده است.");
            }

            var roleName = Roles.GetRoleBy(command.RoleId);
            var path = $"ProfilePhotos/{roleName}";

            string picturePath = "";

            if (command.ProfilePhoto != null && command.ProfilePhoto.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(account.ProfilePhoto))
                {
                    _fileUploader.DeleteFile(account.ProfilePhoto);
                }

                picturePath = await _fileUploader.UploadAsync(command.ProfilePhoto, path, cancellation);
            }

            account.EditFullInfoByAdmin(
                command.Fullname,
                command.Username.ToLower(),
                command.Email,
                command.Mobile,
                command.RoleId,
                picturePath);

            await _accountRepository.SaveChangesAsync(cancellation);

            return operation.Succeeded($"ویرایش کاربر {command.Id}# با موفقیت انجام شد.");
        }

        public async Task<OperationResult> EditPersonalProfile(EditPersonalProfile command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var account = await _accountRepository.GetAsync(command.Id, cancellation);
            if (account == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            if (await _accountRepository.ExistsAsync(x => x.Username.ToLower() == command.Username.ToLower() && x.Id != command.Id, cancellation))
                return op.Failed($"کاربری با نام {command.Username} از قبل وجود دارد.");

            if (!string.IsNullOrWhiteSpace(command.Email))
            {
                if (await _accountRepository.ExistsAsync(x => x.Email.ToLower() == command.Email.ToLower() && x.Id != command.Id, cancellation))
                    return op.Failed("این ایمیل قبلاً توسط کاربر دیگری ثبت شده است.");
            }

            if (!string.IsNullOrWhiteSpace(command.Mobile))
            {
                if (await _accountRepository.ExistsAsync(x => x.Mobile.ToLower() == command.Mobile.ToLower() && x.Id != command.Id, cancellation))
                    return op.Failed("این شماره موبایل قبلاً توسط کاربر دیگری ثبت شده است.");
            }

            var picturePath = account.ProfilePhoto;
            if (command.ProfilePhoto != null && command.ProfilePhoto.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(account.ProfilePhoto))
                    _fileUploader.DeleteFile(account.ProfilePhoto);

                var roleName = Roles.GetRoleBy(account.RoleId);
                var path = $"ProfilePhotos/{roleName}";
                picturePath = await _fileUploader.UploadAsync(command.ProfilePhoto, path, cancellation);
            }

            account.UpdatePersonalProfileAndMobileAdminPanel(command.Fullname, command.Username, command.Mobile, command.Email, picturePath);

            await _accountRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("پروفایل شما با موفقیت بروزرسانی شد.");
        }

        public async Task<OperationResult> EditSuperAdminProfile(EditSuperAdminProfile command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var account = await _accountRepository.GetAsync(command.Id, cancellation);
            if (account == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            if (await _accountRepository.ExistsAsync(x => x.Mobile == command.Mobile && x.Id != command.Id, cancellation))
                return op.Failed("شماره موبایل وارد شده در سیستم موجود است.");

            if (await _accountRepository.ExistsAsync(x => x.Email.ToLower() == command.Email.ToLower() && x.Id != command.Id, cancellation))
                return op.Failed("ایمیل وارد شده در سیستم موجود است.");

            if (!string.IsNullOrWhiteSpace(command.Password))
            {
                var newPasswordHash = _passwordHasher.Hash(command.Password);
                account.ChangePassword(newPasswordHash);
            }

            account.UpdateSuperAdmin(command.Email, command.Mobile);

            await _accountRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("پروفایل شما با موفقیت بروزرسانی شد.");
        }

        public async Task<OperationResult> Activate(long id, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var acc = await _accountRepository.GetAsync(id, cancellation);

            if (acc == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            if (!_authHelper.IsAdmin())
                return op.Failed("فقط ادمین مجاز به فعال‌سازی است.");

            acc.Activate();
            await _accountRepository.SaveChangesAsync(cancellation);
            return op.Succeeded();
        }

        public async Task<OperationResult> Deactivate(long id, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var acc = await _accountRepository.GetAsync(id, cancellation);

            if (acc == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            if (!_authHelper.IsAdmin())
                return op.Failed("فقط ادمین مجاز به غیرفعال‌سازی است.");

            var currentUserId = _authHelper.CurrentUserId();
            if (id != 1 && id != currentUserId
                &&
               (acc.RoleId > Roles.Admin || currentUserId == Roles.SuperAdmin))
                acc.Deactivate();
            else
                return op.Failed("امکان غیرفعال‌سازی این اکانت وجود ندارد.");

            await _accountRepository.SaveChangesAsync(cancellation);
            return op.Succeeded();
        }

        public async Task<EditAccount?> GetDetailsForAdmin(long id, CancellationToken cancellation)
        {
            return await _accountRepository.GetDetailsAsync(id, cancellation);
        }

        public async Task<List<AccountViewModel>> Search(AccountSearchModel searchModel, CancellationToken cancellation)
        {
            return await _accountRepository.SearchAsync(searchModel, cancellation);
        }

        public async Task<OperationResult> ResetPasswordByAdmin(long userId, string newPassword, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            if (!_authHelper.IsSuperAdmin())
                return operation.Failed("فقط سوپر ادمین مجاز به ریست رمز عبور است.");

            var account = await _accountRepository.GetAsync(userId, cancellation);
            if (account == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            var newPasswordHash = _passwordHasher.Hash(newPassword);
            account.ChangePassword(newPasswordHash);

            await _accountRepository.SaveChangesAsync(cancellation);

            return operation.Succeeded($"رمز عبور کاربر #{userId} با موفقیت ریست شد.");
        }

        public async Task<OperationResult> SendPasswordResetLink(SendPasswordResetLinkDto command, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            var account = await _accountRepository.GetByEmailAsync(command.Email, cancellation);

            if (account == null || account.RoleId != 1)
                return operation.Failed("حساب کاربری یافت نشد یا مجاز به این عملیات نیستید.");

            var token = Guid.NewGuid().ToString("N");
            var cacheKey = $"ResetPasswordToken_{command.Email.ToLower()}";

            _cache.Set(cacheKey, token, TimeSpan.FromMinutes(15));

            var baseUrl = _config["AppConfig:AdminBaseUrl"];

            var resetLink = $"{baseUrl}/Admin/ResetPassword?email={command.Email.ToLower()}&token={token}";

            var emailBody = $@"
            <div style='font-family: Tahoma, Arial, sans-serif; direction: rtl; background-color: #f4f6f9; padding: 30px; text-align: right;'>
                <div style='max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; border: 1px solid #e0e0e0;'>
        
                    <div style='background-color: #1A365D; color: #ffffff; padding: 20px; text-align: center; font-size: 18px; font-weight: bold;'>
                        سیستم مدیریت PharmaPlus
                    </div>
        
                    <div style='padding: 30px; color: #333333; line-height: 1.8; font-size: 14px;'>
                        <p><strong>کاربر گرامی،</strong></p>
                        <p>درخواستی مبنی بر بازیابی رمز عبور حساب مدیریت شما دریافت شده است. در صورت تأیید، لطفاً روی دکمه زیر کلیک کنید:</p>
            
                        <div style='text-align: center; margin: 35px 0;'>
                            <a href='{resetLink}' style='background-color: #1A365D; color: #ffffff; text-decoration: none; padding: 12px 30px; border-radius: 4px; font-weight: bold; display: inline-block;'>
                                بازیابی رمز عبور
                            </a>
                        </div>
            
                        <p style='color: #e53e3e; font-size: 13px; margin-bottom: 0;'>
                            ⚠️ توجه: این لینک به دلایل امنیتی تنها <strong>۱۵ دقیقه</strong> اعتبار دارد.
                        </p>
                        <p style='color: #718096; font-size: 12px; margin-top: 10px;'>
                            اگر شما این درخواست را ثبت نکرده‌اید، می‌توانید با خیال راحت این ایمیل را نادیده بگیرید.
                        </p>
                    </div>

                </div>
            </div>";
            await _emailService.SendEmailAsync(command.Email.ToLower(), "بازیابی رمز عبور پنل مدیریت", emailBody);

            return operation.Succeeded("لینک بازیابی رمز عبور به ایمیل شما ارسال شد.");
        }

        public async Task<OperationResult> ResetPasswordWithLink(ResetPasswordWithLinkDto command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var cacheKey = $"ResetPasswordToken_{command.Email.ToLower()}";

            if (!_cache.TryGetValue(cacheKey, out string savedToken) || savedToken != command.Token)
                return operation.Failed("لینک بازیابی نامعتبر یا منقضی شده است.");

            var account = await _accountRepository.GetByEmailAsync(command.Email.ToLower(), cancellation);
            if (account == null || !_authHelper.IsSuperAdmin())
                return operation.Failed("حساب کاربری نامعتبر است.");

            var hashPassword = _passwordHasher.Hash(command.NewPassword);
            account.ChangePassword(hashPassword);

            await _accountRepository.SaveChangesAsync(cancellation);

            _cache.Remove(cacheKey);

            return operation.Succeeded("رمز عبور با موفقیت تغییر کرد. اکنون می‌توانید وارد شوید.");
        }

        // ==========================================
        // Store API (JWT-Based)
        // ==========================================
        public async Task<OperationResult> SendLoginOtp(string mobile, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var sent = await GenerateCacheAndSendOtpAsync(mobile, OtpCachePrefixes.LoginOrRegister);
            return sent ? operation.Succeeded("کد تایید ارسال شد.") : operation.Failed("خطا در ارسال پیامک. لطفا بعدا تلاش کنید.");
        }

        public async Task<(OperationResult Result, JwtResponseDto? Data)> LoginOrRegisterWithOtp(string mobile, string code, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            if (!ValidateAndConsumeOtp(mobile, code, OtpCachePrefixes.LoginOrRegister))
                return (operation.Failed("کد وارد شده منقضی شده یا صحیح نیست."), null);

            var account = await _accountRepository.GetByMobileAsync(mobile, cancellation);

            if (account == null)
            {
                var roleName = Roles.GetRoleBy(Roles.Customer);
                var path = $"ProfilePhotos/{roleName}";

                var seed = Guid.NewGuid().ToString("N");
                var photo = _fileUploader.GenerateIdenticonAsFormFile(seed);
                var picturePath = await _fileUploader.UploadAsync(photo, path, cancellation);

                account = new Account(mobile, picturePath);

                _accountRepository.Create(account);
                await _accountRepository.SaveChangesAsync(cancellation);

                var cartResult = await _cartApplication.CreateCartAsync(account.Id, cancellation);
                if (!cartResult.IsSucceeded)
                    return (operation.Failed(cartResult.Message), null);
            }
            else if (!account.IsActive)
            {
                return (operation.Failed("حساب کاربری شما غیرفعال است."), null);
            }

            var role = await _roleRepository.GetAsync(account.RoleId, cancellation);
            var permissions = role?.Permissions?.Select(p => p.Code).ToList() ?? new List<int>();

            var dto = await GenerateAndSaveTokensAsync(account, permissions, false, cancellation);

            return (operation.Succeeded("ورود موفقیت‌آمیز بود."), dto);
        }

        public async Task<(OperationResult Result, JwtResponseDto? Data)> LoginToStoreViaUserPass(LoginToApiViaUserMobilePass command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var account = await _accountRepository.GetByUsernameOrMobileAsync(command.UsernameOrMobile.ToLower(), cancellation);

            if (account == null)
                return (op.Failed(ApplicationMessages.RecordNotFound), null);

            if (!account.IsActive)
                return (op.Failed("حساب کاربری شما غیرفعال است."), null);

            if (!account.IsCompleted() || string.IsNullOrWhiteSpace(account.Password))
                return (op.Failed("شما هنوز رمز عبور ندارید. لطفا ابتدا با شماره موبایل (OTP) وارد شوید و پروفایل خود را تکمیل کنید."), null);

            var (verified, needsUpgrade) = _passwordHasher.Check(account.Password, command.Password);
            if (!verified)
                return (op.Failed(ApplicationMessages.WrongUserPass), null);

            var role = await _roleRepository.GetAsync(account.RoleId, cancellation);
            var permissions = role?.Permissions?.Select(p => p.Code).ToList() ?? new List<int>();

            var dto = await GenerateAndSaveTokensAsync(account, permissions, command.RememberMe, cancellation);

            return (op.Succeeded("ورود موفقیت‌آمیز بود."), dto);
        }

        public async Task<OperationResult> LogoutFromStore(string token, CancellationToken cancellation)
        {
            var op = new OperationResult();
            if (string.IsNullOrWhiteSpace(token)) return op.Failed("توکن نامعتبر است");

            var tokenRecord = await _tokenRepository.GetByAccessTokenAsync(token, cancellation);
            if (tokenRecord == null) return op.Failed("توکن یافت نشد");

            await _tokenRepository.RevokeAsync(tokenRecord, cancellation);
            return op.Succeeded("خروج از حساب با موفقیت انجام شد.");
        }

        public async Task<(OperationResult Result, JwtResponseDto? Data)> RefreshToken(string refreshToken, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (string.IsNullOrWhiteSpace(refreshToken))
                return (op.Failed("توکن معتبر نیست."), null);

            var tokenRecord = await _tokenRepository.GetByRefreshTokenAsync(refreshToken, cancellation);

            if (tokenRecord == null)
                return (op.Failed("توکن یافت نشد."), null);

            if (tokenRecord.IsRefreshExpired())
                return (op.Failed("توکن بازیابی (Refresh Token) منقضی شده است. لطفا دوباره وارد شوید."), null);

            if (tokenRecord.IsRevoked)
                return (op.Failed("توکن بازیابی باطل شده است. لطفا دوباره وارد شوید."), null);

            var account = await _accountRepository.GetAsync(tokenRecord.UserId, cancellation);

            if (account == null)
                return (op.Failed(ApplicationMessages.RecordNotFound), null);

            if (!account.IsActive)
                return (op.Failed("حساب کاربری شما غیرفعال شده است و امکان تجدید توکن وجود ندارد."), null);

            tokenRecord.Revoke();
            await _tokenRepository.RevokeAsync(tokenRecord, cancellation);

            var role = await _roleRepository.GetAsync(account.RoleId, cancellation);
            var permissions = role?.Permissions?.Select(p => p.Code).ToList() ?? new List<int>();

            var dto = await GenerateAndSaveTokensAsync(account, permissions, rememberMe: false, cancellation);

            return (op.Succeeded("توکن با موفقیت تجدید شد."), dto);
        }

        public async Task<OperationResult> SendResetPasswordOtp(string mobile, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            var account = await _accountRepository.GetByMobileAsync(mobile, cancellation);
            if (account == null)
                return operation.Failed("کاربری با این شماره موبایل یافت نشد.");

            var sent = await GenerateCacheAndSendOtpAsync(mobile, OtpCachePrefixes.ResetPassword);

            return sent
                ? operation.Succeeded("کد تایید برای بازیابی رمز عبور به شماره شما ارسال شد.")
                : operation.Failed("خطا در ارسال پیامک. لطفاً مجدداً تلاش کنید.");
        }

        public async Task<OperationResult> ResetPasswordWithOtp(ResetPasswordWithOtpApi command, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            if (!ValidateAndConsumeOtp(command.Mobile, command.Code, OtpCachePrefixes.ResetPassword))
                return operation.Failed("کد وارد شده منقضی شده یا صحیح نیست.");

            var account = await _accountRepository.GetByMobileAsync(command.Mobile, cancellation);
            if (account == null)
                return operation.Failed("کاربری با این شماره موبایل یافت نشد.");

            var hashedPassword = _passwordHasher.Hash(command.Password);

            account.ChangePassword(hashedPassword);

            await _accountRepository.SaveChangesAsync(cancellation);

            return operation.Succeeded("رمز عبور شما با موفقیت تغییر کرد. اکنون می‌توانید وارد حساب کاربری خود شوید.");
        }

        // Profile Management (API)

        public async Task<OperationResult> UpdateProfileInfo(UpdateGeneralInfo command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var account = await _accountRepository.GetAsync(command.Id, cancellation);
            if (account == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            if (await _accountRepository.ExistsAsync(x => x.Username.ToLower() == command.Username.ToLower() && x.Id != command.Id, cancellation))
                return op.Failed($"کاربری با نام {command.Username} از قبل وجود دارد.");

            if (!string.IsNullOrWhiteSpace(command.Email))
            {
                if (await _accountRepository.ExistsAsync(x => x.Email.ToLower() == command.Email.ToLower() && x.Id != command.Id, cancellation))
                    return op.Failed("این ایمیل قبلاً توسط کاربر دیگری ثبت شده است.");
            }

            var picturePath = account.ProfilePhoto;
            if (command.ProfilePhoto != null && command.ProfilePhoto.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(account.ProfilePhoto))
                    _fileUploader.DeleteFile(account.ProfilePhoto);

                var roleName = Roles.GetRoleBy(account.RoleId);
                var path = $"ProfilePhotos/{roleName}";
                picturePath = await _fileUploader.UploadAsync(command.ProfilePhoto, path, cancellation);
            }

            account.UpdateProfileInfoApi(command.Fullname, command.Username, command.Email, picturePath);

            await _accountRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("پروفایل شما با موفقیت بروزرسانی شد.");
        }

        public async Task<OperationResult> RequestMobileChangeOtp(long userId, string newMobile, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            var account = await _accountRepository.GetAsync(userId, cancellation);
            if (account == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (account.Mobile == newMobile)
                return operation.Failed("این شماره، موبایل فعلی شماست.");

            if (await _accountRepository.ExistsAsync(x => x.Mobile == newMobile, cancellation))
                return operation.Failed("این شماره موبایل قبلاً ثبت شده است.");

            var sent = await GenerateCacheAndSendOtpAsync(newMobile, OtpCachePrefixes.ChangeMobile);
            return sent ? operation.Succeeded("کد تایید به شماره جدید ارسال شد.") : operation.Failed("خطا در ارسال پیامک. لطفا بعدا تلاش کنید.");
        }

        public async Task<OperationResult> ChangeMobileApi(ChangeMobileDto command, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            if (!ValidateAndConsumeOtp(command.Mobile, command.Code, OtpCachePrefixes.ChangeMobile))
                return operation.Failed("کد وارد شده منقضی شده یا صحیح نیست.");

            var account = await _accountRepository.GetAsync(command.Id, cancellation);
            if (account == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _accountRepository.ExistsAsync(x => x.Mobile == command.Mobile && x.Id != command.Id, cancellation))
                return operation.Failed("در این فاصله، شماره موبایل توسط شخص دیگری ثبت شده است.");

            account.ChangeMobile(command.Mobile);

            await _accountRepository.SaveChangesAsync(cancellation);

            return operation.Succeeded("شماره موبایل شما با موفقیت تغییر کرد.");
        }

        public async Task<OperationResult> SetPassword(SetPassword command, long userId, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            if (command.Id != userId)
                return operation.Failed("شما تنها مجاز به تغییر رمز عبور حساب کاربری خود هستید.");

            var account = await _accountRepository.GetAsync(userId, cancellation);
            if (account == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (!string.IsNullOrWhiteSpace(account.Password))
                return operation.Failed("رمز عبور از قبل تنظیم شده است.");

            if (command.Password != command.RePassword)
                return operation.Failed(ApplicationMessages.PasswordsNotMatch);

            var newPasswordHash = _passwordHasher.Hash(command.Password);

            account.SetPassword(newPasswordHash);

            await _accountRepository.SaveChangesAsync(cancellation);

            return operation.Succeeded("رمز عبور با موفقیت تنظیم شد.");
        }

        // ==========================================
        // Shared (Admin & Store)
        // ==========================================
        public async Task<AccountViewModel> GetAccountDetails(long id, CancellationToken cancellation)
        {
            var account = await _accountRepository.GetAsync(id, cancellation);

            if (account == null)
                return new();

            return new AccountViewModel()
            {
                Id = id,
                Fullname = account.Fullname,
                Mobile = account.Mobile,
                Username = account.Username.ToLower(),
                RoleName = Roles.GetRoleBy(account.RoleId),
                IsActive = account.IsActive,
                IsCompleted = account.IsCompleted(),
                HasPassword = !string.IsNullOrWhiteSpace(account.Password),
                ProfilePhoto = account.ProfilePhoto,
                CreationDate = account.CreationDate.ToFarsi(),
            };
        }

        public async Task<long> GetRoleIdByUsername(string username, CancellationToken cancellation)
        {
            var acc = await _accountRepository.GetByUsernameAsync(username.ToLower(), cancellation);
            if (acc == null)
                return 0;
            return acc.RoleId;
        }

        public async Task<bool> IsMobileExist(string mobile, CancellationToken cancellation)
        {
            var acc = await _accountRepository.GetByMobileAsync(mobile, cancellation);
            if (acc == null)
                return false;
            return true;
        }

        public async Task<OperationResult> ChangePassword(ChangePassword command, long userId, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            if (command.Id != userId)
                return operation.Failed("شما تنها مجاز به تغییر رمز عبور حساب کاربری خود هستید.");

            var account = await _accountRepository.GetAsync(userId, cancellation);
            if (account == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            var (verified, _) = _passwordHasher.Check(account.Password, command.CurrentPassword);
            if (!verified)
                return operation.Failed("رمز عبور فعلی اشتباه است.");

            if (command.Password != command.RePassword)
                return operation.Failed(ApplicationMessages.PasswordsNotMatch);

            var newPasswordHash = _passwordHasher.Hash(command.Password);

            account.ChangePassword(newPasswordHash);

            await _accountRepository.SaveChangesAsync(cancellation);

            return operation.Succeeded("رمز عبور با موفقیت تغییر یافت.");
        }

        public async Task<List<AccountViewModel>> GetAccounts(CancellationToken cancellation)
        {
            return await _accountRepository.GetAccountsAsync(cancellation);
        }

        private async Task<JwtResponseDto> GenerateAndSaveTokensAsync(Account account, List<int> permissions, bool rememberMe, CancellationToken cancellation)
        {
            var accessMinutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var am) ? am : 45;

            var refreshDays = rememberMe
                ? (int.TryParse(_config["Jwt:RefreshTokenDaysRemember"], out var rdRem) ? rdRem : 14)
                : (int.TryParse(_config["Jwt:RefreshTokenDays"], out var rd) ? rd : 3);

            var accessToken = _jwtService.GenerateAccessToken(account.Id, account.Username.ToLower(), account.RoleId, permissions, accessMinutes);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var accessExpiry = _jwtService.GetExpiration(accessToken);
            var refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);

            var tokenEntity = new Token(account.Id, accessToken, accessExpiry, refreshToken, refreshExpiry);
            await _tokenRepository.AddAsync(tokenEntity, cancellation); // have SaveChangesAsync() in itself

            return new JwtResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = accessExpiry,
                RefreshTokenExpiry = refreshExpiry
            };
        }

        private async Task<bool> GenerateCacheAndSendOtpAsync(string mobile, string cachePrefix)
        {
            var otpCode = new Random().Next(10000, 99999).ToString();
            var cacheKey = $"{cachePrefix}{mobile}";

            _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(2));

            return await _smsService.SendOtpAsync(mobile, otpCode);
        }

        private bool ValidateAndConsumeOtp(string mobile, string code, string cachePrefix)
        {
            var cacheKey = $"{cachePrefix}{mobile}";

            if (!_cache.TryGetValue(cacheKey, out string cachedCode) || cachedCode != code)
                return false;

            _cache.Remove(cacheKey);
            return true;
        }

        public async Task<bool> NeedsSetup(CancellationToken cancellation)
        {
            return await _accountRepository.NeedSetup(cancellation);
        }

        public async Task<OperationResult> Setup(SetupSuperAdminProfile command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var account = await _accountRepository.GetAsync(command.Id, cancellation);
            if (account == null)
                return op.Failed(ApplicationMessages.RecordNotFound);

            if (await _accountRepository.ExistsAsync(x => x.Mobile == command.Mobile && x.Id != command.Id, cancellation))
                return op.Failed("شماره موبایل وارد شده در سیستم موجود است.");

            if (await _accountRepository.ExistsAsync(x => x.Email.ToLower() == command.Email.ToLower() && x.Id != command.Id, cancellation))
                return op.Failed("ایمیل وارد شده در سیستم موجود است.");

            if (!string.IsNullOrWhiteSpace(command.Password))
            {
                var newPasswordHash = _passwordHasher.Hash(command.Password);
                account.ChangePassword(newPasswordHash);
            }
            else
                return op.Failed("کلمه عبور نمی‌تواند خالی باشد!");

            account.UpdateSuperAdmin(command.Email, command.Mobile);
            account.SetCompleted();

            await _accountRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("راه‌اندازی با موفقیت انجام شد. اکنون می‌توانید وارد سایت شوید.");
        }
    }

}