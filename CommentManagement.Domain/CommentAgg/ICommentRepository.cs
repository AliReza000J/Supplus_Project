using _0_Framework.Domain;
using CommentManagement.Application.Contracts.Comment;

namespace CommentManagement.Domain.CommentAgg
{
    public interface ICommentRepository : IRepository<long, Comment>
    {
        Task<List<CommentViewModel>> Search(CommentSearchModel searchModel, CancellationToken cancellation);
        Task<CommentViewModel> GetDetails(long id, CancellationToken cancellation);
        Task<bool> OwnsComment(long commentId, long userId, CancellationToken cancellation);

        // reactions
        Task<bool> HasUserLiked(long commentId, long userId, CancellationToken cancellation);
        Task<bool> HasUserDisliked(long commentId, long userId, CancellationToken cancellation);

        // reaction add/remove helpers for repository-level operations
        void AddReaction(CommentReaction reaction);
        void RemoveReaction(CommentReaction reaction);
        Task<CommentReaction?> GetUserReaction(long commentId, long userId, CancellationToken cancellation);
        Task<bool> HasUserCommented(long userId, int type, long ownerRecordId, CancellationToken cancellation);
    }
}
