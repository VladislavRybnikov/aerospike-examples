using OnlineBanking.Domain;
using OnlineBanking.Domain.FinancialTransactionDetails;

namespace OnlineBanking.Controllers;

public interface ICreateFinancialTransacionRequest
{
    FinancialTransaction ToFinancialTransaction();
}

public static class Requests
{
	public record CreateDeposit(
        Guid ReceiverId,
        decimal Amount,
        DepositDetails? Details,
        string? Comment) : ICreateFinancialTransacionRequest
    {
		public FinancialTransaction ToFinancialTransaction()
			=> FinancialTransaction.CreateDeposit(
				Guid.NewGuid(),
				ReceiverId,
				Amount,
				Details,
				Comment);

    }

	public record CreateWithdrawal(
        Guid SenderId,
        decimal Amount,
        WithdrwalDetails? Details,
        string? Comment) : ICreateFinancialTransacionRequest
    {
        public FinancialTransaction ToFinancialTransaction()
            => FinancialTransaction.CreateWithdrawal(
                Guid.NewGuid(),
                SenderId,
                Amount,
                Details,
                Comment);
    }

    public record CreateTransfer(
        Guid SenderId,
        Guid ReceiverId,
        decimal Amount,
        TransferDetails? Details,
        string? Comment) : ICreateFinancialTransacionRequest
    {
        public FinancialTransaction ToFinancialTransaction()
            => FinancialTransaction.CreateTransfer(
                Guid.NewGuid(),
                SenderId,
                ReceiverId,
                Amount,
                Details,
                Comment);
    }
}