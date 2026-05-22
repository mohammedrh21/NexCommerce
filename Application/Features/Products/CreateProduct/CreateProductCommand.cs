using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Products;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;
using NexCommerce.Domain.Enums;

namespace NexCommerce.Application.Features.Products.CreateProduct;

public record CreateProductCommand(
    Guid VendorId,
    CreateProductRequest Request) : IRequest<ProductDetailDto>;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IVendorRepository vendorRepository,
    ICategoryRepository categoryRepository,
    ILanguageRepository languageRepository,
    IUnitOfWork unitOfWork,
    INotificationService notificationService,
    ILogger<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, ProductDetailDto>
{
    public async Task<ProductDetailDto> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        // Validate vendor exists and is approved
        var vendor = await vendorRepository.FindByIdAsync(command.VendorId, cancellationToken)
            ?? throw new NotFoundException("Vendor profile not found.");

        if (!vendor.IsApproved)
            throw new ForbiddenException("Vendor account is not yet approved.");

        // Validate category exists
        var categoryExists = await categoryRepository.ExistsAsync(req.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new NotFoundException($"Category {req.CategoryId} not found.");

        // Resolve language IDs
        var langCodes = req.Translations.Select(t => t.LanguageCode).Distinct().ToList();
        var languages = await languageRepository.GetByCodesAsync(langCodes, cancellationToken);

        var product = new Product
        {
            Id         = Guid.NewGuid(),
            Price      = req.Price,
            VendorId   = command.VendorId,
            CategoryId = req.CategoryId,
            CreatedAt  = DateTime.UtcNow
        };

        product.Translations = req.Translations.Select(t => new ProductTranslation
        {
            Name        = t.Name,
            Description = t.Description,
            LanguageId  = languages.First(l => l.Code == t.LanguageCode).Id,
            ProductId   = product.Id
        }).ToList();

        product.Variants = req.Variants.Select(v => new ProductVariant
        {
            Size               = v.Size,
            Color              = v.Color,
            SKU                = v.SKU,
            StockQuantity      = v.StockQuantity,
            LowStockThreshold  = v.LowStockThreshold,
            PriceAdjustment    = v.PriceAdjustment,
            ProductId          = product.Id
        }).ToList();

        productRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify admin of new product
        await notificationService.SendAsync(
            Guid.Empty,
            NotificationType.NewOffer,
            $"New product added: {req.Translations.First(t => t.LanguageCode == "en").Name}",
            cancellationToken);

        logger.LogInformation("Product {ProductId} created by vendor {VendorId}", product.Id, command.VendorId);

        // Reload with relations for response
        var loaded = await productRepository.FindWithDetailsAsync(product.Id, cancellationToken);

        return MapToDetail(loaded!, "en");
    }

    private static ProductDetailDto MapToDetail(Product p, string lang) => new(
        Id:           p.Id,
        Price:        p.Price,
        Rating:       p.Rating,
        ReviewCount:  p.Reviews.Count,
        CreatedAt:    p.CreatedAt,
        VendorId:     p.VendorId,
        VendorName:   p.Vendor.Translations.FirstOrDefault(t => t.Language.Code == lang)?.StoreName
                      ?? p.Vendor.Translations.FirstOrDefault()?.StoreName ?? string.Empty,
        CategoryId:   p.CategoryId,
        CategoryName: p.Category.Translations.FirstOrDefault(t => t.Language.Code == lang)?.Name
                      ?? p.Category.Translations.FirstOrDefault()?.Name ?? string.Empty,
        Translations: p.Translations.Select(t => new ProductTranslationDto(t.Language.Code, t.Name, t.Description)),
        Variants:     p.Variants.Select(v => new ProductVariantDto(v.Id, v.Size, v.Color, v.SKU,
                          v.StockQuantity, v.LowStockThreshold, v.PriceAdjustment, v.StockQuantity <= v.LowStockThreshold)),
        Images:       p.Images.Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.PublicId, i.IsMain)));
}
