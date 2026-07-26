using _0_Framework.Application;
using AccountManagement.Application.Contracts.ShippingInfo;
using AccountManagement.Domain.AccountAgg;
using AccountManagement.Domain.ShippingInfoAgg;

namespace AccountManagement.Application;

public class ShippingInfoApplication : IShippingInfoApplication
{
    private readonly IShippingInfoRepository _shippingInfoRepository;
    private readonly IAccountRepository _accountRepository;

    public ShippingInfoApplication(IShippingInfoRepository shippingInfoRepository, IAccountRepository accountRepository)
    {
        _shippingInfoRepository = shippingInfoRepository;
        _accountRepository = accountRepository;
    }

    public async Task<OperationResult> CreateAsync(CreateShippingInfoCommand command, CancellationToken cancellation)
    {
        var op = new OperationResult();

        var acc = await _accountRepository.GetAsync(command.UserId, cancellation);

        if (command.UserId <= 0 || acc == null)
            return op.Failed("کاربر یافت نشد.");

        if (await _shippingInfoRepository.ExistsAsync(
            x => (x.ReceiverAddress == command.ReceiverAddress ||
            x.ReceiverZipCode == command.ReceiverZipCode) &&
            x.UserId == command.UserId
            , cancellation))
            return op.Failed("این آدرس یا کدپستی قبلا برای شما ثبت شده است.");

        var receiverName = string.IsNullOrWhiteSpace(command.ReceiverFullName) ? acc.Fullname : command.ReceiverFullName;
        var receiverMobile = string.IsNullOrWhiteSpace(command.ReceiverMobile) ? acc.Mobile : command.ReceiverMobile;

        if (string.IsNullOrWhiteSpace(receiverName)) return op.Failed("نام گیرنده نمی‌تواند خالی باشد.");

        var shippingInfo = new ShippingInfo(
            command.UserId,
            receiverName,
            receiverMobile,
            command.ReceiverAddress,
            command.ReceiverZipCode);

        _shippingInfoRepository.Create(shippingInfo);
        await _shippingInfoRepository.SaveChangesAsync(cancellation);
        return op.Succeeded();
    }

    public async Task<OperationResult> EditAsync(EditShippingInfoCommand command, CancellationToken cancellation)
    {
        var op = new OperationResult();

        var shippingInfo = await _shippingInfoRepository.GetAsync(command.Id, cancellation);
        if (shippingInfo == null) return op.Failed("خطا در بازیابی اطلاعات");

        if (shippingInfo.UserId != command.UserId) return op.Failed("عملیات مجاز نیست");

        var acc = await _accountRepository.GetAsync(command.UserId, cancellation);

        if (command.UserId <= 0 || acc == null)
            return op.Failed("کاربر یافت نشد.");

        if (await _shippingInfoRepository.ExistsAsync(
            x => (x.ReceiverAddress == command.ReceiverAddress ||
            x.ReceiverZipCode == command.ReceiverZipCode) &&
            x.UserId == command.UserId && x.Id != command.Id
            , cancellation))
            return op.Failed("این آدرس یا کدپستی قبلا برای شما ثبت شده است.");

        var receiverName = string.IsNullOrWhiteSpace(command.ReceiverFullName) ? acc.Fullname : command.ReceiverFullName;
        var receiverMobile = string.IsNullOrWhiteSpace(command.ReceiverMobile) ? acc.Mobile : command.ReceiverMobile;

        if (string.IsNullOrWhiteSpace(receiverName)) return op.Failed("نام گیرنده نمی‌تواند خالی باشد.");

        shippingInfo.Edit(
            receiverName,
            receiverMobile,
            command.ReceiverAddress,
            command.ReceiverZipCode);

        await _shippingInfoRepository.SaveChangesAsync(cancellation);
        return op.Succeeded();
    }

    public async Task<OperationResult> DeleteAsync(long id, long userId, CancellationToken cancellation)
    {
        var op = new OperationResult();

        if (userId <= 0)
            return op.Failed("کاربر نامعتبر است.");

        var shippingInfo = await _shippingInfoRepository.GetAsync(id, cancellation);
        if (shippingInfo == null)
            return op.Failed("خطا در بازیابی اطلاعات");

        if (shippingInfo.UserId != userId)
            return op.Failed("عملیات مجاز نیست");

        _shippingInfoRepository.Delete(shippingInfo);

        await _shippingInfoRepository.SaveChangesAsync(cancellation);

        return op.Succeeded();
    }

    public async Task<EditShippingInfoCommand?> GetDetailsAsync(long id, CancellationToken cancellation)
    {
        var shippingInfo = await _shippingInfoRepository.GetAsync(id, cancellation);
        if (shippingInfo == null) return null;

        return new EditShippingInfoCommand
        {
            Id = shippingInfo.Id,
            UserId = shippingInfo.UserId,
            ReceiverFullName = shippingInfo.ReceiverFullName,
            ReceiverMobile = shippingInfo.ReceiverMobile,
            ReceiverAddress = shippingInfo.ReceiverAddress,
            ReceiverZipCode = shippingInfo.ReceiverZipCode
        };
    }

    public async Task<List<ShippingInfoViewModel>> GetUserAddressesAsync(long userId, CancellationToken cancellation)
    {
        var addresses = await _shippingInfoRepository.GetAllByUserIdAsync(userId, cancellation);

        return addresses.Select(x => new ShippingInfoViewModel
        {
            Id = x.Id,
            ReceiverFullName = x.ReceiverFullName,
            ReceiverMobile = x.ReceiverMobile,
            ReceiverAddress = x.ReceiverAddress,
            ReceiverZipCode = x.ReceiverZipCode
        }).ToList();
    }
}