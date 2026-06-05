using _0_Framework.Domain;

namespace BlogManagement.Domain.UserArticleInteractionAgg
{
    public class ArticleView : EntityBase
    {
        public long ArticleId { get; set; }
        public long UserId { get; set; }

        protected ArticleView()
        {

        }
        public ArticleView(long articleId, long userId)
        {
            ArticleId = articleId;
            UserId = userId;
        }
    }
}
