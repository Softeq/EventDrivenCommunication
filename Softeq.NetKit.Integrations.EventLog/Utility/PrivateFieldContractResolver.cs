using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Softeq.NetKit.Integrations.EventLog.Utility
{
    internal class PrivateFieldContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            MakeWritable(property, member);
            return property;
        }

        internal static JsonProperty MakeWritable(JsonProperty jProperty, MemberInfo member)
        {
            var property = member as PropertyInfo;
            if (property == null)
            {
                return jProperty;
            }

            if (jProperty.Writable)
            {
                jProperty.Writable = property.SetMethod != null;
            }
            else
            {
                jProperty.Writable = true;
                jProperty.ValueProvider = new PrivateBackingFiledValueProvider(property);
            }

            return jProperty;
        }
    }
}
