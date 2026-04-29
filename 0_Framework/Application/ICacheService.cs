namespace _0_Framework.Application
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            long? size = null,
            CancellationToken cancellationToken = default);

        void Remove(string key);
    }
}
