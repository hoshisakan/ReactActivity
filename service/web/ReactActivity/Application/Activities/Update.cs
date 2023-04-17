using Domain;
using Persistence;
using Application.Core;

using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using FluentValidation;


namespace Application.Activities
{
    public class Update
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Activity Activity { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Activity).SetValidator(new ActivityValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
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

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                Activity? activity = await _context.Activities.FindAsync(request.Activity.Id);

                if (activity == null)
                {
                    return null;
                }

                request.Activity.ModifiedAt = DateTime.Now;
                request.Activity.CreatedAt = activity?.CreatedAt ?? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                _mapper.Map(request.Activity, activity);

                bool result = await _context.SaveChangesAsync() > 0;

                if (!result)
                {
                    return Result<Unit>.Failure("Failed to update the activity");
                }

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}