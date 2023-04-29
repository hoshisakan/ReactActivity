using Application.Core;
using Persistence;

using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore; //TODO: must import, otherwise can't be to use async method.

namespace Application.RefreshTokens
{
    public class Retrieve
    {
        public class Query : IRequest<Result<RefreshTokenDto>>
        {
            public string? AppUserId { get; set; }
            public string? RefreshToken { get; set; }
            public string Predicate { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<RefreshTokenDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<Retrieve> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<Retrieve> logger)
            {
                _context = context;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<RefreshTokenDto>> Handle(Query request, CancellationToken cancellationToken)
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

                RefreshTokenDto? refreshTokenDto = await query.FirstOrDefaultAsync();

                if (refreshTokenDto == null)
                {
                    return Result<RefreshTokenDto>.Failure("Failed to find refresh token");
                }
                return Result<RefreshTokenDto>.Success(refreshTokenDto);
            }
        }
    }
}