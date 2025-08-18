namespace E_Commerce.Common.Domain.ValueObjects;

public record FullName
{
    public string FirstName { get; }
    public string LastName { get; }
    public string? MiddleName { get; }

    private FullName(string firstName, string lastName, string? middleName = null)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
    }

    public static FullName Create(string firstName, string lastName, string? middleName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        return new FullName(firstName.Trim(), lastName.Trim(), middleName?.Trim());
    }

    public string GetFullName()
    {
        return string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";
    }

    public override string ToString() => GetFullName();

    public static implicit operator string(FullName fullName) => fullName.ToString();
}
