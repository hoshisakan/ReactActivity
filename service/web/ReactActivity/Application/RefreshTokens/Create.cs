using Domain;
using Application.Core;
using Application.Interfaces;
using Persistence;

using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.RefreshTokens
{
    public class Create
    {
        public class Command : IRequest<Result<Unit>>
        {
            public RefreshToken RefreshToken { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<Create> _logger;

            public Handler(DataContext context, IUserAccessor userAccessor, ILogger<Create> logger)
            {
                _context = context;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                request.RefreshToken.CreatedAt = DateTime.Now;

                _context.RefreshTokens.Add(request.RefreshToken);

                bool result = await _context.SaveChangesAsync() > 0;

                if (!result)
                {
                    return Result<Unit>.Failure("Failed to create refresh token");
                }
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}