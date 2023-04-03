using Domain;
using Persistence;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Application.Activities
{
    public class Create
    {
        public class Command : IRequest
        {
            public Activity Activity { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;
            private readonly ILogger<Create> _logger;

            public Handler(DataContext context, ILogger<Create> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                request.Activity.CreatedAt = DateTime.Now;
                _context.Activities.Add(request.Activity);

                await _context.SaveChangesAsync();

                return Unit.Value;
            }
        }
    }
}