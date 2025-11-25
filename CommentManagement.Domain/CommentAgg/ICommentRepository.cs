using _0_Framework.Domain;
using CommentManagement.Application.Contracts.Comment;

namespace CommentManagement.Domain.CommentAgg
{
    public interface ICommentRepository : IRepository<long, Comment>
    {
        Task<List<CommentViewModel>> Search(CommentSearchModel searchModel);
        Task<CommentViewModel> GetDetails(long id);
        Task<bool> OwnsComment(long commentId, string userIdentifier);

        // reactions
        Task<bool> HasUserLiked(long commentId, string userIdentifier);
        Task<bool> HasUserDisliked(long commentId, string userIdentifier);

        // reaction add/remove helpers for repository-level operations
        void AddReaction(CommentReaction reaction);
        void RemoveReaction(CommentReaction reaction);
        Task<CommentReaction?> GetUserReaction(long commentId, string userIdentifier);
        Task<bool> HasUserCommented(string userIdentifier, int type, long ownerRecordId);
    }
}
