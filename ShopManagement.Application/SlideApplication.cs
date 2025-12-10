using _0_Framework.Application;
using ShopManagement.Application.Contracts.Slide;
using ShopManagement.Domain.SlideAgg;

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

        public async Task<OperationResult> Create(CreateSlide command)
        {
            var operation = new OperationResult();

            var pictureName = _fileUploader.Upload(command.Picture, "slides");

            var slide = new Slide(pictureName, command.PictureAlt, command.PictureTitle,
                command.Heading, command.Title, command.Text, command.Link, command.BtnText);

            _slideRepository.Create(slide);
            await _slideRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditSlide command)
        {
            var operation = new OperationResult();
            var slide = await _slideRepository.GetAsync(command.Id);
            if (slide == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            var pictureName = _fileUploader.Upload(command.Picture, "slides");

            slide.Edit(pictureName, command.PictureAlt, command.PictureTitle,
                command.Heading, command.Title, command.Text, command.Link, command.BtnText);
            await _slideRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<EditSlide?> GetDetails(long id)
        {
            return await _slideRepository.GetDetails(id);
        }

        public async Task<List<SlideViewModel>> GetList()
        {
            return await _slideRepository.GetList();
        }

        public async Task<OperationResult> Remove(long id)
        {
            var operation = new OperationResult();
            var slide = await _slideRepository.GetAsync(id);
            if (slide == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            slide.Remove();
            await _slideRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<OperationResult> Restore(long id)
        {
            var operation = new OperationResult();
            var slide = await _slideRepository.GetAsync(id);
            if (slide == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            slide.Restore();
            await _slideRepository.SaveChangesAsync();
            return operation.Succeeded();
        }
    }
}
