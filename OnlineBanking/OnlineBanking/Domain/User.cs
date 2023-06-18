using System;
using OneOf;

namespace OnlineBanking.Domain;

public record User(Guid id, string Name, string Email, UserAccount[] Accounts)
{
    public OneOf<AccountBalance, DomainError> HoldFromCurrencyAccount(decimal amount, string currency)
    {
        if (Accounts?.FirstOrDefault(acc => acc?.Balance?.Currency == currency) is { } account)
        {
            if (!account.Balance.Hold(amount))
            {
                return UserAccountError.InsufficientFunds(account.Balance.MainBalance, amount);
            }

            return account.Balance;
        }

        return UserAccountError.AccountNotFound();
    }

    public OneOf<AccountBalance, DomainError> DepositToCurrencyAccount(decimal amount, string currency)
    {
        if (Accounts?.FirstOrDefault(acc => acc?.Balance?.Currency == currency) is { } account)
        {
            account.Balance.Deposit(amount);
            return account.Balance;
        }

        return UserAccountError.AccountNotFound();
    }

    public OneOf<AccountBalance, DomainError> WithdrawFromCurrencyAccount(decimal amount, string currency)
    {
        if (Accounts?.FirstOrDefault(acc => acc?.Balance?.Currency == currency) is { } account)
        {
            if (!account.Balance.Withdraw(amount))
            {
                return UserAccountError.InsufficientFunds(account.Balance.MainBalance, amount);
            }

            return account.Balance;
        }

        return UserAccountError.AccountNotFound();
    }

    public OneOf<AccountBalance, DomainError> UnHoldFromCurrencyAccount(decimal amount, string currency)
    {
        if (Accounts?.FirstOrDefault(acc => acc?.Balance?.Currency == currency) is { } account)
        {
            if (!account.Balance.Withdraw(amount))
            {
                return UserAccountError.InsufficientFunds(account.Balance.MainBalance, amount);
            }
            account.Balance.Deposit(amount);

            return account.Balance;
        }

        return UserAccountError.AccountNotFound();
    }
}