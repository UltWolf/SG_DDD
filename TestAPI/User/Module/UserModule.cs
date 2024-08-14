
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SourceGenerator.Application.User.Dto;
using SourceGenerator.Application.User.Commands;
using SourceGenerator.Application.User.Queries;
using SourceGenerator.Domain.User.Entity.Filters;
using Swashbuckle.AspNetCore.Annotations;

namespace Test.API.Modules
{
    public static class UserModule
    {
        public static WebApplication AddUserEndpointsModule(this WebApplication app)
        {
            
    app.MapPost("/users", async (CreateUserDto user, IMediator mediator) =>
    {
        var command = new CreateUserCommand(){User = user};
        var result = await mediator.Send(command);
          return result.IsT0 ?  Results.Ok(new { Message = "User created successfully.", User = result.AsT0 }): Results.BadRequest(result.AsT0);
    })
    .WithName("CreateUser")
    .WithMetadata(new SwaggerOperationAttribute(summary: "Create User", description: "Creates a new User"))
    .Produces<UserDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);

            
    app.MapGet("/users/{id}", async (Guid id, IMediator mediator) =>
    {
        var query = new GetUserQuery(){UserId =  id};
        var result = await mediator.Send(query);
        return result.IsT0 ? Results.Ok(result.AsT0) : Results.NotFound(new { Message = $"User with ID {id} not found." });
    })
    .WithName("GetUserById")
    .WithMetadata(new SwaggerOperationAttribute(summary: "Get User by ID", description: "Fetches a User by their ID"))
    .Produces<UserDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

            
    app.MapPut("/users", async (UserDto user, IMediator mediator) =>
    {
        var command = new UpdateUserCommand(){User= user };
        var result = await mediator.Send(command);
        return result.IsT0 ? Results.Ok(new { Message = "User updated successfully.", User = result.AsT0 }) : Results.NotFound(new { Message = $"User   not found." });
    })
    .WithName("UpdateUser")
    .WithMetadata(new SwaggerOperationAttribute(summary: "Update User", description: "Updates an existing User"))
    .Produces<UserDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

            
    app.MapDelete("/users/{id}", async (Guid id, IMediator mediator) =>
    {
        var command = new DeleteUserCommand(){UserId = id};
          await mediator.Send(command);
                return Results.Ok(new { Message = "User deleted successfully." });
    })
    .WithName("DeleteUser")
    .WithMetadata(new SwaggerOperationAttribute(summary: "Delete User", description: "Deletes an existing User"))
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

            
            return app;
        }
    }
}
