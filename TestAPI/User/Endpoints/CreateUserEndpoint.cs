
using FastEndpoints;
using AutoMapper;
using MediatR; 
using SourceGenerator.Application.User.Dto;
using SourceGenerator.Application.User.Commands;
using SourceGenerator.Application.User.Queries;
using SourceGenerator.Domain.User.Entity.Filters; 

namespace Test.API.User.Endpoints
{
    public class CreateUserEndpoint : Endpoint<CreateUserDto, UserDto>
    {
        private readonly AutoMapper.IMapper _mapper; 
        private readonly IMediator _mediator;

        public CreateUserEndpoint( AutoMapper.IMapper mapper, IMediator mediator)
        {
            _mapper = mapper; 
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/user");
        }
  public override async Task<Results<Ok<UserDto>, NotFound, ProblemDetails>> ExecuteAsync(
         CreateUserDto req, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateUserCommand
            {
                User = req
            });
            if (result.AsT0 != null)
            {
                return TypedResults.Ok(result.AsT0);
            }
            else
            {
                AddError(result.AsT1.Message);
                return new FastEndpoints.ProblemDetails(ValidationFailures);
            }
        }
       
    }
}
