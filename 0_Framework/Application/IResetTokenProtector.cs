namespace _0_Framework.Application
{
    public interface IResetTokenProtector
    {
        string Protect(string rawToken);
        bool Verify(string rawToken, string protectedToken);
    }

}
