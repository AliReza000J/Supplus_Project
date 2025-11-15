using _0_Framework.Application;

namespace CommentManagement.Application.Contracts.Comment
{
    public interface ICommentApplication
    {
        // --- CRUD اصلی ---
        OperationResult Add(AddComment command, string userIdentifier, string username);                 // ثبت کامنت / امتیاز
        OperationResult Edit(EditComment command, string userIdentifier);               // ویرایش کامنت (توسط کاربر )
        OperationResult Delete(long id, string userIdentifier);                         // حذف کامل (درصورت نیاز)
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
