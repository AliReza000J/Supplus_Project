namespace _0_Framework.Application
{
    // 1. DTOs
    public record ZibalRequestResponse(
    int Result,
    long? TrackId,
    string? Message
);

    public record ZibalVerifyResponse(
        int Result,
        long? Amount,
        long? RefNumber,
        string? Message
    );

    // 2. Interface
    public interface IZibalService
    {
        Task<(long trackId, string message)> RequestPayment(double amount, string orderId);
        Task<(int result, double amount, long refNumber, string message)> VerifyPayment(long trackId);
    }
}
