using _0_Framework.Application;

namespace CommentManagement.Application.Contracts.Comment
{
    public interface ICommentApplication
    {
        // --- main CRUD ---
        Task<OperationResult> AddAsync(AddComment command, string userIdentifier, string username);                 // ثبت کامنت / امتیاز
        Task<OperationResult> EditAsync(EditComment command, string userIdentifier);               // ویرایش کامنت (توسط کاربر )
        Task<OperationResult> DeleteAsync(long id, string userIdentifier);                         // حذف کامل (درصورت نیاز)
        Task<OperationResult> Confirm(long id);                        // تأیید توسط ادمین
        Task<OperationResult> Cancel(long id);                         // رد توسط ادمین

        // --- تعامل ادمین ---
        Task<OperationResult> Reply(long id, string reply);             // پاسخ ادمین به کاربر

        // --- تعامل کاربران ---
        Task<OperationResult> LikeAsync(long id, string userIdentifier);     // لایک (کاربر)
        Task<OperationResult> DislikeAsync(long id, string userIdentifier);  // دیسلایک (کاربر)

        // --- Query ---
        Task<CommentViewModel> GetDetailsAsync(long id);
        Task<List<CommentViewModel>> SearchAsync(CommentSearchModel searchModel);
    }
}
