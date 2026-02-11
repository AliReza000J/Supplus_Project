namespace CommentManagement.Domain.CommentAgg
{
    public class CommentReaction
    {
        public long Id { get; set; }
        public long CommentId { get; set; }
        public long UserId { get; set; }
        public ReactionType Reaction { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Comment? Comment { get; set; }

        protected CommentReaction() { }

        public CommentReaction(long commentId, long userId, ReactionType reaction)
        {
            CommentId = commentId;
            UserId = userId;
            Reaction = reaction;
            CreatedAt = DateTime.Now;
        }
    }
}
