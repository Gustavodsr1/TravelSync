using FluentValidation;

namespace TravelSync.Trip.API.Features.CreateTrip;

public sealed class CreateTripValidator : AbstractValidator<CreateTripCommand>
{
    public CreateTripValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Trip name is required.")
            .MaximumLength(200).WithMessage("Trip name cannot exceed 200 characters.");

        RuleFor(c => c.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(c => c.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThanOrEqualTo(c => c.StartDate)
            .WithMessage("End date must be on or after start date.");

        RuleForEach(c => c.Destinations).ChildRules(d =>
        {
            d.RuleFor(x => x.Country).NotEmpty().WithMessage("Destination country is required.");
            d.RuleFor(x => x.City).NotEmpty().WithMessage("Destination city is required.");
        });
    }
}
