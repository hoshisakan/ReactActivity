using Domain;
using Persistence;
using Application.Core;

using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;


namespace Application.Activities
{
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<Delete> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<Delete> logger)
            {
                _context = context;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                Activity? activity = await _context.Activities.FindAsync(request.Id);

                if (activity == null)
                {
                    return null;
                }

                _context.Activities.Remove(activity);

                bool result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                {
                    return Result<Unit>.Failure("Failed to delete the activity");
                }
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}