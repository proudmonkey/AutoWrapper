<img align="right" src="/AutoWrapper/logo.png" />

# AutoWrapper  [![Nuget](https://img.shields.io/nuget/v/AutoWrapper.Core?color=blue)](https://www.nuget.org/packages/AutoWrapper.Core) [![Nuget downloads](https://img.shields.io/nuget/dt/AutoWrapper.Core?color=green)](https://www.nuget.org/packages/AutoWrapper.Core)

The `AutoWrapper` is a global exception handler and response wrapper for ASP.NET Core APIs. It uses a `middleware` to intercept incoming HTTP requests and automatically wraps the responses for you by providing a consistent response format for both successful and error results. The goal is to let you focus on your business specific requirements and let the wrapper handles the `HTTP` response. This saves you time from developing your APIs while enforcing own standards for your `HTTP` responses.

`AutoWrapper` is a project fork based from [VMD.RESTApiResponseWrapper.Core](https://github.com/proudmonkey/RESTApiResponseWrapper.Core) which is designed to support .NET Core 3.x and above. The implementation of this package was refactored to provide a more convenient way to use the middleware with added flexibility.

#### Main features:

* Exception handling
* `ModelState` validation error handling (support both `Data Annotation` and `FluentValidation`)
* A configurable `API` exception
* A consistent response format for `Result` and `Errors`
* A detailed `Result` response
* A detailed `Error` response
* A configurable `HTTP` `StatusCodes` and messages
* Add support for `Swagger`
* Add Logging support for `Request`, `Response` and `Exceptions`
* A configurable middleware `options` to configure the wrapper.

# Installation
1. Download and Install the latest `AutoWrapper.Core` from NuGet or via CLI:

```
PM> Install-Package AutoWrapper.Core -Version 1.0.0
```

2. Declare the following namespace within `Startup.cs`

```csharp
using AutoWrapper;
```
3. Register the `middleware` below within the `Configure()` method of `Startup.cs` "before" the `UseRouting()` `middleware`:

```csharp
app.UseApiResponseAndExceptionWrapper();
```
That's simple! Here’s how the response is going to look like for the default ASP.NET Core API template “`WeatherForecastController`” API:

```json
{
    "message": "Request successful.",
    "isError": false,
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

# Defining Your Own Custom Message
To display a custom message in your response, use the `ApiResponse` object from `AutoWrapper.Wrappers` namespace. For example, if you want to display a message when a successful `POST` has been made, then you can do something like this:

```csharp
[HttpPost]
public async Task<ApiResponse> Post([FromBody]CreateBandDTO band)  
{
    //Call a method to add a new record to the database
    try
    {
        var result = await SampleData.AddNew(band);
        return new ApiResponse("New record has been created to the database", result, 201);
    }
    catch (Exception ex)
    {
        //TO DO: Log ex
        throw;
    }
}
```
Running the code will give you the following result when successful:

```json
{
    "message": "New record has been created to the database",
    "isError": false,
    "result": 100
}
```
The `ApiResponse` object has the following parameters that you can set:

```csharp
ApiResponse(string message, object result = null, int statusCode = 200, string apiVersion = "1.0.0.0")  
```

# Defining Your Own Api Exception
`AutoWrapper` also provides an `ApiException` object that you can use to define your own exception. For example, if you want to throw your own exception message, you could simply do:

#### For capturing ModelState validation errors

```csharp
throw new ApiException(ModelState.AllErrors());
```

#### For throwing your own exception message
```csharp
throw new ApiException($"Record with id: {id} does not exist.", 400);
```
For example, let’s modify the `POST` method with `ModelState` validation:

```csharp
[HttpPost]
public async Task<ApiResponse> Post([FromBody]CreateBandDTO band)
{
    if (ModelState.IsValid)
    {
        //Call a method to add a new record to the database
        try
        {
            var result = await SampleData.AddNew(band);
            return new ApiResponse("New record has been created to the database", result, 201);
        }
        catch (Exception ex)
        {
            //TO DO: Log ex
            throw;
        }
    }
    else
        throw new ApiException(ModelState.AllErrors());
}
```
Running the code will result to something like this when validation fails:

```json
{
    "isError": true,
    "responseException": {
        "exceptionMessage": "Request responded with validation error(s). Please correct the specified validation errors and try again.",
        "details": null,
        "referenceErrorCode": null,
        "referenceDocumentLink": null,
        "validationErrors": [
            {
                "field": "Name",
                "message": "The Name field is required."
            }
        ]
    }
}
```

See how the `validationErrors` property is automatically populated with the violated `fields` from your model.

The `ApiException` object contains the following three overload constructors that you can use to define an exception:

```csharp
ApiException(string message, int statusCode = 500, string errorCode = "", string refLink = "")
ApiException(IEnumerable<ValidationError> errors, int statusCode = 400)
ApiException(System.Exception ex, int statusCode = 500)
```
# Options
The following properties are the options that you can set:

### Version 1.0.0
* `ApiVersion`
* `ShowApiVersion`
* `ShowStatusCode`
* `IsDebug`

### Version 1.x.0 Additions
* `IsApiOnly`
* `WrapWhenApiPathStartsWith`

These new options are added based on the feedback from [sondreb](https://github.com/sondreb). Thank you!

#### ShowApiVersion
if you want to show the `API` version in the response, then you can do:

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { ShowApiVersion = true });
```
The default `API` version format is set to "`1.0.0.0`" 

#### ApiVersion
If you wish to specify a different version format, then you can do:

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { ShowApiVersion = true, ApiVersion = "2.0" });
```

#### ShowStatusCode
if you want to show the `StatusCode` in the response, then you can do:

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { ShowStatusCode = true });
```

