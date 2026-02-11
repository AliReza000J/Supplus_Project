using _0_Framework.Application;

namespace CommentManagement.Application.Contracts.Comment
{
    public interface ICommentApplication
    {
        // --- main CRUD ---
        Task<OperationResult> AddAsync(AddComment command, long userId, string username, CancellationToken cancellation);                 // ثبت کامنت / امتیاز
        Task<OperationResult> EditAsync(EditComment command, long userId, CancellationToken cancellation);               // ویرایش کامنت (توسط کاربر )
        Task<OperationResult> DeleteAsync(long id, long userId, CancellationToken cancellation);                         // حذف کامل (درصورت نیاز)
        Task<OperationResult> Confirm(long id, CancellationToken cancellation);                        // تأیید توسط ادمین
        Task<OperationResult> Cancel(long id, CancellationToken cancellation);                         // رد توسط ادمین

        // --- تعامل ادمین ---
        Task<OperationResult> Reply(long id, string reply, CancellationToken cancellation);             // پاسخ ادمین به کاربر

        // --- تعامل کاربران ---
        Task<OperationResult> LikeAsync(long id, long userId, CancellationToken cancellation);     // لایک (کاربر)
        Task<OperationResult> DislikeAsync(long id, long userId, CancellationToken cancellation);  // دیسلایک (کاربر)

        // --- Query ---
        Task<CommentViewModel> GetDetailsAsync(long id, CancellationToken cancellation);
        Task<List<CommentViewModel>> SearchAsync(CommentSearchModel searchModel, CancellationToken cancellation);
    }
}
