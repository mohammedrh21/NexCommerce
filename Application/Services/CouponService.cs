using Microsoft.Extensions.Logging;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Coupons;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;
using NexCommerce.Domain.Enums;

namespace NexCommerce.Application.Services;

/// <summary>
/// Application-layer coupon management service.
/// Handles CRUD and validation logic, delegating persistence to <see cref="ICouponRepository"/>.
/// <para>
/// Note: <see cref="CouponTranslation"/> stores <c>LanguageId</c> (FK) and <c>Description</c> only.
/// The <see cref="CouponTranslationDto.Name"/> field is surfaced as the localised language display name.
/// </para>
/// </summary>
public sealed class CouponService(
    ICouponRepository coupons,
    ILanguageRepository languages,
    IUnitOfWork unitOfWork,
    ILogger<CouponService> logger) : ICouponService
{
    // ── Queries ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<PagedResult<CouponDto>> GetAllAsync(
        int page, int pageSize, string lang, CancellationToken ct = default)
    {
        var (items, total) = await coupons.GetPagedAsync(page, pageSize, ct);

        return new PagedResult<CouponDto>
        {
            Data       = items.Select(c => MapToDto(c, lang)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
        };
    }

    /// <inheritdoc/>
    public async Task<CouponDto> GetByIdAsync(Guid id, string lang, CancellationToken ct = default)
    {
        var coupon = await coupons.FindByIdAsync(id, ct)
            ?? throw new NotFoundException($"Coupon '{id}' was not found.");

        return MapToDto(coupon, lang);
    }

    // ── Mutations ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<CouponDto> CreateAsync(CreateCouponRequest request, CancellationToken ct = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        if (await coupons.CodeExistsAsync(code, ct))
            throw new ValidationException($"Coupon code '{code}' is already in use.");

        // Resolve language codes → Language entities
        var langCodes  = request.Translations.Select(t => t.LanguageCode).Distinct().ToList();
        var langLookup = (await languages.GetByCodesAsync(langCodes, ct))
                             .ToDictionary(l => l.Code, StringComparer.OrdinalIgnoreCase);

        var coupon = new Coupon
        {
            Id            = Guid.NewGuid(),
            Code          = code,
            DiscountType  = request.DiscountType,
            DiscountValue = request.DiscountValue,
            ExpiryDate    = request.ExpiryDate,
            UsageLimit    = request.UsageLimit,
            IsActive      = true,
            Translations  = request.Translations
                .Where(t => langLookup.ContainsKey(t.LanguageCode))
                .Select(t => new CouponTranslation
                {
                    Id          = Guid.NewGuid(),
                    LanguageId  = langLookup[t.LanguageCode].Id,
                    Description = t.Description,
                })
                .ToList(),
        };

        coupons.Add(coupon);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Coupon '{Code}' created (id={Id})", coupon.Code, coupon.Id);
        return MapToDto(coupon, "en");
    }

    /// <inheritdoc/>
    public async Task<CouponDto> UpdateAsync(Guid id, UpdateCouponRequest request, CancellationToken ct = default)
    {
        var coupon = await coupons.FindByIdAsync(id, ct)
            ?? throw new NotFoundException($"Coupon '{id}' was not found.");

        if (request.DiscountType.HasValue)  coupon.DiscountType  = request.DiscountType.Value;
        if (request.DiscountValue.HasValue) coupon.DiscountValue = request.DiscountValue.Value;
        if (request.ExpiryDate.HasValue)    coupon.ExpiryDate    = request.ExpiryDate.Value;
        if (request.UsageLimit.HasValue)    coupon.UsageLimit    = request.UsageLimit.Value;
        if (request.IsActive.HasValue)      coupon.IsActive      = request.IsActive.Value;

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Coupon '{Code}' updated", coupon.Code);
        return MapToDto(coupon, "en");
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var coupon = await coupons.FindByIdAsync(id, ct)
            ?? throw new NotFoundException($"Coupon '{id}' was not found.");

        coupons.Remove(coupon);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Coupon '{Code}' deleted (id={Id})", coupon.Code, id);
    }

    // ── Validation ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<CouponValidationResultDto> ValidateAsync(
        ValidateCouponRequest request, CancellationToken ct = default)
    {
        var code   = request.Code.Trim().ToUpperInvariant();
        var coupon = await coupons.FindByCodeAsync(code, ct);

        if (coupon is null)
            return Fail("Coupon code was not found.");

        if (!coupon.IsActive)
            return Fail("This coupon is no longer active.");

        if (coupon.ExpiryDate < DateTime.UtcNow)
            return Fail("This coupon has expired.");

        var used = await coupons.GetUsageCountAsync(coupon.Id, ct);
        if (used >= coupon.UsageLimit)
            return Fail("This coupon has reached its usage limit.");

        var discountAmount = coupon.DiscountType == DiscountType.Percentage
             ? Math.Round(request.OrderAmount * (coupon.DiscountValue / 100m), 2, MidpointRounding.AwayFromZero)
             : Math.Min(coupon.DiscountValue, request.OrderAmount);

        var finalAmount = Math.Max(0m, request.OrderAmount - discountAmount);

        return new CouponValidationResultDto(
            IsValid:        true,
            ErrorMessage:   null,
            CouponId:       coupon.Id,
            Code:           coupon.Code,
            DiscountType:   coupon.DiscountType,
            DiscountValue:  coupon.DiscountValue,
            DiscountAmount: discountAmount,
            FinalAmount:    finalAmount);

        static CouponValidationResultDto Fail(string error)
            => new(false, error, null, null, null, null, null, null);
    }

    // ── Mapper ─────────────────────────────────────────────────────────────────

    private static CouponDto MapToDto(Coupon coupon, string lang)
    {
        var usedCount = coupon.Usages?.Count ?? 0;

        // CouponTranslation only stores LanguageId + Description.
        // Name is surfaced as the Language display name; code from the Language nav property.
        var translations = coupon.Translations?
            .Select(t => new CouponTranslationDto(
                LanguageCode: t.Language?.Code        ?? string.Empty,
                Name:         t.Language?.Name        ?? string.Empty,
                Description:  t.Description))
            ?? [];

        return new CouponDto(
            Id:            coupon.Id,
            Code:          coupon.Code,
            DiscountType:  coupon.DiscountType,
            DiscountValue: coupon.DiscountValue,
            ExpiryDate:    coupon.ExpiryDate,
            UsageLimit:    coupon.UsageLimit,
            UsedCount:     usedCount,
            IsActive:      coupon.IsActive,
            IsExpired:     coupon.ExpiryDate < DateTime.UtcNow,
            Translations:  translations);
    }
}
