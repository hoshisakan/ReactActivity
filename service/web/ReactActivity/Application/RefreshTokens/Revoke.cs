using Domain;
using Persistence;
using Application.Core;
using Application.Interfaces;

using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Microsoft.EntityFrameworkCore;


namespace Application.RefreshTokens
{
    public class Revoke
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string AppUserId { get; set; }
            public string Token { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AppUserId).NotEmpty();
                RuleFor(x => x.Token).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Revoke> _logger;

            public Handler(DataContext context, IUserAccessor userAccessor, ILogger<Revoke> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                List<RefreshToken>? refreshTokenFromDb = await _context.RefreshTokens
                    .Where(u => u.AppUser.Id == request.AppUserId && u.Token == request.Token)
                    .Include(rt => rt.AppUser)
                    .ToListAsync();
                
                _logger.LogInformation(
                    $"request.AppUserId: {request.AppUserId}, request.Token: {request.Token}, revoke count: {refreshTokenFromDb.Count}");

                if (refreshTokenFromDb.Count == 0)
                {
                    return Result<Unit>.Failure("Failed to revoke the refresh token, because the token invalid.");
                }

                refreshTokenFromDb.Select(rt => { rt.Revoked = DateTime.UtcNow; return rt; }).ToList();

                _logger.LogInformation(
                    $"request.AppUserId: {request.AppUserId}, request.Token: {request.Token}, revoke count: {refreshTokenFromDb.Count}"
                );

                bool result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    return Result<Unit>.Success(Unit.Value);
                }
                return Result<Unit>.Failure("Problem the revoke.");
            }
        }
    }
}