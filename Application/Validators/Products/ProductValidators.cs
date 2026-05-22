using FluentValidation;
using NexCommerce.Application.DTOs.Products;

namespace NexCommerce.Application.Validators.Products;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.")
            .LessThan(1_000_000).WithMessage("Price must be less than 1,000,000.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Translations)
            .NotEmpty().WithMessage("At least one translation is required.")
            .Must(t => t.Any(tr => tr.LanguageCode == "en"))
            .WithMessage("An English translation is required.");

        RuleForEach(x => x.Translations).SetValidator(new ProductTranslationRequestValidator());

        RuleFor(x => x.Variants)
            .NotEmpty().WithMessage("At least one variant is required.");

        RuleForEach(x => x.Variants).SetValidator(new ProductVariantRequestValidator());
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        When(x => x.Price.HasValue, () =>
            RuleFor(x => x.Price!.Value)
                .GreaterThan(0).WithMessage("Price must be greater than zero."));
    }
}

public class ProductTranslationRequestValidator : AbstractValidator<CreateProductTranslationRequest>
{
    public ProductTranslationRequestValidator()
    {
        RuleFor(x => x.LanguageCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}

public class ProductVariantRequestValidator : AbstractValidator<CreateProductVariantRequest>
{
    public ProductVariantRequestValidator()
    {
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Low stock threshold cannot be negative.");
        When(x => x.SKU is not null, () =>
            RuleFor(x => x.SKU!).MaximumLength(50));
    }
}
