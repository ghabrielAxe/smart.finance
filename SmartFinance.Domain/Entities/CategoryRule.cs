namespace SmartFinance.Domain.Entities;

public class CategoryRule : BaseEntity
{
    public Guid CategoryId { get; private set; }
    public string Keyword { get; private set; }
    public int Priority { get; private set; }

    public virtual Category Category { get; private set; }

    protected CategoryRule() { }

    public CategoryRule(Guid categoryId, string keyword, int priority)
    {
        CategoryId = categoryId;
        Keyword = keyword.ToLowerInvariant();
        Priority = priority;
    }

    public void UpdateCategory(Guid newCategoryId)
    {
        if (newCategoryId == Guid.Empty)
            throw new ArgumentException("ID de categoria inválido.");
        CategoryId = newCategoryId;
        SetUpdatedAt();
    }
}
