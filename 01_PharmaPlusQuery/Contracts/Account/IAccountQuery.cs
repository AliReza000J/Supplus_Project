namespace _01_PharmaPlusQuery.Contracts.Account
{
    public interface IAccountQuery
    {
        Task<AccountOverviewQueryModel> GetUserOverview(long userId, CancellationToken cancellationToken);
    }
}
