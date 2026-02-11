using BlogManagement.Application.Contracts.Article;
using BlogManagement.Application.Contracts.ArticleCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ServiceHost.Areas.Admin.Pages.Blog.Articles
{
    public class CreateModel : PageModel
    {
        public CreateArticle Command;
        public SelectList ArticleCategories;

        private readonly IArticleApplication _articleApplication;
        private readonly IArticleCategoryApplication _articleCategoryApplication;

        public CreateModel(IArticleApplication articletApplication, IArticleCategoryApplication articleCategoryApplication)
        {
            _articleApplication = articletApplication;
            _articleCategoryApplication = articleCategoryApplication;
        }

        public async Task OnGet(CancellationToken cancellation)
        {
            ArticleCategories = new SelectList(await _articleCategoryApplication.GetArticleCategories(cancellation), "Id", "Name");
        }

        public async Task<IActionResult> OnPost(CreateArticle command, CancellationToken cancellation)
        {
            var result = await _articleApplication.Create(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index");
        }
    }
}
