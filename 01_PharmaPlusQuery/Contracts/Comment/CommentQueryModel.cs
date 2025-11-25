namespace _01_LampshadeQuery.Contracts.Comment
{
    public class CommentQueryModel
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public long UserId { get; set; }
        public string? Message { get; set; }
        public string CreationDate { get; set; }
        //public long ParentId { get; set; }
        public long OwnerRecordId { get; set; }
        public int Type { get; set; }
        public int Rate { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public bool? IsBuyer { get; set; }
        public string? Reply { get; set; }
        public string? ReplyDate { get; set; }
        public string? LastModifedDate { get; set; }
    }
}
