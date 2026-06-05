using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.ShippingInfo;

public interface IShippingInfoApplication
{
    Task<OperationResult> CreateAsync(CreateShippingInfoCommand command, CancellationToken cancellation);
    Task<OperationResult> EditAsync(EditShippingInfoCommand command, CancellationToken cancellation);
    Task<OperationResult> DeleteAsync(long id, long userId, CancellationToken cancellation);
    Task<EditShippingInfoCommand?> GetDetailsAsync(long id, CancellationToken cancellation);
    Task<List<ShippingInfoViewModel>> GetUserAddressesAsync(long userId, CancellationToken cancellation);
}

public class ShippingInfoViewModel
{
    public long Id { get; set; }
    public string ReceiverFullName { get; set; }
    public string ReceiverMobile { get; set; }
    public string ReceiverAddress { get; set; }
    public string ReceiverZipCode { get; set; }
}

public class CreateShippingInfoDto
{
    [Display(Name = "نام و نام‌ خانوادگی گیرنده")]
    public string? ReceiverFullName { get; set; }

    private string? _receiverMobile;
    [Display(Name = "شماره موبایل گیرنده")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
    public string? ReceiverMobile
    {
        get => _receiverMobile;
        set => _receiverMobile = value?.ToEnglishNumber();
    }

    [Display(Name = "نشانی گیرنده")]
    [Required(ErrorMessage = ValidationMessages.IsRequired)]
    public string ReceiverAddress { get; set; }

    private string _receiverZipCode;
    [Display(Name = "کد پستی")]
    [Required(ErrorMessage = ValidationMessages.IsRequired)]
    [RegularExpression(@"^\d{10}$", ErrorMessage = ValidationMessages.InvalidInput)]
    public string ReceiverZipCode
    {
        get => _receiverZipCode;
        set => _receiverZipCode = value.ToEnglishNumber();
    }
}

public class CreateShippingInfoCommand : CreateShippingInfoDto
{
    public long UserId { get; set; }
}

public class EditShippingInfoDto : CreateShippingInfoDto
{
    public long Id { get; set; }
}

public class EditShippingInfoCommand : CreateShippingInfoCommand
{
    public long Id { get; set; }
}
