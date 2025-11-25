namespace CommentManagement.Domain.CommentAgg
{
    public class CommentReaction
    {
        public long Id { get; set; }
        public long CommentId { get; set; }
        public string UserIdentifier { get; set; } = string.Empty; // e.g. userId or ip or cookie
        public ReactionType Reaction { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Comment? Comment { get; set; }

        protected CommentReaction() { }

        public CommentReaction(long commentId, string userIdentifier, ReactionType reaction)
        {
            CommentId = commentId;
            UserIdentifier = userIdentifier;
            Reaction = reaction;
            CreatedAt = DateTime.Now;
        }
    }
}
