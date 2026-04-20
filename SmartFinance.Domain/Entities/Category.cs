namespace SmartFinance.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; }
    public string HexColor { get; private set; }
    public Guid? ParentId { get; private set; }
    public string[] Keywords { get; private set; }

    public virtual Category ParentCategory { get; private set; }

    protected Category() { }

    public Category(string name, string hexColor, string[] keywords = null, Guid? parentId = null)
    {
        Name = name;
        HexColor = hexColor;
        ParentId = parentId;
        Keywords = keywords ?? Array.Empty<string>();
    }

    public void Update(string name, string hexColor, string[]? keywords, Guid? parentId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome da categoria é obrigatório.");

        Name = name;
        HexColor = hexColor;
        Keywords = keywords ?? Array.Empty<string>();
        ParentId = parentId;
        SetUpdatedAt();
    }
}
