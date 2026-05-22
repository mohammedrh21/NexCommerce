using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Products;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Features.Products.UpdateProduct;

public record UpdateProductCommand(
    Guid ProductId,
    Guid RequestingUserId,
    bool IsAdmin,
    UpdateProductRequest Request) : IRequest<ProductDetailDto>;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ILanguageRepository languageRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProductCommandHandler> logger)
    : IRequestHandler<UpdateProductCommand, ProductDetailDto>
{
    public async Task<ProductDetailDto> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.FindWithDetailsAsync(command.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product {command.ProductId} not found.");

        if (!command.IsAdmin && product.Vendor.UserId != command.RequestingUserId)
            throw new ForbiddenException("You do not have permission to update this product.");

        var req = command.Request;

        if (req.Price.HasValue)    product.Price      = req.Price.Value;
        if (req.CategoryId.HasValue) product.CategoryId = req.CategoryId.Value;

        if (req.Translations is not null)
        {
            var langCodes = req.Translations.Select(t => t.LanguageCode).ToList();
            var languages = await languageRepository.GetByCodesAsync(langCodes, cancellationToken);

            foreach (var translation in req.Translations)
            {
                var existing = product.Translations
                    .FirstOrDefault(t => t.Language.Code == translation.LanguageCode);

                if (existing is not null)
                {
                    existing.Name        = translation.Name;
                    existing.Description = translation.Description;
                }
                else
                {
                    product.Translations.Add(new ProductTranslation
                    {
                        Name        = translation.Name,
                        Description = translation.Description,
                        LanguageId  = languages.First(l => l.Code == translation.LanguageCode).Id,
                        ProductId   = product.Id
                    });
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Product {ProductId} updated by user {UserId}", command.ProductId, command.RequestingUserId);

        return new ProductDetailDto(
            product.Id, product.Price, product.Rating, product.Reviews.Count,
            product.CreatedAt, product.VendorId,
            product.Vendor.Translations.FirstOrDefault()?.StoreName ?? string.Empty,
            product.CategoryId,
            product.Category.Translations.FirstOrDefault()?.Name ?? string.Empty,
            product.Translations.Select(t => new ProductTranslationDto(t.Language.Code, t.Name, t.Description)),
            product.Variants.Select(v => new ProductVariantDto(v.Id, v.Size, v.Color, v.SKU,
                v.StockQuantity, v.LowStockThreshold, v.PriceAdjustment, v.StockQuantity <= v.LowStockThreshold)),
            product.Images.Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.PublicId, i.IsMain)));
    }
}
