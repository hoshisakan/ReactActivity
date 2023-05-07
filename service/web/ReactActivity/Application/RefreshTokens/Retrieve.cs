using Application.Core;
using Persistence;

using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;


namespace Application.RefreshTokens
{
    public class Retrieve
    {
        public class Query : IRequest<Result<RefreshTokenDto>>
        {
            public string? AppUserId { get; set; }
            public string? Token { get; set; }
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
                    .Where(r => r.Revoked == null && DateTime.UtcNow < r.Expires)
                    .Include(rt => rt.AppUser)
                    .OrderByDescending(rt => rt.CreatedAt)
                    .ProjectTo<RefreshTokenDto>(_mapper.ConfigurationProvider)
                    .AsQueryable();

                _logger.LogInformation($"request refresh token: {request.Token}");
                _logger.LogInformation($"request predicate: {request.Predicate}");

                query = request.Predicate switch
                {
                    "token" => query.Where(a => a.Token == request.Token),
                    "appUserId" => query.Where(a => a.AppUserId == request.AppUserId),
                    _ => query.Where(a => a.AppUserId == request.AppUserId && a.Token == request.Token)
                };

                RefreshTokenDto? refreshTokenDto = await query.FirstOrDefaultAsync();

                if (refreshTokenDto == null)
                {
                    return Result<RefreshTokenDto>.Failure("Failed to find refresh token");
                }
                _logger.LogInformation($"Revoked: {refreshTokenDto.Revoked}");
                return Result<RefreshTokenDto>.Success(refreshTokenDto);
            }
        }
    }
}