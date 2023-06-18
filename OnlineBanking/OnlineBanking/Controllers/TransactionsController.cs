﻿using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using OnlineBanking.Domain;

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

    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(FinancialTransaction), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateSatatus(Guid id, [FromBody] FinancialTransactionStatus status)
	{
		await _repository.UpdateStatusAsync(id, status);
		return Ok(await _repository.GetByIdAsync(id));
	}

	[HttpGet("{id}")]
    [ProducesResponseType(typeof(FinancialTransaction), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await _repository.GetByIdAsync(id));
    }

	[HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FinancialTransaction>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAllByUserId([FromQuery] Guid userId)
	{
		return Ok(await _repository.GetAllIncommingTransactions(userId));
	}

    private async Task<IActionResult> HandleAsync(ICreateFinancialTransacionRequest request)
    {
        var transaction = request.ToFinancialTransaction();
        await _repository.InsertAsync(transaction);
        return Ok(transaction.Id);
    }
}
