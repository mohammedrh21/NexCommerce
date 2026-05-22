using FluentValidation;
using NexCommerce.Application.DTOs.Orders;
using NexCommerce.Application.DTOs.Reviews;
using NexCommerce.Application.DTOs.Cart;
using NexCommerce.Application.DTOs.Coupons;
using NexCommerce.Application.DTOs.Categories;

namespace NexCommerce.Application.Validators;

public class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductVariantId)
                .NotEmpty().WithMessage("Product variant ID is required.");
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.")
                .LessThanOrEqualTo(100).WithMessage("Cannot order more than 100 units per item.");
        });

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required.")
            .MaximumLength(500);
    }
}

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Invalid order status.");
        When(x => x.Note is not null, () =>
            RuleFor(x => x.Note!).MaximumLength(500));
    }
}

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(1000);
    }
}

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(x => x.ProductVariantId).NotEmpty();
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(50).WithMessage("Cannot add more than 50 units at once.");
    }
}

public class CreateCouponRequestValidator : AbstractValidator<CreateCouponRequest>
{
    public CreateCouponRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9_-]+$").WithMessage("Coupon code must be uppercase alphanumeric.");
        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than zero.");
        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future.");
        RuleFor(x => x.UsageLimit)
            .GreaterThan(0).WithMessage("Usage limit must be at least 1.");
        RuleFor(x => x.Translations)
            .NotEmpty().WithMessage("At least one translation is required.");
    }
}

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Translations)
            .NotEmpty().WithMessage("At least one translation is required.")
            .Must(t => t.Any(tr => tr.LanguageCode == "en"))
            .WithMessage("An English translation is required.");

        RuleForEach(x => x.Translations).ChildRules(t =>
        {
            t.RuleFor(x => x.LanguageCode).NotEmpty().MaximumLength(10);
            t.RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        });
    }
}
