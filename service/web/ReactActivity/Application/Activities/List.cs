using Domain;
using Persistence;
using Application.Core;
using Application.Interfaces;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Application.Activities
{
    public class List
    {
        public class Query : IRequest<Result<List<ActivityDto>>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<List<ActivityDto>>>
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

            public async Task<Result<List<ActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                List<ActivityDto> activities = await _context.Activities
                                .ProjectTo<ActivityDto>(
                                    _mapper.ConfigurationProvider,
                                    new {currentUsername = _userAccessor.GetUsername()})
                                .ToListAsync(cancellationToken);
                return Result<List<ActivityDto>>.Success(activities);
            }
        }
    }
}