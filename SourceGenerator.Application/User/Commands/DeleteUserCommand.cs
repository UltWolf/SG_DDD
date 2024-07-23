
using MediatR;
using SourceGenerator.Domain.User.Entity;
using SourceGenerator.Domain.User.Entity.Repositories;

namespace SourceGenerator.Domain.User.Entity.Commands
{
    public class DeleteUserCommand : IRequest
    {
        public required Guid UserId { get; init; }
    }

    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly IUserRepository _repository;

        public DeleteUserHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            return _repository.DeleteAsync(new User() { Id = new UserId(request.UserId) }, cancellationToken);
        }
    }
}
