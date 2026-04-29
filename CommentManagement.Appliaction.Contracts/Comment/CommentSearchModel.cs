using _0_Framework.Application;

namespace CommentManagement.Application.Contracts.Comment
{
    public class CommentSearchModel
    {
        public int? Type { get; set; } // Product, Article

        private string? _username;
        public string? Username
        {
            get => _username;
            set => _username = value?.ToEnglishNumber();
        }

        public long? RecordId { get; set; }
        public bool? IsConfirmed { get; set; }
        public bool? IsCanceled { get; set; }
        public int? Rate { get; set; }
        public bool HasMessage { get; set; } = true;
        public bool ShowPendingOnly { get; set; } = false;
    }
}
