
using FastEndpoints;
using AutoMapper;
using MediatR;  
using SourceGenerator.Application.User.Dto;
using SourceGenerator.Application.User.Commands;
using SourceGenerator.Application.User.Queries;
using SourceGenerator.Domain.User.Entity.Filters; 

namespace Test.API.User.Endpoints
{
    public class UpdateUserEndpoint : Endpoint<UserDto, UserDto>
    {
        private readonly AutoMapper.IMapper _mapper; 
        private readonly IMediator _mediator;

        public UpdateUserEndpoint( AutoMapper.IMapper mapper, IMediator mediator)
        {
            _mapper = mapper; 
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/user");
        }

        public override async Task HandleAsync(UserDto request, CancellationToken cancellationToken)
        {
             var entity = await _mediator.Send(new UpdateUserCommand{User= request}); 
            await SendAsync(_mapper.Map<UserDto>(entity));
        }
    }
}
