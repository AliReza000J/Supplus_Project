using System.Text.Json.Serialization;

namespace _01_PharmaPlusQuery.Contracts.Comment
{
    public interface ICommentQuery
    {
        Task<List<UserCommentQueryModel>?> GetUserComments(long userId, CancellationToken cancellation);
    }

    public class UserCommentQueryModel
    {
        public long Id { get; set; }
        public string? Message { get; set; }
        public string CreationDate { get; set; }
        public int Status { get; set; }
        public int Rate { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public string? Reply { get; set; }
        public string? ReplyDate { get; set; }
        public string ProductSlug { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string CategoryFullSlug { get; set; }

        [JsonIgnore]
        public long RecordId { get; set; }
    }
}
