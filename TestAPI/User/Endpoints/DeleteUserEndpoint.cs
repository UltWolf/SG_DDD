
using FastEndpoints;
using AutoMapper;
using MediatR;
using SourceGenerator.Domain.User.Entity;
using SourceGenerator.Application.User.Dto;
using SourceGenerator.Application.User.Commands;
using SourceGenerator.Application.User.Queries;
using SourceGenerator.Domain.User.Entity.Filters; 

namespace Test.API.User.Endpoints
{
    public class DeleteUserEndpoint : Endpoint<DeleteUserRequest>
    { 
        private readonly IMediator _mediator;

        public DeleteUserEndpoint(  IMediator mediator)
        { 
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/user/{UserId}");
        }

        public override async Task HandleAsync(DeleteUserRequest request, CancellationToken cancellationToken)
        { 
            await _mediator.Send(new DeleteUserCommand() {UserId = request.Id.Value});
            await SendOkAsync();
        }
    }
 public class DeleteUserRequest
    {
        public UserId Id { get; set; }
    }
}
