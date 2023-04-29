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
            public string? AppUserId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.AppUserId).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<Revoke> _logger;

            public Handler(DataContext context, IUserAccessor userAccessor, ILogger<Revoke> logger)
            {
                _context = context;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                List<RefreshToken>? refreshTokenFromDb = await _context.RefreshTokens.ToListAsync();
                refreshTokenFromDb.Where(
                    u => u.AppUserId == request.AppUserId
                ).Select(rt => { rt.IsRevoked = true; return rt; }).ToList();

                bool result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    return Result<Unit>.Success(Unit.Value);
                }
                return Result<Unit>.Failure("Failed to revoke the refresh token.");
            }
        }
    }
}