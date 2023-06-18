using System;

namespace OnlineBanking.Domain;

public record AccountBalance
{
    public string? Currency { get; set; }

    public decimal MainBalance { get; set; }

    public decimal PendingBalance { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static AccountBalance Create(string currency, DateTimeOffset createdAt)
    {
        return new AccountBalance
        {
            Currency = currency,
            CreatedAt = createdAt.UtcDateTime,
            UpdatedAt = createdAt.UtcDateTime
        };
    }

    public bool Hold(decimal amount, DateTimeOffset dateTimeOffset)
    {
        if (amount > MainBalance)
            return false;

        MainBalance -= amount;
        PendingBalance += amount;
        UpdatedAt = dateTimeOffset.UtcDateTime;

        return true;
    }

    public bool Deposit(decimal amount, DateTimeOffset dateTimeOffset)
    {
        MainBalance += amount;
        UpdatedAt = dateTimeOffset.UtcDateTime;

        return true;
    }

    public bool Withdraw(decimal amount, DateTimeOffset dateTimeOffset)
    {
        if (amount > PendingBalance) return false;

        PendingBalance -= amount;
        UpdatedAt = dateTimeOffset.UtcDateTime;

        return true;
    }
}