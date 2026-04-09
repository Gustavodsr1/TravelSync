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
            d.RuleFor(x => x.Latitude)
                .InclusiveBetween(-90m, 90m).WithMessage("Latitude must be between -90 and 90.")
                .When(x => x.Latitude.HasValue);
            d.RuleFor(x => x.Longitude)
                .InclusiveBetween(-180m, 180m).WithMessage("Longitude must be between -180 and 180.")
                .When(x => x.Longitude.HasValue);
        });
    }
}
