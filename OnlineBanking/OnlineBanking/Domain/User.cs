using System;
using OneOf;

namespace OnlineBanking.Domain;

public record User
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public UserAccount[]? Accounts { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static User CreateWithSingleAccount(
        Guid id,
        string name,
        string email,
        Guid accountId,
        string accountNumber,
        string accountCurrency,
        DateTimeOffset createdAt
        )
    {
        return new User
        {
            Id = id,
            Name = name,
            Email = email,
            Accounts = new[]
            {
                UserAccount.Create(accountId, accountNumber, accountCurrency, createdAt)
            },
            CreatedAt = createdAt.UtcDateTime,
            UpdatedAt = createdAt.UtcDateTime
        };
    }

    public OneOf<AccountBalance, DomainError> HoldFromCurrencyAccount(decimal amount, string currency, DateTimeOffset dateTimeOffset)
    {
        if (Accounts?.FirstOrDefault(acc => acc?.Balance?.Currency == currency) is { } account)
        {
            if (!account.UpdateBalance((balance, updatedAt) => balance.Hold(amount, updatedAt), dateTimeOffset))
            {
                return UserAccountError.InsufficientFunds(account.Balance.MainBalance, amount);
            }

            return account.Balance;
        }

        return UserAccountError.AccountNotFound();
    }

    public OneOf<AccountBalance, DomainError> DepositToCurrencyAccount(decimal amount, string currency, DateTimeOffset dateTimeOffset)
    {
        if (Accounts?.FirstOrDefault(acc => acc?.Balance?.Currency == currency) is { } account)
        {
            account.UpdateBalance((balance, updatedAt) => balance.Deposit(amount, updatedAt), dateTimeOffset);
            return account.Balance;
        }

        return UserAccountError.AccountNotFound();
    }

    public OneOf<AccountBalance, DomainError> WithdrawFromCurrencyAccount(decimal amount, string currency, DateTimeOffset dateTimeOffset)
    {
        if (Accounts?.FirstOrDefault(acc => acc?.Balance?.Currency == currency) is { } account)
        {
            if (!account.UpdateBalance((balance, updatedAt) => balance.Withdraw(amount, updatedAt), dateTimeOffset))
            {
                return UserAccountError.InsufficientFunds(account.Balance.MainBalance, amount);
            }

            return account.Balance;
        }

        return UserAccountError.AccountNotFound();
    }

    public OneOf<AccountBalance, DomainError> UnHoldFromCurrencyAccount(decimal amount, string currency, DateTimeOffset dateTimeOffset)
    {
        if (Accounts?.FirstOrDefault(acc => acc?.Balance?.Currency == currency) is { } account)
        {
            if (!account.UpdateBalance((balance, updatedAt) => balance.Withdraw(amount, updatedAt), dateTimeOffset))
            {
                return UserAccountError.InsufficientFunds(account.Balance.MainBalance, amount);
            }
            account.UpdateBalance((balance, updatedAt) => balance.Deposit(amount, updatedAt), dateTimeOffset);

            return account.Balance;
        }

        return UserAccountError.AccountNotFound();
    }
}