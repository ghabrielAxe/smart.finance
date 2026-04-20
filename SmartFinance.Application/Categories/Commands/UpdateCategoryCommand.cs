using FluentValidation;
using MediatR;
using SmartFinance.Domain.Repositories;

namespace SmartFinance.Application.Categories.Commands;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string HexColor,
    string[]? Keywords,
    Guid? ParentId
) : IRequest<bool>;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.HexColor).NotEmpty().MaximumLength(7);
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork
    )
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category == null)
            throw new KeyNotFoundException("Categoria não encontrada.");

        category.Update(request.Name, request.HexColor, request.Keywords, request.ParentId);

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        return await _unitOfWork.CommitAsync(cancellationToken);
    }
}
