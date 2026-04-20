using FluentValidation;
using MediatR;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;

namespace SmartFinance.Application.Categories.Commands;

public record CreateCategoryCommand(
    string Name,
    string HexColor,
    string[]? Keywords,
    Guid? ParentId
) : IRequest<Guid>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.HexColor).NotEmpty().MaximumLength(7);
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork
    )
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var category = new Category(
            request.Name,
            request.HexColor,
            request.Keywords,
            request.ParentId
        );

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return category.Id;
    }
}
