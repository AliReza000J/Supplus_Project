using _0_Framework.Application;
using _0_Framework.Infrastructure;
using BlogManagement.Infrastructure.EFCore;
using CommentManagement.Application.Contracts.Comment;
using CommentManagement.Domain.CommentAgg;
using CommnetManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.EFCore;

namespace CommentManagement.Infrastructure.EFCore.Repository
{
    public class CommentRepository : BaseRepository<long, Comment>, ICommentRepository
    {
        private readonly CommentContext _context;
        private readonly ShopContext _shopContext;
        private readonly BlogContext _blogContext;
        public CommentRepository(CommentContext context, ShopManagement.Infrastructure.EFCore.ShopContext shopContext, BlogContext blogContext) : base(context)
        {
            _context = context;
            _shopContext = shopContext;
            _blogContext = blogContext;
        }

        public async Task<List<CommentViewModel>> Search(CommentSearchModel searchModel, CancellationToken cancellation)
        {
            var query = _context.Comments.AsNoTracking()
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Username = c.Username,
                    Message = c.Message,
                    RecordId = c.RecordId,
                    RecordName = "", // fill later
                    Type = c.Type,
                    IsConfirmed = c.IsConfirmed,
                    IsCanceled = c.IsCanceled,
                    CommentDate = c.CreationDate.ToFarsi(),
                    Rate = c.Rate,
                    LikeCount = c.LikeCount,
                    DislikeCount = c.DislikeCount,
                    Reply = c.AdminReply,
                    IsBuyer = c.IsBuyer ?? false,
                    LastModified = c.LastModified.ToFarsi(),
                    ReplyDate = c.AdminReplyDate.HasValue ? c.AdminReplyDate.Value.ToFarsi() : null
                });

            if (searchModel != null)
            {
                if (searchModel.Type.HasValue)
                    query = query.Where(x => x.Type == searchModel.Type.Value);

                if (!string.IsNullOrWhiteSpace(searchModel.Username))
                    query = query.Where(x => x.Username.Contains(searchModel.Username));

                if (searchModel.RecordId.HasValue)
                    query = query.Where(x => x.RecordId == searchModel.RecordId.Value);

                if (searchModel.IsConfirmed.HasValue)
                    query = query.Where(x => x.IsConfirmed == searchModel.IsConfirmed.Value);

                if (searchModel.IsCanceled.HasValue)
                    query = query.Where(x => x.IsCanceled == searchModel.IsCanceled.Value);

                if (searchModel.Rate.HasValue)
                    query = query.Where(x => x.Rate == searchModel.Rate.Value);

                if (searchModel.HasMessage == true)
                    query = query.Where(x => !string.IsNullOrWhiteSpace(x.Message));

                if (searchModel.ShowPendingOnly == true)
                    query = query.Where(x => (!x.IsCanceled && !x.IsConfirmed));
            }

            var list = await query.OrderByDescending(x => x.Id).ToListAsync(cancellation);

            // Resolve OwnerName for product comments
            var productIds = list.Where(l => l.Type == (int)CommentType.Product).Select(l => l.RecordId).Distinct().ToList();
            if (productIds.Any())
            {
                var names = await _shopContext.Products
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Name })
                    .ToDictionaryAsync(p => p.Id, p => p.Name,
                    cancellation);

                foreach (var item in list.Where(x => x.Type == (int)CommentType.Product))
                {
                    if (names.TryGetValue(item.RecordId, out var name))
                        item.RecordName = name;
                }
            }

            var articleIds = list.Where(l => l.Type == (int)CommentType.Article).Select(l => l.RecordId).Distinct().ToList();
            if (articleIds.Any())
            {
                var names = await _blogContext.Articles
                    .Where(p => articleIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Title })
                    .ToDictionaryAsync(p => p.Id, p => p.Title, cancellation);

                foreach (var item in list.Where(x => x.Type == (int)CommentType.Article))
                {
                    if (names.TryGetValue(item.RecordId, out var name))
                        item.RecordName = name;
                }
            }

            return list;
        }

        public async Task<CommentViewModel> GetDetails(long id, CancellationToken cancellation)
        {
            var c = await _context.Comments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id,
                cancellation);

            if (c == null) return null!;

            var vm = new CommentViewModel
            {
                Id = c.Id,
                Username = c.Username,
                Message = c.Message,
                RecordId = c.RecordId,
                RecordName = "", // fill below
                Type = c.Type,
                IsConfirmed = c.IsConfirmed,
                IsCanceled = c.IsCanceled,
                CommentDate = c.CreationDate.ToFarsi(),
                Rate = c.Rate,
                LikeCount = c.LikeCount,
                DislikeCount = c.DislikeCount,
                Reply = c.AdminReply,
                IsBuyer = c.IsBuyer ?? false,
                LastModified = c.LastModified.ToFarsi(),
                ReplyDate = c.AdminReplyDate.HasValue ? c.AdminReplyDate.Value.ToFarsi() : null
            };

            if (c.Type == (int)CommentType.Product)
            {
                var product = await _shopContext.Products.Select(p => new { p.Id, p.Name }).FirstOrDefaultAsync(p => p.Id == vm.RecordId, cancellation);
                vm.RecordName = product?.Name ?? "";
            }

            if (c.Type == (int)CommentType.Article)
            {
                var product = await _blogContext.Articles.Select(p => new { p.Id, p.Title }).FirstOrDefaultAsync(p => p.Id == vm.RecordId, cancellation);
                vm.RecordName = product?.Title ?? "";
            }

            return vm;
        }

        // reactions
        public async Task<bool> HasUserLiked(long commentId, long userId, CancellationToken cancellation)
        {
            return await _context.CommentReactions
                .AsNoTracking()
                .AnyAsync(r => r.CommentId == commentId
                && r.UserId == userId
                && r.Reaction == ReactionType.Like,
                cancellation);
        }

        public async Task<bool> HasUserDisliked(long commentId, long userId, CancellationToken cancellation)
        {
            return await _context.CommentReactions
                .AsNoTracking()
                .AnyAsync(r => r.CommentId == commentId &&
                r.UserId == userId &&
                r.Reaction == ReactionType.Dislike,
                cancellation);
        }

        public void AddReaction(CommentReaction reaction)
        {
            _context.CommentReactions.Add(reaction);
        }

        public void RemoveReaction(CommentReaction reaction)
        {
            _context.CommentReactions.Remove(reaction);
        }

        public async Task<CommentReaction?> GetUserReaction(long commentId, long userId, CancellationToken cancellation)
        {
            return await _context.CommentReactions.FirstOrDefaultAsync(
                r => r.CommentId == commentId && r.UserId == userId, cancellation);
        }

        public async Task<bool> OwnsComment(long commentId, long userId, CancellationToken cancellation)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId, cancellation);
            return comment?.UserId == userId && comment != null;
        }

        public async Task<bool> HasUserCommented(long userId, int type, long ownerRecordId, CancellationToken cancellation)
        {
            return await _context.Comments.AnyAsync(x =>
            x.UserId == userId &&
            x.Type == type &&
            x.RecordId == ownerRecordId &&
            !x.IsCanceled,
            cancellation);
        }
    }
}