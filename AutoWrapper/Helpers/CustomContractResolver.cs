using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using AutoWrapper.Extensions;

namespace AutoWrapper.Helpers
{
    internal class CustomContractResolver<T> : DefaultContractResolver
    {
        private Dictionary<string, string> _propertyMappings { get; set; }
        private readonly bool _useCamelCaseNaming;

        public CustomContractResolver(bool useCamelCaseNaming)
        {
            this._propertyMappings = new Dictionary<string, string>();
            _useCamelCaseNaming = useCamelCaseNaming;
            SetObjectMappings();
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            string resolvedName = null;
            var resolved = this._propertyMappings.TryGetValue(propertyName, out resolvedName);

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
            foreach (PropertyInfo propertyInfo in classType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var wrapperProperty = propertyInfo.GetCustomAttribute<AutoWrapperPropertyMapAttribute>();
                if (wrapperProperty != null)
                {
                    _propertyMappings.Add(wrapperProperty.PropertyName, propertyInfo.Name);
                }

                SetObjectMappings(propertyInfo.PropertyType);
            }
        }

    }
}
