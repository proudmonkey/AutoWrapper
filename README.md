<img align="right" src="/src/AutoWrapper/logo.png" />

# AutoWrapper  [![Nuget](https://img.shields.io/nuget/v/AutoWrapper.Core?color=blue)](https://www.nuget.org/packages/AutoWrapper.Core) [![Nuget downloads](https://img.shields.io/nuget/dt/AutoWrapper.Core?color=green)](https://www.nuget.org/packages/AutoWrapper.Core) ![.NET Core](https://github.com/proudmonkey/AutoWrapper/workflows/.NET%20Core/badge.svg)

Language: English | [中文](README.zh-cn.md)  

`AutoWrapper` is a simple, yet customizable global `HTTP` exception handler and response wrapper for ASP.NET Core APIs. It uses an ASP.NET Core `middleware` to intercept incoming `HTTP` requests and automatically wraps the responses for you by providing a consistent response format for both successful and error results. The goal is to let you focus on your business code specific requirements and let the wrapper automatically handle the `HTTP` response. This can speedup the development time when building your APIs while enforcing own standards for your `HTTP` responses.

#### Main features:

* Exception handling.
* A configurable middleware `options` to configure the wrapper. 
* `ModelState` validation error handling (support both `Data Annotation` and `FluentValidation`).
* A configurable `API` exception.
* A consistent response format for `Result` and `Errors`.
* A detailed `Result` response.
* A detailed `Error` response.
* A configurable `HTTP` `StatusCodes` and messages.
* Add Logging support for `Request`, `Response` and `Exceptions`.
* Add support for Problem Details exception format.
* Add support for ignoring action methods that don't need to be wrapped using `[AutoWrapIgnore]` filter attribute.

# Breaking changes
* This release supports .NET 5 and .NET 6
* Middleware has been renamed from `UseApiResponseAndExceptionWrapper` to `UseAutoWrapper`. Make sure to update your `Startup.cs` to use the new name.
* `ProblemDetails` is now the default exception format
* `UseApiProblemDetailsException` has been renamed to `DisableProblemDetailsException`
* Removed `Newtonsoft.Json` dependency and replaced it with `System.Text.Json`
* Use the interface `IApiResponse` model instead of the concrete `ApiResponse` model for returning responses using the default format. This allows you to add your own properties that will wrapped within the `Result` property. 
* `AutoWrapIgnore` and `RequestDataLogIgnore` attributes now leaves under `AutoWrapper.Attributes` namespace. The implementation was changed from using `IActionFilter` to use `Attribute`, eliminating all the request header logic.
* The following options has been removed:
   * ApiVersion
   * ReferenceLoopHandling
   * UseCustomSchema

> `ReferenceLoopHandling` and `DefaultContractResolver` aren't still supported in .NET 5 that's why handling reference loop and `ApiResponse` property mappings will not be supported when targetting .NET 5.
> .NET Core 3.1 will still use Newtonsoft.Json and it's only supported by AutoWrapper <= v4.5.1

# Installation
1. Download and Install the latest `AutoWrapper.Core` from NuGet or via CLI:

```
PM> Install-Package AutoWrapper.Core -Version 5.0.0-rc-04
```

2. Declare the following namespace within `Startup.cs`

```csharp
using AutoWrapper;
```
3. Register the `middleware` below within the `Configure()` method of `Startup.cs` "before" the `UseRouting()` `middleware`:

```csharp
app.UseAutoWrapper();
```
That's simple! Here’s how the response is going to look like for the default ASP.NET Core API template “`WeatherForecastController`” API:

