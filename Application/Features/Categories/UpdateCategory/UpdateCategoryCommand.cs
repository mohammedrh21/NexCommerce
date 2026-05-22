using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Categories;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Features.Categories.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, UpdateCategoryRequest Request) : IRequest<CategoryDto>;

public sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ILanguageRepository languageRepository,
    IUnitOfWork unitOfWork,
    ICategoryService categoryService,
    ILogger<UpdateCategoryCommandHandler> logger)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var id = command.Id;
        var request = command.Request;

        var category = await categoryRepository.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Category {id} not found.");

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == id)
                throw new ValidationException("Category cannot be its own parent.");
            category.ParentId = request.ParentId.Value;
        }

        if (request.Translations is not null)
        {
            var langCodes = request.Translations.Select(t => t.LanguageCode).ToList();
            var languages = await languageRepository.GetByCodesAsync(langCodes, cancellationToken);

            foreach (var t in request.Translations)
            {
                var existing = category.Translations.FirstOrDefault(x => x.Language.Code == t.LanguageCode);
                if (existing is not null)
                {
                    existing.Name = t.Name;
                }
                else
                {
                    category.Translations.Add(new CategoryTranslation
                    {
                        Name       = t.Name,
                        LanguageId = languages.First(l => l.Code == t.LanguageCode).Id,
                        CategoryId = id
                    });
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Category {CategoryId} updated", id);
        
        return await categoryService.GetByIdAsync(id, "en", cancellationToken);
    }
}
