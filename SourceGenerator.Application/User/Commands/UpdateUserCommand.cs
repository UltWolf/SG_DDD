
using MediatR;
using AutoMapper;

using SourceGenerator.Application.User.Dto; 
using SourceGenerator.Domain.User.Entity;
using SourceGenerator.Domain.User.Entity.Repositories;
using SourceGenerator.Application.User.Errors;

namespace SourceGenerator.Application.User.Commands
{
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public UserDto UpdateUser { get; init; } = null!;
    }

    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository; 

        public UpdateUserHandler(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository; 
        }

        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<User>(request.UpdateUser);
            var updatedEntity = await _repository.UpdateAsync(entity, cancellationToken);
            return _mapper.Map<UserDto>(updatedEntity);
        }
    }
}
