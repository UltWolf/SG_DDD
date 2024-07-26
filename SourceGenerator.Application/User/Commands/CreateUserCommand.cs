
using MediatR;
using AutoMapper;
using OneOf;
using SourceGenerator.Domain.Basic;
using SourceGenerator.Application.User.Dto; 
using SourceGenerator.Domain.User.Entity;
using SourceGenerator.Domain.User.Entity.Repositories; 
using SourceGenerator.Domain.User.Entity.ValueTypes; 

namespace SourceGenerator.Application.User.Commands
{
    public class CreateUserCommand : IRequest<OneOf<UserDto, SourceGenerator.Domain.Basic.BasicError>>
    {
        public CreateUserDto NewUser { get; init; } = null!;
    }

    public class CreateUserHandler : IRequestHandler<CreateUserCommand, OneOf<UserDto, SourceGenerator.Domain.Basic.BasicError>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository;

        public CreateUserHandler(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<OneOf<UserDto, SourceGenerator.Domain.Basic.BasicError>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<UserEntity>(request.NewUser);
            var newEntity = await _repository.AddAsync(entity, cancellationToken);
            return _mapper.Map<UserDto>(newEntity);
        }
    }
}
