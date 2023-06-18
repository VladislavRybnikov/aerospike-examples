using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OnlineBanking.Domain;
using OnlineBanking.Domain.Repositories;
using static OnlineBanking.Controllers.Requests;

namespace OnlineBanking.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly IFinancialTransactionRepository _repository;
    private readonly IUserRepository _userRepository;

    public TransactionsController(
        IFinancialTransactionRepository repository,
        IUserRepository userRepository)
	{
        _repository = repository;
        _userRepository = userRepository;
    }

	[HttpPost("deposit")]
	[ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
	public Task<IActionResult> Create(Requests.CreateDeposit request)
	{
        return HandleAsync(request);
    }

    [HttpPost("withdrawal")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public Task<IActionResult> Create(Requests.CreateWithdrawal request)
    {
        return HandleAsync(request);
    }

    [HttpPost("transfer")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public Task<IActionResult> Create(Requests.CreateTransfer request)
    {
        return HandleAsync(request);
    }

	[HttpGet("{id}")]
    [ProducesResponseType(typeof(FinancialTransaction), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Get(Guid id)
    {
        var transaction = await _repository.GetByIdAsync(id);
        return transaction == null ? NotFound() : Ok(transaction);
    }

    [HttpPost("{id}/begin")]
    [ProducesResponseType(typeof(FinancialTransaction), (int)HttpStatusCode.OK)]
    public Task<IActionResult> Begin(Guid id)
    {
        return HandleAsync(id, (transaction, receiver, sender, updatedAt) => transaction
            .BeginProcessing(receiver, sender, updatedAt));
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(FinancialTransaction), (int)HttpStatusCode.OK)]
    public Task<IActionResult> Complete(Guid id)
    {
        return HandleAsync(id, (transaction, receiver, sender, updatedAt) => transaction
            .Complete(receiver, sender, updatedAt));
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(FinancialTransaction), (int)HttpStatusCode.OK)]
    public Task<IActionResult> Cancel(Guid id)
    {
        return HandleAsync(id, (transaction, receiver, sender, updatedAt) => transaction
            .Cancel(receiver, sender, updatedAt));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FinancialTransaction>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAllByUserId([FromQuery] Guid userId, [FromQuery] UserTransactionType type)
	{
		return Ok(type == UserTransactionType.Incomming
            ? await _repository.GetAllIncommingTransactions(userId)
            : await _repository.GetAllOutcommingTransactions(userId));
	}

    private async Task<IActionResult> HandleAsync(ICreateFinancialTransacionRequest request)
    {
        var transaction = request.CreateFinancialTransaction(DateTimeOffset.UtcNow);
        await _repository.InsertAsync(transaction);
        return Ok(transaction.Id);
    }

    private async Task<IActionResult> HandleAsync(
        Guid id,
        Func<FinancialTransaction, User?, User?, DateTimeOffset, OneOf<FinancialTransactionStatus, DomainError>> processStatus)
    {
        var (transaction, receiver, sender) = await GetTransactionDataAsync(id);
        if (transaction == null)
            return NotFound();

        var beforeStatus = transaction.Status;

        var result = processStatus(transaction, receiver, sender, DateTimeOffset.UtcNow);

        if (result.TryPickT0(out var newStatus, out _) && newStatus != beforeStatus)
        {
            await UpdateTransactionData(transaction, receiver, sender);
        }

        return result.MapT0(_ => transaction).Match<IActionResult>(
                success => Ok(success),
                failure => failure.Type switch
                {
                    DomainErrorType.BadRequest => BadRequest(failure),
                    DomainErrorType.NotFound => NotFound(failure),
                    DomainErrorType.Forbidden => Forbid(),
                    _ => throw new ArgumentException("Invalid DomainErrorType.")
                }
            );
    }

    private async Task<(FinancialTransaction? Transaction, User? Receiver, User? Sender)> GetTransactionDataAsync(Guid id)
    {
        var transaction = await _repository.GetByIdAsync(id);
        if (transaction == null)
            return (null, null, null);

        var beforeStatus = transaction.Status;
        (User? receiver, User? sender) = (null, null);

        if (transaction.ReceiverId is { } receiverId)
        {
            receiver = await _userRepository.GetByIdAsync(receiverId);
        }

        if (transaction.SenderId is { } senderId)
        {
            sender = await _userRepository.GetByIdAsync(senderId);
        }

        return (transaction, receiver, sender);
    }

    private async Task UpdateTransactionData(FinancialTransaction transaction, User? receiver, User? sender)
    {
        await _repository.UpdateAsync(transaction);
        if (receiver is not null) await _userRepository.UpdateAsync(receiver);
        if (sender is not null) await _userRepository.UpdateAsync(sender);
    }
}
