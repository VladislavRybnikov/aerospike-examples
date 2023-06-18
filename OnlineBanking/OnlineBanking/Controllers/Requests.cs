using OnlineBanking.Domain;
using OnlineBanking.Domain.FinancialTransactionDetails;

namespace OnlineBanking.Controllers;

public interface ICreateFinancialTransacionRequest
{
    FinancialTransaction CreateFinancialTransaction(DateTimeOffset dateTimeOffsetNow);
}

public interface ICreateUserRequest
{
    User CreateUser(DateTimeOffset dateTimeOffsetNow);
}

public static class Requests
{
	public record CreateDeposit(
        Guid ReceiverId,
        decimal Amount,
        string Currency,
        DepositDetails? Details,
        string? Comment) : ICreateFinancialTransacionRequest
    {
		public FinancialTransaction CreateFinancialTransaction(DateTimeOffset dateTimeOffsetNow)
			=> FinancialTransaction.CreateDeposit(
				Guid.NewGuid(),
				ReceiverId,
				Amount,
                Currency,
                dateTimeOffsetNow,
                Details,
				Comment);

    }

	public record CreateWithdrawal(
        Guid SenderId,
        decimal Amount,
        string Currency,
        WithdrwalDetails? Details,
        string? Comment) : ICreateFinancialTransacionRequest
    {
        public FinancialTransaction CreateFinancialTransaction(DateTimeOffset dateTimeOffsetNow)
            => FinancialTransaction.CreateWithdrawal(
                Guid.NewGuid(),
                SenderId,
                Amount,
                Currency,
                dateTimeOffsetNow,
                Details,
                Comment);
    }

    public record CreateTransfer(
        Guid SenderId,
        Guid ReceiverId,
        decimal Amount,
        string Currency,
        TransferDetails? Details,
        string? Comment) : ICreateFinancialTransacionRequest
    {
        public FinancialTransaction CreateFinancialTransaction(DateTimeOffset dateTimeOffsetNow)
            => FinancialTransaction.CreateTransfer(
                Guid.NewGuid(),
                SenderId,
                ReceiverId,
                Amount,
                Currency,
                dateTimeOffsetNow,
                Details,
                Comment);
    }

    public record RegisterUser(
        string Name,
        string Email,
        string AccountNumber,
        string AccountCurrency
        ) : ICreateUserRequest
    {
        public User CreateUser(DateTimeOffset dateTimeOffsetNow)
            => User.CreateWithSingleAccount(
                Guid.NewGuid(),
                Name,
                Email,
                Guid.NewGuid(),
                AccountNumber,
                AccountCurrency,
                dateTimeOffsetNow);
    }

    public enum UserTransactionType
    {
        Incomming,
        Outcomming
    }
}