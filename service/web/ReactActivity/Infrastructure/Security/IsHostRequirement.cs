using Persistence;
using Domain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Security
{
    public class IsHostRequirement : IAuthorizationRequirement
    {
    }

    public class IsHostRequirementHandler : AuthorizationHandler<IsHostRequirement>
    {
        private readonly DataContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<IsHostRequirementHandler> _logger;

        public IsHostRequirementHandler(DataContext context,
            IHttpContextAccessor httpContextAccessor, ILogger<IsHostRequirementHandler> logger)
        {
            _dbContext = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsHostRequirement requirement)
        {
            string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Task.CompletedTask;
            }

            _logger.LogInformation($"User Id: {userId}");

            Guid activityId = Guid.Parse(
                _httpContextAccessor.HttpContext?.Request.RouteValues
                .SingleOrDefault(x => x.Key == "id").Value?.ToString()
            );

            _logger.LogInformation($"User {userId} is trying to access activity {activityId}.");

            ActivityAttendee attendee = _dbContext.ActivityAttendees
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.AppUserId == userId && x.ActivityId == activityId).Result;
            
            if (attendee == null)
            {
                return Task.CompletedTask;
            }

            _logger.LogInformation($"User IsHost: {attendee.IsHost}");

            if (attendee.IsHost)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}