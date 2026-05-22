using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.Exceptions;

namespace NexCommerce.Application.Features.Categories.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest;

public sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteCategoryCommandHandler> logger)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var id = command.Id;

        var category = await categoryRepository.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Category {id} not found.");

        if (await categoryRepository.HasProductsAsync(id, cancellationToken))
            throw new ValidationException("Cannot delete category with existing products.");

        if (await categoryRepository.HasSubCategoriesAsync(id, cancellationToken))
            throw new ValidationException("Cannot delete category with subcategories.");

        categoryRepository.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Category {CategoryId} deleted", id);
    }
}
