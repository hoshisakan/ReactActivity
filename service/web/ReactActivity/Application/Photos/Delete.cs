using Domain;
using Application.Core;
using Application.Interfaces;
using Persistence;

using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Photos
{
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
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

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                AppUser? user = await _context.Users.Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());

                if (user == null)
                {
                    return null;
                }

                Photo photo = user.Photos.FirstOrDefault(x => x.Id == request.Id);

                if (photo == null)
                {
                    return null;
                }

                if (photo.IsMain)
                {
                    return Result<Unit>.Failure("You cannot delete your main photo");
                }

                string deleteResult = await _photoAccessor.DeletePhoto(request.Id);

                if (deleteResult == null)
                {
                    return Result<Unit>.Failure("Problem deleting photo from Cloudinary");
                }

                user.Photos.Remove(photo);

                bool success = await _context.SaveChangesAsync() > 0;

                if (success)
                {
                    return Result<Unit>.Success(Unit.Value);
                }
                return Result<Unit>.Failure("Problem deleting photo from API");
            }
        }
    }
}