namespace _0_Framework.Application
{
    public interface IResetTokenGenerator
    {
        string Generate(); // returns raw token safe for URL (use base64url)
    }
}
