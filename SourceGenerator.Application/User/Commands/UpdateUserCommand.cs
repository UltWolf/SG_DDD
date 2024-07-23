
using MediatR;

using SourceGenerator.Domain.User.Entity.Dto;
using SourceGenerator.Domain.User.Entity.Service;
using SourceGenerator.Domain.User.Entity;
using SourceGenerator.Domain.User.Entity.Repositories;
using SourceGenerator.Domain.User.Entity.Errors;

namespace SourceGenerator.Domain.User.Entity.Commands
{
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public UserDto UpdateUser { get; init; } = null!;
    }

    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository;
        private readonly IUserService _service;

        public UpdateUserHandler(IUserRepository repository, IMapper mapper, IUserService service)
        {
            _mapper = mapper;
            _repository = repository;
            _service = service;
        }

        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<User>(request.UpdateUser);
            var updatedEntity = await _repository.UpdateAsync(entity, cancellationToken);
            return _mapper.Map<UserDto>(updatedEntity);
        }
    }
}
