namespace OnlineBanking.Domain;

public record AccountBalance
{
    public decimal MainBalance { get; private set; }

    public decimal PendingBalance { get; private set; }

    public string? Currency { get; private set; }

    public bool Hold(decimal amount)
    {
        if (amount > MainBalance)
            return false;

        MainBalance -= amount;
        PendingBalance += amount;

        return true;
    }

    public void Deposit(decimal amount)
    {
        MainBalance += amount;
    }

    public bool Withdraw(decimal amount)
    {
        if (amount > PendingBalance) return false;

        PendingBalance -= amount;

        return true;
    }
}