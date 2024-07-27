
using MediatR;
using AutoMapper;
using OneOf;
using SourceGenerator.Domain.Basic;
using SourceGenerator.Application.User.Dto;
using SourceGenerator.Domain.User.Entity.Repositories;
using SourceGenerator.Domain.User.Entity.ValueTypes;

namespace SourceGenerator.Application.User.Queries
{
    public class GetUserQuery : IRequest<OneOf<UserDto, SourceGenerator.Domain.Basic.BasicError>>
    {
        public required Guid UserId { get; init; }
    }

    public class GetUserHandler : IRequestHandler<GetUserQuery, OneOf<UserDto, SourceGenerator.Domain.Basic.BasicError>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository;

        public GetUserHandler(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<OneOf<UserDto, SourceGenerator.Domain.Basic.BasicError>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(new UserId(request.UserId), cancellationToken);
            return entity == null ? new SourceGenerator.Domain.Basic.BasicError() : _mapper.Map<UserDto>(entity);
        }
    }
}
