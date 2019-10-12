using Newtonsoft.Json;

namespace AutoWrapper.Contracts
{
    internal interface IJsonSettings
    {
        JsonSerializerSettings GetJSONSettings(bool ignoreNull, bool useCamelCaseNaming = true);
    }
}
