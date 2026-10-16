using _0_Framework.Domain;
using _0_Framework.Application;

namespace CommentManagement.Domain.CommentAgg
{
    public class Comment : EntityBase
    {
        public string Username { get; private set; }
        public long UserId { get; set; }
        public string? Message { get; private set; }
        public long RecordId { get; private set; }
        public int Type { get; private set; } // store enum as int for EF mapping
        public int Rate { get; private set; }
        public int LikeCount { get; private set; }
        public int DislikeCount { get; private set; }
        public bool IsConfirmed { get; private set; }
        public bool IsCanceled { get; private set; }
        public bool? IsBuyer { get; private set; } = false;

        public string? AdminReply { get; private set; }
        public DateTime? AdminReplyDate { get; private set; }
        public DateTime? LastModified { get; private set; }


        protected Comment() { }

        public Comment(string username, long userId, string? message,
            long ownerRecordId, int type, int rate)
        {
            Username = username;
            UserId = userId;
            Message = message;
            RecordId = ownerRecordId;
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
        public void EditMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                Message = message;

            IsConfirmed = false;
            IsCanceled = false;
            LastModified = DateTime.Now;
        }

        public void ClearMessage()
        {
            Message = null;
            LastModified = DateTime.Now;
        }

        public void EditRate(int rate)
        {
            if (rate >= 1 && rate <= 5)
                Rate = rate;

            IsConfirmed = true;
            IsCanceled = false;
            LastModified = DateTime.Now;
        }

        public void Confirm()
        {
            IsConfirmed = true;
            IsCanceled = false;
        }

        public void Cancel()
        {
            IsCanceled = true;
            IsConfirmed = false;
        }

        public void AddLike()
        {
            LikeCount++;
        }

        public void AddDislike()
        {
            DislikeCount++;
        }

        public void AddReply(string reply)
        {
            AdminReply = reply;
            AdminReplyDate = DateTime.Now;
        }

        public void EditReply(string reply)
        {
            AdminReply = reply;
            AdminReplyDate = DateTime.Now;
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
            IsConfirmed = true;
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

        public void BuyerComment() => IsBuyer = true;
    }
}
