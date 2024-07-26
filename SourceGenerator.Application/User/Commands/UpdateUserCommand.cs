
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
    public class UpdateUserCommand : IRequest<OneOf<UserDto, SourceGenerator.Domain.Basic.BasicError>>
    {
        public UserDto UpdateUser { get; init; } = null!;
    }

    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, OneOf<UserDto, SourceGenerator.Domain.Basic.BasicError>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository; 

        public UpdateUserHandler(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository; 
        }

        public async Task<OneOf<UserDto, SourceGenerator.Domain.Basic.BasicError>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<UserEntity>(request.UpdateUser);
            var updatedEntity = await _repository.UpdateAsync(entity, cancellationToken);
            return _mapper.Map<UserDto>(updatedEntity);
        }
    }
}
