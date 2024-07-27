
using MediatR;
using AutoMapper;
using OneOf;
using SourceGenerator.Domain.Basic;
using SourceGenerator.Application.User.Dto;
using SourceGenerator.Domain.User.Entity.Repositories;
using SourceGenerator.Domain.User.Entity.ValueTypes;
using SourceGenerator.Domain.User.Entity.Filters;
namespace SourceGenerator.Application.User.Queries
{
    public class GetUsersQuery : IRequest<OneOf<List<UserDto>, SourceGenerator.Domain.Basic.BasicError>>
    {
        public UserFilter Filter{get;set;}
    }

    public class GetUsersHandler : IRequestHandler<GetUsersQuery, OneOf<List<UserDto>, SourceGenerator.Domain.Basic.BasicError>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository;

        public GetUsersHandler(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<OneOf<List<UserDto>, SourceGenerator.Domain.Basic.BasicError>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var entities = await _repository.GetFiltered(request.Filter,cancellationToken);
            return entities.Any() ? _mapper.Map<List<UserDto>>(entities) : new SourceGenerator.Domain.Basic.BasicError();
        }
    }
}
