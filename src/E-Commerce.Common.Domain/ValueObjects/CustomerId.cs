namespace E_Commerce.Common.Domain.ValueObjects;

public record CustomerId
{
    public Guid Value { get; }

    private CustomerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(value));
        
        Value = value;
    }

    public static CustomerId Create(Guid value) => new(value);
    public static CustomerId Create(string value) => new(Guid.Parse(value));
    public static CustomerId NewId() => new(Guid.NewGuid());

    public static implicit operator Guid(CustomerId customerId) => customerId.Value;
    public static implicit operator string(CustomerId customerId) => customerId.Value.ToString();

    public override string ToString() => Value.ToString();
}
