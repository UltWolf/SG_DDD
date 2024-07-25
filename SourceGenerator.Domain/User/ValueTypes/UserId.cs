
using System;
using System.ComponentModel;
using  SourceGenerator.Domain.Basic;
using  SourceGenerator.Domain.Basic;
namespace SourceGenerator.Domain.User.Entity.ValueTypes
{
    [TypeConverter(typeof(StronglyTypedIdTypeConverter<UserId>))]
    public record UserId(Guid Value) : IStronglyTypeGuidId;
}
