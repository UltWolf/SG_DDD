
using MediatR;
using AutoMapper;

using SourceGenerator.Application.User.Dto;
using SourceGenerator.Application.User.Service;
using SourceGenerator.Domain.User.Entity;
using SourceGenerator.Domain.User.Entity.Repositories;
using SourceGenerator.Application.User.Errors;

namespace SourceGenerator.Application.User.Commands
{
    public class CreateUserCommand : IRequest<UserDto>
    {
        public CreateUserDto NewUser { get; init; } = null!;
    }

    public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository;

        public CreateUserHandler(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<User>(request.NewUser);
            var newEntity = await _repository.AddAsync(entity, cancellationToken);
            return _mapper.Map<UserDto>(newEntity);
        }
    }
}
