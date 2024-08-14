
using FastEndpoints;
using AutoMapper;
using MediatR; 
using SourceGenerator.Application.User.Dto;
using SourceGenerator.Application.User.Commands;
using SourceGenerator.Application.User.Queries;
using SourceGenerator.Domain.User.Entity.Filters; 
namespace Test.API.User.Queries
{
    public class GetUserEndpoint : Endpoint<GetUserRequest, UserDto>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository;

        public GetUserEndpoint(IUserRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public override void Configure()
        {
            Get("/user/{UserId}");
        }

        public override async Task HandleAsync(GetUserRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetAsync(new UserEntity() { Id = new UserId(request.UserId) }, cancellationToken);
            await SendAsync(_mapper.Map<UserDto>(entity));
        }
    }
}
