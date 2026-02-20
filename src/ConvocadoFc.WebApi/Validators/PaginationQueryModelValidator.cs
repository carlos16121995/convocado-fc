using ConvocadoFc.WebApi.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators;

public class PaginationQueryModelValidator : AbstractValidator<PaginationQueryModel>
{
    public PaginationQueryModelValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.OrderBy).MaximumLength(100);
    }
}
