using Domain;

using FluentValidation;

namespace Application.Activities
{
    public class ActivityValidator : AbstractValidator<Activity>
    {
        public ActivityValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title cannot be empty");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description cannot be empty");
            RuleFor(x => x.Category).NotEmpty().WithMessage("Category cannot be empty");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date cannot be empty");
            RuleFor(x => x.City).NotEmpty().WithMessage("City cannot be empty");
            RuleFor(x => x.Venue).NotEmpty().WithMessage("Venue cannot be empty");
        }
    }
}