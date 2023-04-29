using Persistence;
using Application.Core;

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application.RefreshTokens
{
    public class List
    {
        public class Query : IRequest<Result<List<RefreshTokenDto>>>
        {
            public string? AppUserId { get; set; }
            public string? RefreshToken { get; set; }
            public string Predicate { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<RefreshTokenDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<List> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<List> logger)
            {
                _context = context;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<List<RefreshTokenDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                IQueryable<RefreshTokenDto>? query = _context.RefreshTokens
                    .Where(
                        x => x.IsRevoked == false
                        // && x.IsUsed == true
                    )
                    .ProjectTo<RefreshTokenDto>(_mapper.ConfigurationProvider)
                    .AsQueryable();

                _logger.LogInformation($"request.RefreshToken: {request.RefreshToken}");
                _logger.LogInformation($"request.Predicate: {request.Predicate}");

                query = request.Predicate switch
                {
                    "token" => query.Where(a => a.Token == request.RefreshToken),
                    _ => query.Where(a => a.AppUserId == request.AppUserId)
                };

                List<RefreshTokenDto>? refreshTokenDtoList = await query.ToListAsync();

                if (refreshTokenDtoList == null)
                {
                    return Result<List<RefreshTokenDto>>.Failure("Failed to get refresh token list.");
                }
                return Result<List<RefreshTokenDto>>.Success(refreshTokenDtoList);
            }
        }
    }
}