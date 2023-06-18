using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using OnlineBanking.Domain;
using OnlineBanking.Domain.Repositories;

namespace OnlineBanking.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;

    public UsersController(IUserRepository repository)
	{
        _repository = repository;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Register(Requests.RegisterUser request)
    {
        var user = request.CreateUser(DateTimeOffset.UtcNow);
        await _repository.InsertAsync(user);

        return Ok(user.Id);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Get(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }
}
