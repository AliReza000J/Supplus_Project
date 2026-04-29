using BlogManagement.Application.Contracts.Article;
using BlogManagement.Application.Contracts.ArticleCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Blog.Articles
{
    public class EditModel : PageModel
    {
        public EditArticle? Command;
        public SelectList? ArticleCategories;

        private readonly IArticleApplication _articleApplication;
        private readonly IArticleCategoryApplication _articleCategoryApplication;

        public EditModel(IArticleApplication articleApplication, IArticleCategoryApplication articleCategoryApplication)
        {
            _articleApplication = articleApplication;
            _articleCategoryApplication = articleCategoryApplication;
        }

        public async Task OnGet(long id, CancellationToken cancellation)
        {
            Command = await _articleApplication.GetDetails(id, cancellation);
            ArticleCategories = new SelectList(await _articleCategoryApplication.GetArticleCategories(cancellation), "Id", "Name");
        }

        public async Task<IActionResult> OnPost(EditArticle command, CancellationToken cancellation)
        {
            var result = await _articleApplication.Edit(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index");
        }
    }
}
