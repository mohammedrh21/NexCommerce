using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Categories;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Features.Categories.CreateCategory;

public record CreateCategoryCommand(CreateCategoryRequest Request) : IRequest<CategoryDto>;

public sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ILanguageRepository languageRepository,
    IUnitOfWork unitOfWork,
    ICategoryService categoryService,
    ILogger<CreateCategoryCommandHandler> logger)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (request.ParentId.HasValue)
        {
            var parentExists = await categoryRepository.ExistsAsync(request.ParentId.Value, cancellationToken);
            if (!parentExists)
                throw new NotFoundException($"Parent category {request.ParentId} not found.");
        }

        var langCodes = request.Translations.Select(t => t.LanguageCode).ToList();
        var languages = await languageRepository.GetByCodesAsync(langCodes, cancellationToken);

        var category = new Category { ParentId = request.ParentId };
        category.Translations = request.Translations.Select(t => new CategoryTranslation
        {
            Name       = t.Name,
            LanguageId = languages.First(l => l.Code == t.LanguageCode).Id,
            CategoryId = category.Id
        }).ToList();

        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Category {CategoryId} created", category.Id);
        
        // Read back via service logic to ensure consistent DTO formatting
        return await categoryService.GetByIdAsync(category.Id, "en", cancellationToken);
    }
}
