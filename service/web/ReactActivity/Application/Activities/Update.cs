using Domain;
using Persistence;

using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;


namespace Application.Activities
{
    public class Update
    {
        public class Command : IRequest
        {
            public Activity Activity { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<Update> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<Update> logger)
            {
                _context = context;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                Activity? activity = await _context.Activities.FindAsync(request.Activity.Id);

                request.Activity.ModifiedAt = DateTime.Now;
                request.Activity.CreatedAt = activity?.CreatedAt ?? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                _mapper.Map(request.Activity, activity);

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}