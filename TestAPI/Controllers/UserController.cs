

using MediatR;
using Microsoft.AspNetCore.Mvc;
using SourceGenerator.Application.User.Commands;
using SourceGenerator.Application.User.Dto;
using SourceGenerator.Application.User.Queries;
using SourceGenerator.Domain.User.Entity.Filters;
namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{

    [ProducesResponseType(200, Type = typeof(List<UserDto>))]
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] UserDto dto)
    {
        return Ok(await Mediator.Send(new UpdateUserCommand
        {
            User = dto
        }));
    }

    [ProducesResponseType(200, Type = typeof(List<UserDto>))]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] UserFilter filter)
    {
        return Ok((await Mediator.Send(new GetUsersQuery
        {
            Filter = filter
        })));
    }

    [ProducesResponseType(200, Type = typeof(UserDto))]
    [HttpPost]
    public async Task<IActionResult> Post(CreateUserDto createUserDto)
    {
        var result = await Mediator.Send(new CreateUserCommand
        {
            User = createUserDto
        });
        if (result != null)
        {
            return Ok(result)
}
        return BadRequest();
    }

    [ProducesResponseType(200)]
    [HttpDelete("{UserId}")]
    public async Task<IActionResult> Delete(Guid UserId)
    {
        await Mediator.Send(new DeleteUserCommand
        {
            UserId = UserId
        });
        return Ok();
    }
}