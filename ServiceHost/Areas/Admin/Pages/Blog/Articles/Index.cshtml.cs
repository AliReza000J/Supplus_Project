using BlogManagement.Application.Contracts.Article;
using BlogManagement.Application.Contracts.ArticleCategory;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ServiceHost.Areas.Admin.Pages.Blog.Articles
{
    public class IndexModel : PageModel
    {
        public ArticleSearchModel? SearchModel;
        public List<ArticleViewModel>? Articles;
        public SelectList? ArticleCategories;

        private readonly IArticleApplication _articleApplication;
        private readonly IArticleCategoryApplication _articleCategoryApplication;

        public IndexModel(IArticleApplication articleApplication,
            IArticleCategoryApplication articleCategoryApplication)
        {
            _articleApplication = articleApplication;
            _articleCategoryApplication = articleCategoryApplication;
        }

        public async Task OnGet(ArticleSearchModel searchModel, CancellationToken cancellation)
        {
            ArticleCategories = new SelectList(await _articleCategoryApplication.GetArticleCategories(cancellation), "Id", "Name");
            Articles = await _articleApplication.Search(searchModel, cancellation);
        }
    }
}
