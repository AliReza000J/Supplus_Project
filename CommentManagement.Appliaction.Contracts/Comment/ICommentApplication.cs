using _0_Framework.Application;

namespace CommentManagement.Application.Contracts.Comment
{
    public interface ICommentApplication
    {
        // --- CRUD اصلی ---
        OperationResult Add(AddComment command);                 // ثبت کامنت / امتیاز
        OperationResult Edit(EditComment command);               // ویرایش کامنت (توسط کاربر یا ادمین)
        OperationResult Delete(long id);                         // حذف کامل (درصورت نیاز)
        OperationResult Confirm(long id);                        // تأیید توسط ادمین
        OperationResult Cancel(long id);                         // رد توسط ادمین

        // --- تعامل ادمین ---
        OperationResult Reply(long id, string reply);             // پاسخ ادمین به کاربر

        // --- تعامل کاربران ---
        OperationResult Like(long id, string userIdentifier);     // لایک (کاربر)
        OperationResult Dislike(long id, string userIdentifier);  // دیسلایک (کاربر)

        // --- Query ---
        CommentViewModel GetDetails(long id);
        List<CommentViewModel> Search(CommentSearchModel searchModel);
    }
}
