using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using AutoWrapper.Extensions;

namespace AutoWrapper.Helpers
{
    public class CustomContractResolver<T> : DefaultContractResolver
    {
        public Dictionary<string, string> _propertyMappings { get; set; }
        private readonly bool _useCamelCaseNaming;

        public CustomContractResolver(bool useCamelCaseNaming)
        {
            _propertyMappings = new Dictionary<string, string>();
            _useCamelCaseNaming = useCamelCaseNaming;
            SetObjectMappings();
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            var resolved = _propertyMappings.TryGetValue(propertyName, out string resolvedName);

            if (_useCamelCaseNaming)
                return (resolved) ? resolvedName.ToCamelCase() : base.ResolvePropertyName(propertyName.ToCamelCase());

            return (resolved) ? resolvedName : base.ResolvePropertyName(propertyName);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            return prop;
        }

        private void SetObjectMappings()
        {
            SetObjectMappings(typeof(T));
        }

        private void SetObjectMappings(Type classType)
        {
            foreach (PropertyInfo propertyInfo in classType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {

                var wrapperProperty = propertyInfo.GetCustomAttribute<AutoWrapperPropertyMapAttribute>();
                if (wrapperProperty != null)
                {
                    _propertyMappings.Add(wrapperProperty.PropertyName, propertyInfo.Name);
                }

                var type = propertyInfo.PropertyType;
                if (type.IsClass)
                    SetObjectMappings(propertyInfo.PropertyType);
            }
        }

    }
}
