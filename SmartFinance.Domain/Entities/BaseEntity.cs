namespace SmartFinance.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    public void SetUser(Guid userId)
    {
        if (UserId != Guid.Empty && UserId != userId)
            throw new UnauthorizedAccessException("Tentativa de violação de propriedade de dados.");

        UserId = userId;
    }

    public void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
