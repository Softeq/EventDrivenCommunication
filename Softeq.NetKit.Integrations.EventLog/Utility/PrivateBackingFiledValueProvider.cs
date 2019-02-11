using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Softeq.NetKit.Integrations.EventLog.Utility
{
    internal class PrivateBackingFiledValueProvider : IValueProvider
    {
        private readonly PropertyInfo _property;

        public PrivateBackingFiledValueProvider(PropertyInfo property)
        {
            _property = property;
        }

        public void SetValue(object target, object value)
        {
            var backing = GetBackingField(_property, _property.Name);
            backing.SetValue(target, value);
        }

        public object GetValue(object target)
        {
            object value = _property.GetValue(target);
            return value;
        }

        private static string GetBackingFieldName(string propertyName)
        {
            return $"<{propertyName}>k__BackingField";
        }

        private static FieldInfo GetBackingField(PropertyInfo property, string propertyName)
        {
            return property.DeclaringType.GetField(GetBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}
