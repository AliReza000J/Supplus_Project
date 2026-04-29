using _0_Framework.Application;
using _0_Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Application.Contracts.Slide;
using ShopManagement.Domain.SlideAgg;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopManagement.Infrastructure.EFCore.Repository
{
    public class SlideRepository : BaseRepository<long, Slide>, ISlideRepository
    {
        private readonly ShopContext _context;

        public SlideRepository(ShopContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EditSlide?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _context.Slides.Select(x => new EditSlide
            {
                Id = x.Id,
                BtnText = x.BtnText,
                Heading = x.Heading,
                PictureAlt = x.PictureAlt,
                PictureTitle = x.PictureTitle,
                Text = x.Text,
                Link = x.Link,
                Title = x.Title
            }).FirstOrDefaultAsync(x => x.Id == id, cancellation);
        }

        public async Task<List<SlideViewModel>> GetList(CancellationToken cancellation)
        {
            return await _context.Slides.Select(x => new SlideViewModel
            {
                Id = x.Id,
                Heading = x.Heading,
                Picture = x.Picture,
                Title = x.Title,
                IsRemoved = x.IsRemoved,
                CreationDate = x.CreationDate.ToFarsi()
            }).OrderByDescending(x => x.Id).ToListAsync(cancellation);
        }
    }
}