#### IsDebug
By default, `AutoWrapper` suppresses stack trace information. If you want to see the actual details of the error from the response during the development stage, then simply set the `AutoWrapperOptions` `IsDebug` to `true`:

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { IsDebug = true }); 
```

#### IsApiOnly
`AutoWrapper` is meant to be used for ASP.NET Core API project templates only. If you are combining `API Controllers` within your front-end projects like Angular, MVC, React, Blazor and other SPA frameworks that supports .NET Core, then use this property to enable it.

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { IsApiOnly = false} );
```

#### WrapWhenApiPathStartsWith
If you set the `IsApiOnly` option to `false`, you can also specify the segment of your `API` path for validation. By default it was set to `"/api"`. If you want to set it to something else, then you can do:

```csharp
app.UseApiResponseAndExceptionWrapper( new AutoWrapperOptions { IsApiOnly = false, WrapWhenApiPathStartsWith = "/myapi" });
```
This will activate the `AutoWrapper` to intercept HTTP responses when a request contains the `WrapWhenApiPathStartsWith` value.

> Note that I would still recommend you to implement your `API Controllers` in a seperate project to value the separation of concerns and to avoid mixing route configurations for your `SPAs` and `APIs`.

# Support for Logging

Another good thing about `AutoWrapper` is that logging is already pre-configured. .NET Core apps has built-in logging mechanism by default, and any requests and responses that has been intercepted by the wrapper will be automatically logged (thanks to Dependency Injecton!). .NET Core supports a logging `API` that works with a variety of built-in and third-party logging providers. Depending on what supported .NET Core logging provider you use and how you configure the location to log the data (e.g text file, Cloud , etc. ), AutoWrapper will automatically write the logs there for you.

# Support for Swagger
[Swagger](https://swagger.io/) provides an advance documentation for your APIs where it allows developers to reference the details of your `API` endpoints and test them when necessary. This is very helpful especially when your `API` is public and you expect many developers to use it.

`AutoWrapper` omit any request with “`/swagger`” in the `URL` so you can still be able to navigate to the Swagger UI for your API documentation.

# Samples
[AutoWrapper: Prettify Your ASP.NET Core APIs with Meaningful Responses](http://vmsdurano.com/autowrapper-prettify-your-asp-net-core-apis-with-meaningful-responses/)

# Feedback
I’m pretty sure there are still lots of things to improve in this project, so feel free to try it out and let me know your thoughts. Comments and suggestions are welcome, please drop a message and I’d be happy to answer any queries as I can.

# Contributor

* **Vincent Maverick Durano** - [Blog](http://vmsdurano.com/)

# Release History 

* 10/06/2019: AutoWrapper version `1.2.0` - refactor, cleanup and bugfixes for SPA support.
* 10/04/2019: AutoWrapper version `1.1.0` - with newly added options.
* 09/23/2019: AutoWrapper version `1.0.0` - offcial release. 
* 09/14/2019: AutoWrapper version `1.0.0-rc` - prerelease. 

# License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE) file for details
