namespace CommentManagement.Application.Contracts.Comment
{
    public class CommentSearchModel
    {
        public int? Type { get; set; } // Product, Article
        public string? Username { get; set; }
        public long? OwnerRecordId { get; set; }
        public bool? IsConfirmed { get; set; }
        public bool? IsCanceled { get; set; }
        public int? Rate { get; set; }

    }
}
