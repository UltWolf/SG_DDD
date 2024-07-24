using System.ComponentModel;
using System.Globalization;

namespace SourceGenerator.Domain.Basic
{
    public sealed class StronglyTypedIdTypeConverter : TypeConverter

    {
        public override bool CanConvertFrom(
            ITypeDescriptorContext? context,
            Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(
            ITypeDescriptorContext? context,
            CultureInfo? culture,
            object value)
        {
            var stringValue = value as string;


            return base.ConvertFrom(context, culture, value)!;
        }
    }
}
