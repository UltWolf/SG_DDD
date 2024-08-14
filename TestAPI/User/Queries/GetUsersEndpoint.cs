
using FastEndpoints;
using AutoMapper;
using MediatR; 
using SourceGenerator.Application.User.Dto;
using SourceGenerator.Application.User.Commands;
using SourceGenerator.Application.User.Queries;
using SourceGenerator.Domain.User.Entity.Filters; 

namespace Test.API.User.Queries
{
    public class GetUsersEndpoint : EndpointWithoutRequest<List<UserDto>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository;

        public GetUsersEndpoint(IUserRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public override void Configure()
        {
            Get("/users");
        }

        public override async Task HandleAsync(CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync(cancellationToken);
            var dtos = _mapper.Map<List<UserDto>>(entities);
            await SendAsync(dtos);
        }
    }
}
