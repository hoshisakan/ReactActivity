using Persistence;
using Application.Core;
using Application.Interfaces;

using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Application.Activities
{
    public class List
    {
        public class Query : IRequest<Result<PagedList<ActivityDto>>>
        {
            public ActivityParams Params { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<PagedList<ActivityDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<List> _logger;


            public Handler(DataContext context, IMapper mapper,
                IUserAccessor userAccessor, ILogger<List> logger)
            {
                _context = context;
                _mapper = mapper;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<PagedList<ActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                IQueryable<ActivityDto>? query = _context.Activities
                                .Where(d => d.Date >= request.Params.StartDate)
                                .OrderBy(d => d.Date)
                                .ProjectTo<ActivityDto>(
                                    _mapper.ConfigurationProvider,
                                    new {currentUsername = _userAccessor.GetUsername()})
                                .AsQueryable();

                if (request.Params.IsGoing && !request.Params.IsHost)
                {
                    query = query.Where(
                        x => x.Attendees.Any(
                            a => a.Username == _userAccessor.GetUsername()
                        )
                    );
                }

                if (request.Params.IsHost && !request.Params.IsGoing)
                {
                    query = query.Where(
                        x => x.HostUsername == _userAccessor.GetUsername()
                    );
                }

                return Result<PagedList<ActivityDto>>.Success(
                    await PagedList<ActivityDto>.CreateAsync(
                        query, request.Params.PageNumber,
                        request.Params.PageSize
                    )
                );
            }
        }
    }
}