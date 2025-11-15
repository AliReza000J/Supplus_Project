namespace CommentManagement.Application.Contracts.Comment
{
    public class CommentViewModel
    {
        public long Id { get; set; }
        public string Username { get; set; }
        //public string? Email { get; set; }
        public string? Message { get; set; }
        public long OwnerRecordId { get; set; }
        public string OwnerName { get; set; }
        public int Type { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsCanceled { get; set; }
        public string CommentDate { get; set; }
        public int Rate { get; set; }

        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public bool? IsBuyer { get; set; }

        public string? Reply { get; set; }
        public string? ReplyDate { get; set; }

        public bool? LikedByCurrentUser { get; set; }
        public bool? DislikedByCurrentUser { get; set; }
    }
}
