using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using OnlineBanking.Domain;
using static OnlineBanking.Controllers.Requests;

namespace OnlineBanking.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly IFinancialTransactionRepository _repository;

    public TransactionsController(IFinancialTransactionRepository repository)
	{
        _repository = repository;
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
        return Ok(await _repository.GetByIdAsync(id));
    }

    [HttpPost("{id}/process")]
    [ProducesResponseType(typeof(FinancialTransaction), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Process(Guid id)
    {
        return Ok("TODO");
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(FinancialTransaction), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Complete(Guid id)
    {
        return Ok("TODO");
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(FinancialTransaction), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        return Ok("TODO");
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
}
