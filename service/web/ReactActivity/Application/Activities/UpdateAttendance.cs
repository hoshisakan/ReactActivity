using Application.Core;
using Persistence;
using Application.Interface;
using Domain;

using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Activities
{
    public class UpdateAttendance
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<UpdateAttendance> _logger;

            public Handler(DataContext context, IUserAccessor userAccessor, ILogger<UpdateAttendance> logger)
            {
                _context = context;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                Activity? activity = await _context.Activities
                        .Include(a => a.Attendees)
                        .ThenInclude(u => u.AppUser)
                        .SingleOrDefaultAsync(x => x.Id == request.Id);

                if (activity == null)
                {
                    return null;
                }

                _logger.LogInformation("Current authentication user is: {Username}", _userAccessor.GetUsername());

                AppUser user = await _context.Users.FirstOrDefaultAsync(x =>
                    x.UserName == _userAccessor.GetUsername());

                if (user == null)
                {
                    return null;
                }

                string hostUsername = activity.Attendees.FirstOrDefault(x => x.IsHost)?.AppUser?.UserName;

                _logger.LogInformation("Host username is: {HostUsername}", hostUsername);

                ActivityAttendee? attendance = activity.Attendees.FirstOrDefault(x => x.AppUser.UserName == user.UserName);
            
                //TODO: If the user is the host, cancel the activity
                if (attendance != null && hostUsername == user.UserName)
                {
                    activity.IsCancelled = !activity.IsCancelled;
                }

                //TODO: If the user is not the host, remove the attendance
                if (attendance != null && hostUsername != user.UserName)
                {
                    activity.Attendees.Remove(attendance);
                }
                //TODO: If the user is not attending, add the attendance
                else if (attendance == null)
                {
                    _logger.LogInformation("Adding attendance for user {Username} to activity {ActivityId}", user.UserName, activity.Id);
                    attendance = new ActivityAttendee
                    {
                        AppUser = user,
                        Activity = activity,
                        IsHost = false,
                        CreatedAt = DateTime.Now
                    };
                    activity.Attendees.Add(attendance);
                }

                bool result = await _context.SaveChangesAsync() > 0;

                return result ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Failed to update attendance");
            }
        }
    }
}