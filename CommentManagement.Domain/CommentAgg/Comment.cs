using _0_Framework.Domain;

namespace CommentManagement.Domain.CommentAgg
{
    public class Comment : BaseEntity
    {
        public string Username { get; private set; }
        public string UserIdentifier { get; set; } = string.Empty;
        public string? Message { get; private set; }
        public long OwnerRecordId { get; private set; }
        public int Type { get; private set; } // store enum as int for EF mapping
        public int Rate { get; private set; }
        public int LikeCount { get; private set; }
        public int DislikeCount { get; private set; }
        public bool IsConfirmed { get; private set; }
        public bool IsCanceled { get; private set; }
        public bool? IsBuyer { get; private set; }

        public string? AdminReply { get; private set; }
        public DateTime? AdminReplyDate { get; private set; }
        public DateTime? LastModified { get; private set; }


        protected Comment() { }

        public Comment(string username, string userIdentifier, string? message,
            long ownerRecordId, int type, int rate)
        {
            Username = username;
            UserIdentifier = userIdentifier;
            Message = message;
            OwnerRecordId = ownerRecordId;
            Type = type;

            if (rate != 0)
                Rate = Math.Clamp(rate, 1, 5);

            LikeCount = 0;
            DislikeCount = 0;
            IsConfirmed = false;
            IsCanceled = false;
            LastModified = DateTime.Now;
        }

        // user
        public void Edit(string? message, int? rate)
        {
            if (!string.IsNullOrWhiteSpace(message))
                Message = message;

            if (rate.HasValue && rate.Value is >= 1 and <= 5)
                Rate = rate.Value;

            LastModified = DateTime.Now;
        }

        public void Confirm()
        {
            IsConfirmed = true;
            IsCanceled = false;
            LastModified = DateTime.Now;
        }

        public void Cancel()
        {
            IsCanceled = true;
            IsConfirmed = false;
            LastModified = DateTime.Now;
        }

        public void AddLike()
        {
            LikeCount++;
            LastModified = DateTime.Now;
        }

        public void AddDislike()
        {
            DislikeCount++;
            LastModified = DateTime.Now;
        }

        public void AddReply(string reply)
        {
            AdminReply = reply;
            AdminReplyDate = DateTime.Now;
            LastModified = DateTime.Now;
        }

        public void EditReply(string reply)
        {
            AdminReply = reply;
            AdminReplyDate = DateTime.Now;
            LastModified = DateTime.Now;
        }

        public void RateComment(int rate)
        {
            if (rate is >= 1 and <= 5)
            {
                Rate = rate;
                LastModified = DateTime.Now;
            }
        }

        // Remove => soft-delete
        public void Remove()
        {
            IsCanceled = true;
            IsConfirmed = false;
            Message = "[این نظر توسط نویسنده آن حذف شده است]";
            LastModified = DateTime.Now;
        }

        public void RemoveReactionLike()
        {
            LikeCount = Math.Max(0, LikeCount - 1);
        }

        public void RemoveReactionDislike()
        {
            DislikeCount = Math.Max(0, DislikeCount - 1);
        }
    }
}
