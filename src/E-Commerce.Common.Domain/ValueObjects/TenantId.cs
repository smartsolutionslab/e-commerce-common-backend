namespace E_Commerce.Common.Domain.ValueObjects;

public record TenantId
{
    public Guid Value { get; }

    private TenantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(value));
        
        Value = value;
    }

    public static TenantId Create(Guid value) => new(value);
    public static TenantId Create(string value) => new(Guid.Parse(value));
    public static TenantId NewId() => new(Guid.NewGuid());

    public static implicit operator Guid(TenantId tenantId) => tenantId.Value;
    public static implicit operator string(TenantId tenantId) => tenantId.Value.ToString();

    public override string ToString() => Value.ToString();
}
