using _0_Framework.Domain;

namespace AccountManagement.Domain.ShippingInfoAgg;

public class ShippingInfo : EntityBase
{
    public long UserId { get; private set; }
    public string ReceiverFullName { get; private set; }
    public string ReceiverMobile { get; private set; }
    public string ReceiverAddress { get; private set; }
    public string ReceiverZipCode { get; private set; }

    public ShippingInfo() { }

    public ShippingInfo(long userId, string receiverFullName, string receiverMobile, string receiverAddress, string receiverZipCode)
    {
        UserId = userId;
        ReceiverFullName = receiverFullName;
        ReceiverMobile = receiverMobile;
        ReceiverAddress = receiverAddress;
        ReceiverZipCode = receiverZipCode;
    }

    public void Edit(string receiverFullName, string receiverMobile, string receiverAddress, string receiverZipCode)
    {
        ReceiverFullName = receiverFullName;
        ReceiverMobile = receiverMobile;
        ReceiverAddress = receiverAddress;
        ReceiverZipCode = receiverZipCode;
    }
}