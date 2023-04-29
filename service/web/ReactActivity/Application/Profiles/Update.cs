using Domain;
using Persistence;
using Application.Core;
using Application.Interfaces;

using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles
{
    public class Update
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string DisplayName { get; set; }
            public string Bio { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.DisplayName).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<Update> _logger;

            public Handler(DataContext context, IUserAccessor userAccessor, ILogger<Update> logger)
            {
                _context = context;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                AppUser? user = await _context.Users.FirstOrDefaultAsync(
                    x => x.UserName == _userAccessor.GetUsername()
                );
                
                if (user == null)
                {
                    return null;
                }

                bool IsExistsDisplayName = await _context.Users.AnyAsync(
                    x => x.UserName != user.UserName && x.DisplayName == request.DisplayName
                );

                if (IsExistsDisplayName)
                {
                    return Result<Unit>.Failure("Display name is already taken");
                }

                user.DisplayName = request.DisplayName ?? user.DisplayName;
                user.Bio = request.Bio ?? user.Bio;

                _logger.LogInformation("Updating profile for user {Username}", user.UserName);
                _logger.LogInformation("DisplayName: {DisplayName}", user.DisplayName);
                _logger.LogInformation("Bio: {Bio}", user.Bio);

                //TODO: If current user's profile updated data with the same data, then will talk entity data has been updated.
                _context.Entry(user).State = EntityState.Modified;

                bool result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    return Result<Unit>.Success(Unit.Value);
                }
                return Result<Unit>.Failure("Failed to update the profile");
            }
        }
    }
}