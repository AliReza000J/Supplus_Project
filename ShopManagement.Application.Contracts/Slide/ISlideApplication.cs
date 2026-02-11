using _0_Framework.Application;
using System.Collections.Generic;

namespace ShopManagement.Application.Contracts.Slide
{
    public interface ISlideApplication
    {
        Task<OperationResult> Create(CreateSlide command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditSlide command, CancellationToken cancellation);
        Task<OperationResult> Remove(long id, CancellationToken cancellation);
        Task<OperationResult> Restore(long id, CancellationToken cancellation);
        Task<EditSlide?> GetDetails(long id, CancellationToken cancellation);
        Task<List<SlideViewModel>> GetList(CancellationToken cancellation);
    }
}
