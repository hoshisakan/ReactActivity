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
    public class UpdateUsedState
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string AppUserId { get; set; }
            public string Token { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<UpdateUsedState> _logger;

            public Handler(DataContext context, IUserAccessor userAccessor, ILogger<UpdateUsedState> logger)
            {
                _context = context;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                RefreshToken? refreshTokenFromDb = await _context.RefreshTokens.Where(
                    rt => rt.AppUserId == request.AppUserId && rt.Token == request.Token
                ).FirstOrDefaultAsync();

                if (refreshTokenFromDb != null)
                {
                    refreshTokenFromDb.IsUsed = true;

                    _context.Entry(refreshTokenFromDb).State = EntityState.Modified;

                    bool result = await _context.SaveChangesAsync() > 0;
                    if (result)
                    {
                        return Result<Unit>.Success(Unit.Value);
                    }
                }
                return Result<Unit>.Failure("Failed to update the user refresh token used state.");
            }
        }
    }
}