using Application.Core;
using Application.Interfaces;
using Persistence;
using Domain;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Followers
{
    public class FollowToggle
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string TargetUsername { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<FollowToggle> _logger;

            public Handler(DataContext context, IUserAccessor userAccessor,
                ILogger<FollowToggle> logger)
            {
                _context = context;
                _userAccessor = userAccessor;
                _logger = logger;
            }
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                //TODO Check current user whether exists or not in users table, if not exists the user not following anyone.
                AppUser? observer = await _context.Users.FirstOrDefaultAsync(
                    x => x.UserName == _userAccessor.GetUsername());

                //TODO: Check the following target whether exists or not in users table.
                AppUser? target = await _context.Users.FirstOrDefaultAsync(
                    x => x.UserName == request.TargetUsername);

                if (target == null)
                {
                    return null;
                }

                UserFollowing? following = await _context.UserFollowings.FindAsync(observer.Id, target.Id);

                if (following == null)
                {
                    following = new UserFollowing
                    {
                        Observer = observer,
                        Target = target
                    };
                    _context.UserFollowings.Add(following);
                }
                else
                {
                    _context.UserFollowings.Remove(following);
                }

                bool success = await _context.SaveChangesAsync() > 0;

                if (success)
                {
                    return Result<Unit>.Success(Unit.Value);
                }
                return Result<Unit>.Failure("Failed to update following");
            }
        }
    }
}