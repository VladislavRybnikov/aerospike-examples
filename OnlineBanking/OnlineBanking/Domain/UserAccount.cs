namespace OnlineBanking.Domain;

public record UserAccount
{
    public Guid Id { get; set; }

    public string? Number { get; set; }

    public AccountBalance? Balance { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static UserAccount Create(
        Guid id,
        string number,
        string currency,
        DateTimeOffset createdAt)
    {
        return new UserAccount
        {
            Id = id,
            Number = number,
            Balance = AccountBalance.Create(currency, createdAt),
            CreatedAt = createdAt.UtcDateTime,
            UpdatedAt = createdAt.UtcDateTime
        };
    }

    public bool UpdateBalance(Func<AccountBalance, DateTimeOffset, bool> updateBalance, DateTimeOffset updatedAt)
    {
        UpdatedAt = updatedAt.UtcDateTime;
       
        return updateBalance(Balance, updatedAt);
    }
}