```json
{
    "message": "GET Request successful.",
    "result": [
        {
            "date": "2019-09-16T23:37:51.5544349-05:00",
            "temperatureC": 21,
            "temperatureF": 69,
            "summary": "Mild"
        },
        {
            "date": "2019-09-17T23:37:51.554466-05:00",
            "temperatureC": 28,
            "temperatureF": 82,
            "summary": "Cool"
        },
        {
            "date": "2019-09-18T23:37:51.554467-05:00",
            "temperatureC": 21,
            "temperatureF": 69,
            "summary": "Sweltering"
        },
        {
            "date": "2019-09-19T23:37:51.5544676-05:00",
            "temperatureC": 53,
            "temperatureF": 127,
            "summary": "Chilly"
        },
        {
            "date": "2019-09-20T23:37:51.5544681-05:00",
            "temperatureC": 22,
            "temperatureF": 71,
            "summary": "Bracing"
        }
    ]
}
```

# ASP.NET Core 3.x
For ASP.NET Core 3.x versions, see the documentation here: [AutoWrapper v4.x](https://github.com/proudmonkey/AutoWrapper/tree/v4.x#readme)

# Unwrapping the Result from .NET Client 
[AutoWrapper.Server](https://github.com/proudmonkey/AutoWrapper.Server) is simple library that enables you unwrap the `Result` property of the AutoWrapper's `ApiResponse` object in your C# .NET Client code. The goal is to deserialize the `Result` object directly to your matching `Model` without having you to create the ApiResponse schema. 

For example:

```csharp
[HttpGet]
public async Task<IEnumerable<PersonDTO>> Get()
{
    var client = HttpClientFactory.Create();
    var httpResponse = await client.GetAsync("https://localhost:5001/api/v1/people");

    IEnumerable<PersonDTO> people = null;
    if (httpResponse.IsSuccessStatusCode)
    {
        var jsonString = await httpResponse.Content.ReadAsStringAsync();
        people = Unwrapper.Unwrap<IEnumerable<PersonDTO>>(jsonString);
    }

    return people;
}
```

For more information, see: [AutoWrapper.Server](https://github.com/proudmonkey/AutoWrapper.Server)
# Samples
* [AutoWrapper: Prettify Your ASP.NET Core APIs with Meaningful Responses](http://vmsdurano.com/autowrapper-prettify-your-asp-net-core-apis-with-meaningful-responses/)
* [AutoWrapper: Customizing the Default Response Output](http://vmsdurano.com/asp-net-core-with-autowrapper-customizing-the-default-response-output/)
* [AutoWrapper Now Supports Problem Details For Your ASP.NET Core APIs](http://vmsdurano.com/autowrapper-now-supports-problemdetails/)
* [AutoWrapper.Server: Sample Usage](http://vmsdurano.com/autowrapper-server-is-now-available/)

# Feedback and Give a Star! :star:
I’m pretty sure there are still lots of things to improve in this project. Try it out and let me know your thoughts.

Feel free to submit a [ticket](https://github.com/proudmonkey/AutoWrapper/issues) if you find bugs or request a new feature. Your valuable feedback is much appreciated to better improve this project. If you find this useful, please give it a star to show your support for this project.

# Contributors

* **Vincent Maverick Durano** - [Blog](http://vmsdurano.com/)
* **Huei Feng** - [Github Profile](https://github.com/hueifeng)
* **ITninja04** - [Github Profile](https://github.com/ITninja04)
* **Rahmat Slamet** - [Github Profile](https://github.com/arhen)
* **abelfiore** - [Github Profile](https://github.com/abelfiore)

Want to contribute? Please read the CONTRIBUTING docs [here](https://github.com/proudmonkey/AutoWrapper/blob/master/CONTRIBUTING.md).

# Release History 

See: [Release Log](https://github.com/proudmonkey/AutoWrapper/blob/master/RELEASE.MD)

# License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE) file for details.

# Donate
If you find this project useful — or just feeling generous, consider buying me a beer or a coffee. Cheers! :beers: :coffee:
|               |               |
| ------------- |:-------------:|
|   <a href="https://www.paypal.me/vmsdurano"><img src="https://github.com/proudmonkey/Resources/blob/master/donate_paypal.svg" height="40"></a>   | [![BMC](https://github.com/proudmonkey/Resources/blob/master/donate_coffee.png)](https://www.buymeacoffee.com/ImP9gONBW) |


Thank you!
