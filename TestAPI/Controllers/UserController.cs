

using SourceGenerator;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ApiBaseController
{
    
    [ProducesResponseType(200, Type = typeof(List<UserDto>))]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] UserFilter filter)
    {
        return ReturnOkResult<List<UserDto>>, TrainingError, TrainingErrorCodes>(await Mediator.Send(new GetUsersQuery
        {
            Filter = filter
        }));
    }

    
    [ProducesResponseType(200, Type = typeof(UserDto))]
    [HttpPost]
    public async Task<IActionResult> Post(CreateUserDto createUserDto)
    {
        var result = await Mediator.Send(new CreateUserCommand
        {
            User = createUserDto
        });

        return ReturnOkResult<UserDto, TrainingError, TrainingErrorCodes>(result);
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