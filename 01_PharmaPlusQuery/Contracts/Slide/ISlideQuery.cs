namespace _01_PharmaPlusQuery.Contracts.Slide
{
    public interface ISlideQuery
    {
        Task<List<SlideQueryModel>> GetSlides();
    }
}
