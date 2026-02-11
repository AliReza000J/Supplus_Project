using _0_Framework.Application;
using ShopManagement.Application.Contracts.Slide;
using ShopManagement.Domain.ProductAgg;
using ShopManagement.Domain.SlideAgg;
using System.IO;

namespace ShopManagement.Application
{
    public class SlideApplication : ISlideApplication
    {
        private readonly IFileUploader _fileUploader;
        private readonly ISlideRepository _slideRepository;

        public SlideApplication(ISlideRepository slideRepository, IFileUploader fileUploader)
        {
            _fileUploader = fileUploader;
            _slideRepository = slideRepository;
        }

        public async Task<OperationResult> Create(CreateSlide command, CancellationToken cancellation)
        {
            var operation = new OperationResult();

            var pictureName = await _fileUploader.UploadAsync(command.Picture, "Slides", cancellation);

            var slide = new Slide(pictureName, command.PictureAlt, command.PictureTitle,
                command.Heading, command.Title, command.Text, command.Link, command.BtnText);

            _slideRepository.Create(slide);
            await _slideRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditSlide command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var slide = await _slideRepository.GetAsync(command.Id, cancellation);
            if (slide == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);
            var pictureName = "";

            if (!(command.Picture == null && string.IsNullOrEmpty(command.Picture?.FileName)))
            {
                _fileUploader.DeleteFile(slide.Picture);
                pictureName = await _fileUploader.UploadAsync(command.Picture, "Slides", cancellation);
            }

            slide.Edit(pictureName, command.PictureAlt, command.PictureTitle,
                command.Heading, command.Title, command.Text, command.Link, command.BtnText);
            await _slideRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<EditSlide?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _slideRepository.GetDetails(id, cancellation);
        }

        public async Task<List<SlideViewModel>> GetList(CancellationToken cancellation)
        {
            return await _slideRepository.GetList(cancellation);
        }

        public async Task<OperationResult> Remove(long id, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var slide = await _slideRepository.GetAsync(id, cancellation);
            if (slide == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            slide.Remove();
            await _slideRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<OperationResult> Restore(long id, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var slide = await _slideRepository.GetAsync(id, cancellation);
            if (slide == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            slide.Restore();
            await _slideRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }
    }
}
