using Domain;
using Application.Core;
using Application.Interfaces;
using Persistence;

using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Photos
{
    public class Add
    {
        public class Command : IRequest<Result<Photo>>
        {
            public IFormFile File { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Photo>>
        {
            private readonly DataContext _context;
            private readonly IPhotoAccessor _photoAccessor;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<Add> _logger;

            public Handler(DataContext context, IPhotoAccessor photoAccessor,
                    IUserAccessor userAccessor, ILogger<Add> logger)
            {
                _context = context;
                _photoAccessor = photoAccessor;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<Photo>> Handle(Command request, CancellationToken cancellationToken)
            {
                AppUser? user = await _context.Users.Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());

                if (user == null)
                {
                    return null;
                }

                if (request.File == null)
                {
                    return Result<Photo>.Failure("No file");
                }

                PhotoUploadResult uploadResult = await _photoAccessor.AddPhoto(request.File);

                _logger.LogInformation(uploadResult.Url);
                _logger.LogInformation(uploadResult.PublicId);

                Photo photo = new Photo
                {
                    Url = uploadResult.Url,
                    Id = uploadResult.PublicId
                };

                if (!user.Photos.Any(x => x.IsMain))
                {
                    photo.IsMain = true;
                }

                user.Photos.Add(photo);

                bool result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    return Result<Photo>.Success(photo);
                }

                return Result<Photo>.Failure("Problem adding photo");
            }
        }
    }
}