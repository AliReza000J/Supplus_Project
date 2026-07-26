namespace _0_Framework.Application
{
    public class ImageCleanupResult
    {
        public string ModuleName { get; set; }
        public int CheckedFilesCount { get; set; }
        public int DeletedFilesCount { get; set; }
    }

    public interface IModuleImageCleanupService
    {
        Task<ImageCleanupResult> CleanupAsync(int keepRecentFilesForDays = 1);
    }
}
