using Application.Core;
using Application.Interfaces;
using Domain;
using Persistence;

using MediatR;
using FluentValidation;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Comments
{
    public class Create
    {
        public class Command : IRequest<Result<CommentDto>>
        {
            public Guid ActivityId { get; set; }
            public string Body { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Body).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Result<CommentDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<Create> _logger;

            public Handler(DataContext context, IMapper mapper,
                IUserAccessor userAccessor, ILogger<Create> logger)
            {
                _context = context;
                _mapper = mapper;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                Activity? activity = await _context.Activities.FindAsync(request.ActivityId);

                if (activity == null)
                {
                    return null;
                }

                AppUser? user = await _context.Users
                    .Include(p => p.Photos)
                    .SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());
                
                Comment comment = new Comment
                {
                    Author = user,
                    Activity = activity,
                    Body = request.Body,
                    CreatedAt = DateTime.Now
                };

                activity.Comments.Add(comment);

                bool success = await _context.SaveChangesAsync() > 0;

                if (success)
                {
                    return Result<CommentDto>.Success(_mapper.Map<CommentDto>(comment));
                }
                return Result<CommentDto>.Failure("Problem adding comment");
            }
        }
    }
}