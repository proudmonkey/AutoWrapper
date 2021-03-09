<img align="right" src="/src/AutoWrapper/logo.png" />

# AutoWrapper  [![Nuget](https://img.shields.io/nuget/v/AutoWrapper.Core?color=blue)](https://www.nuget.org/packages/AutoWrapper.Core) [![Nuget downloads](https://img.shields.io/nuget/dt/AutoWrapper.Core?color=green)](https://www.nuget.org/packages/AutoWrapper.Core) ![.NET Core](https://github.com/proudmonkey/AutoWrapper/workflows/.NET%20Core/badge.svg)

Language: English | [中文](README.zh-cn.md)  

`AutoWrapper` is a simple, yet customizable global `HTTP` exception handler and response wrapper for ASP.NET Core APIs. It uses an ASP.NET Core `middleware` to intercept incoming `HTTP` requests and automatically wraps the responses for you by providing a consistent response format for both successful and error results. The goal is to let you focus on your business code specific requirements and let the wrapper automatically handle the `HTTP` response. This can speedup the development time when building your APIs while enforcing own standards for your `HTTP` responses.

#### Main features:

* Exception handling.
* `ModelState` validation error handling (support both `Data Annotation` and `FluentValidation`).
* A configurable `API` exception.
* A consistent response format for `Result` and `Errors`.
* A detailed `Result` response.
* A detailed `Error` response.
* A configurable `HTTP` `StatusCodes` and messages.
* Add support for `Swagger`.
* Add Logging support for `Request`, `Response` and `Exceptions`.
* A configurable middleware `options` to configure the wrapper. See **Options** section below for details.
* Enable property name mappings for the default `ApiResponse` properties.
* Add support for implementing your own user-defined `Response` and `Error` schema / object.
* Add support for Problem Details exception format.
* Add support for ignoring action methods that don't need to be wrapped using `[AutoWrapIgnore]` filter attribute.
* V3.x enable backwards compatibility support for `netcoreapp2.1` and `netcoreapp2.2` .NET Core frameworks.
* Add `ExcludePaths` option to enable support for `SignalR` and `dapr` routes。

# Installation
1. Download and Install the latest `AutoWrapper.Core` from NuGet or via CLI:

```
PM> Install-Package AutoWrapper.Core -Version 4.3.0
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

# Defining Your Own Custom Message
To display a custom message in your response, use the `ApiResponse` object from `AutoWrapper.Wrappers` namespace. For example, if you want to display a message when a successful `POST` has been made, then you can do something like this:

```csharp
[HttpPost]
public async Task<ApiResponse> Post([FromBody]CreatePersonRequest createRequest)  
{
    try
    {
        var personId = await _personManager.CreateAsync(createRequest);
        return new ApiResponse("New record has been created in the database.", personId, Status201Created);
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
    "message": "New record has been created in the database.",
    "result": 100
}
```
The `ApiResponse` object has the following overload constructors that you can use:

```csharp
ApiResponse(string message, object result = null, int statusCode = 200, string apiVersion = "1.0.0.0")
ApiResponse(object result, int statusCode = 200)
ApiResponse(int statusCode, object apiError)
ApiResponse()
```

# Defining Your Own Api Exception
`AutoWrapper` provides two flavors that you can use to define your own custom exception:

* `ApiException` - default
* `ApiProblemDetailsException` - available only in version 4 and up. 

Here are a few examples for throwing your own exception message.

#### Capturing ModelState Validation Errors

```csharp
if (!ModelState.IsValid)
{
    throw new ApiException(ModelState.AllErrors());
}
```

The format of the exception result would look something like this when validation fails:

```json
{
    "isError": true,
    "responseException": {
        "exceptionMessage": "Request responded with one or more validation errors occurred.",
        "validationErrors": [
            {
                "name": "LastName",
                "reason": "'Last Name' must not be empty."
            },
            {
                "name": "FirstName",
                "reason": "'First Name' must not be empty."
            },
            {
                "name": "DateOfBirth",
                "reason": "'Date Of Birth' must not be empty."
            }
        ]
    }
}
```

To use Problem Details as an error format, just set `UseApiProblemDetailsException` to `true`:

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseApiProblemDetailsException = true }); 
```

Then you can use the `ApiProblemDetailsException` object like in the following:

```csharp
if (!ModelState.IsValid)
{
    throw new ApiProblemDetailsException(ModelState);
}

```

The format of the exception result would now look something like this when validation fails:

```json
{
    "isError": true,
    "type": "https://httpstatuses.com/422",
    "title": "Unprocessable Entity",
    "status": 422,
    "detail": "Your request parameters didn't validate.",
    "instance": null,
    "extensions": {},
    "validationErrors": [
        {
            "name": "LastName",
            "reason": "'Last Name' must not be empty."
        },
        {
            "name": "FirstName",
            "reason": "'First Name' must not be empty."
        },
        {
            "name": "DateOfBirth",
            "reason": "'Date Of Birth' must not be empty."
        }
    ]
}
```

You can see how the `validationErrors` property is automatically populated with the violated `name` from your model.

#### Throwing Your Own Exception Message

An example using `ApiException`:

```csharp
throw new ApiException($"Record with id: {id} does not exist.", Status404NotFound);
```

The result would look something like this:

```json
{
    "isError": true,
    "responseException": {
        "exceptionMessage": "Record with id: 1001 does not exist.",
    }
}
```

An example using `ApiProblemDetailsException`:

```csharp
throw new ApiProblemDetailsException($"Record with id: {id} does not exist.", Status404NotFound);  
```
The result would look something like this:

```json
{
    "isError": true,
    "type": "https://httpstatuses.com/404",
    "title": "Record with id: 1001 does not exist.",
    "status": 404,
    "detail": null,
    "instance": null,
    "extensions": {}
}
```

The `ApiException` object contains the following overload constructors that you can use to define an exception:

```csharp
ApiException(string message, int statusCode = 400, string errorCode = "", string refLink = "")
ApiException(IEnumerable<ValidationError> errors, int statusCode = 400)
ApiException(System.Exception ex, int statusCode = 500)
ApiException(object custom, int statusCode = 400)
```

The `ApiProblemDetailsException` object contains the following overload constructors that you can use to define an exception:

```csharp
ApiProblemDetailsException(int statusCode)
ApiProblemDetailsException(string title, int statusCode)
ApiProblemDetailsException(ProblemDetails details)
ApiProblemDetailsException(ModelStateDictionary modelState, int statusCode = Status422UnprocessableEntity)
```

For more information, checkout the links below at the **Samples** section.

# Implement Model Validations
`Model` validations allows you to enforce pre-defined validation rules at a `class`/`property` level. You'd normally use this validation technique to keep a clear separation of concerns, so your validation code becomes much simpler to write, maintain, and test.

As you have already known, starting ASP.NET Core 2.1, it introduced the `ApiController` attribute which performs automatic model state validation for `400 Bad Request` error. When the `Controller` is decorated with `ApiController` attribute, the framework will automatically register a `ModelStateInvalidFilter` which runs on the `OnActionExecuting` event. This checks for the `Model State` validity and returns the response accordingly. This is a great feature, but since we want to return a custom response object instead of the `400 Bad Request` error, we will disable this feature in our case.

To disable the automatic model state validation, just add the following code at `ConfigureServices()` method in `Startup.cs` file:

```csharp
public void ConfigureServices(IServiceCollection services) {  
    services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

}
```

# Enable Property Mappings

> Note: Property Mappings is not available for Problem Details attributes.

Use the `AutoWrapperPropertyMap` attribute to map the AutoWrapper default property to something else. For example, let's say you want to change the name of the `result` property to something else like `data`, then you can simply define your own schema for mapping it like in the following:

```csharp
public class MapResponseObject  
{
    [AutoWrapperPropertyMap(Prop.Result)]
    public object Data { get; set; }
}
```
You can then pass the `MapResponseObject` class to the `AutoWrapper` middleware like this:

```csharp
app.UseApiResponseAndExceptionWrapper<MapResponseObject>();  
```

On successful requests, your response should now look something like this after mapping:

```json
{
    "message": "Request successful.",
    "isError": false,
    "data": {
        "id": 7002,
        "firstName": "Vianne",
        "lastName": "Durano",
        "dateOfBirth": "2018-11-01T00:00:00"
    }
}
```
Notice that the default `result` attribute is now replaced with the `data` attribute.

Keep in mind that you are free to choose whatever property that you want to map. Here is the list of default properties that you can map:

```csharp
[AutoWrapperPropertyMap(Prop.Version)]
[AutoWrapperPropertyMap(Prop.StatusCode)]
[AutoWrapperPropertyMap(Prop.Message)]
[AutoWrapperPropertyMap(Prop.IsError)]
[AutoWrapperPropertyMap(Prop.Result)]
[AutoWrapperPropertyMap(Prop.ResponseException)]
[AutoWrapperPropertyMap(Prop.ResponseException_ExceptionMessage)]
[AutoWrapperPropertyMap(Prop.ResponseException_Details)]
[AutoWrapperPropertyMap(Prop.ResponseException_ReferenceErrorCode)]
[AutoWrapperPropertyMap(Prop.ResponseException_ReferenceDocumentLink)]
[AutoWrapperPropertyMap(Prop.ResponseException_ValidationErrors)]
[AutoWrapperPropertyMap(Prop.ResponseException_ValidationErrors_Field)]
[AutoWrapperPropertyMap(Prop.ResponseException_ValidationErrors_Message)]
```

# Using Your Own Error Schema
You can define your own `Error` object and pass it to the `ApiException()` method. For example, if you have the following `Error` model with mapping configured:

```csharp
public class MapResponseObject  
{
    [AutoWrapperPropertyMap(Prop.ResponseException)]
    public object Error { get; set; }
}

public class Error  
{
    public string Message { get; set; }

    public string Code { get; set; }
    public InnerError InnerError { get; set; }

    public Error(string message, string code, InnerError inner)
    {
        this.Message = message;
        this.Code = code;
        this.InnerError = inner;
    }

}

public class InnerError  
{
    public string RequestId { get; set; }
    public string Date { get; set; }

    public InnerError(string reqId, string reqDate)
    {
        this.RequestId = reqId;
        this.Date = reqDate;
    }
}
```
You can then throw an error like this:

```csharp
throw new ApiException(  
      new Error("An error blah.", "InvalidRange",
      new InnerError("12345678", DateTime.Now.ToShortDateString())
));
```

The format of the output will now look like this:

```json
{
    "isError": true,
    "error": {
        "message": "An error blah.",
        "code": "InvalidRange",
        "innerError": {
            "requestId": "12345678",
            "date": "10/16/2019"
        }
    }
}
```
# Using Your Own API Response Schema
If mapping wont work for you and you need to add additional attributes to the default `API` response schema, then you can use your own custom schema/model to achieve that by setting the `UseCustomSchema` to true in `AutoWrapperOptions` as shown in the following code below:

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseCustomSchema = true }); 
```

Now let's say for example you wanted to have an attribute `SentDate` and `Pagination` object as part of your main `API` response, you might want to define your `API` response schema to something like this:

```csharp
public class MyCustomApiResponse  
{
    public int Code { get; set; }
    public string Message { get; set; }
    public object Payload { get; set; }
    public DateTime SentDate { get; set; }
    public Pagination Pagination { get; set; }

    public MyCustomApiResponse(DateTime sentDate, 
                               object payload = null, 
                               string message = "", 
                               int statusCode = 200, 
                               Pagination pagination = null)
    {
        this.Code = statusCode;
        this.Message = message == string.Empty ? "Success" : message;
        this.Payload = payload;
        this.SentDate = sentDate;
        this.Pagination = pagination;
    }

    public MyCustomApiResponse(DateTime sentDate, 
                               object payload = null, 
                               Pagination pagination = null)
    {
        this.Code = 200;
        this.Message = "Success";
        this.Payload = payload;
        this.SentDate = sentDate;
        this.Pagination = pagination;
    }

    public MyCustomApiResponse(object payload)
    {
        this.Code = 200;
        this.Payload = payload;
    }

}

public class Pagination  
{
    public int TotalItemsCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
```

To test the result, you can create a `GET` method to something like this:

```csharp
public async Task<MyCustomApiResponse> Get()  
{
    var data = await _personManager.GetAllAsync();

    return new MyCustomApiResponse(DateTime.UtcNow, data,
        new Pagination
        {
            CurrentPage = 1,
            PageSize = 10,
            TotalItemsCount = 200,
            TotalPages = 20
        });
}
```

Running the code should give you now the following response format:

```
{
    "code": 200,
    "message": "Success",
    "payload": [
        {
            "id": 1,
            "firstName": "Vianne Maverich",
            "lastName": "Durano",
            "dateOfBirth": "2018-11-01T00:00:00"
        },
        {
            "id": 2,
            "firstName": "Vynn Markus",
            "lastName": "Durano",
            "dateOfBirth": "2018-11-01T00:00:00"
        },
        {
            "id": 3,
            "firstName": "Mitch",
            "lastName": "Durano",
            "dateOfBirth": "2018-11-01T00:00:00"
        }
    ],
    "sentDate": "2019-10-17T02:26:32.5242353Z",
    "pagination": {
        "totalItemsCount": 200,
        "pageSize": 10,
        "currentPage": 1,
        "totalPages": 20
    }
}
```
That’s it. One thing to note here is that once you use your own schema for your `API` response, you have the full ability to control how you would want to format your data, but at the same time losing some of the option configurations for the default `API` Response. The good thing is you can still take advantage of the `ApiException()` method to throw a user-defined error message.

# Options
The following properties are the available options that you can set:

### Version 4.5.x Additions
* `ExcludePaths`

### Version 4.3.x Additions
* `ShouldLogRequestData`
* `ShowIsErrorFlagForSuccessfulResponse`

### Version 4.2.x Additions
* `IgnoreWrapForOkRequests`

### Version 4.1.0 Additions
* `LogRequestDataOnException`

### Version 4.0.0 Additions
* `UseApiProblemDetailsException`
* `UseCustomExceptionFormat`

### Version 3.0.0 Additions
* `BypassHTMLValidation `
* `ReferenceLoopHandling `

### Version 2.x.x Additions
* `EnableResponseLogging`
* `EnableExceptionLogging`

### Version 2.0.x Additions
* `IgnoreNullValue`
* `UseCamelCaseNamingStrategy`
* `UseCustomSchema`

### Version 1.x.x Additions
* `IsApiOnly`
* `WrapWhenApiPathStartsWith`

### Version 1.0.0
* `ApiVersion`
* `ShowApiVersion`
* `ShowStatusCode`
* `IsDebug`

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
`AutoWrapper` is meant to be used for ASP.NET Core API project templates only. If you are combining `API Controllers` within your front-end projects like Angular, MVC, React, Blazor Server and other SPA frameworks that supports .NET Core, then use this property to enable it.

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { IsApiOnly = false} );
```

#### WrapWhenApiPathStartsWith
If you set the `IsApiOnly` option to `false`, you can also specify the segment of your `API` path for validation. By default it is set to `"/api"`. If you want to set it to something else, then you can do:

```csharp
app.UseApiResponseAndExceptionWrapper( new AutoWrapperOptions { 
          IsApiOnly = false, 
          WrapWhenApiPathStartsWith = "/myapi" 
});
```
This will activate the `AutoWrapper` to intercept HTTP responses when a request contains the `WrapWhenApiPathStartsWith` value.

> Note that I would still recommend you to implement your `API Controllers` in a separate project to value the separation of concerns and to avoid mixing route configurations for your `SPAs` and `APIs`.

#### IgnoreWrapForOkRequests
If you want to completely ignore wrapping the response for successful requests to just output directly the data, you simply set the IgnoreWrapForOkRequests to true like in the following:

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions {  
    IgnoreWrapForOkRequests = true,
});
```

# AutoWrapIgnore Attribute
You can use the `[AutoWrapIgnore]` filter attribute for endpoints that you don't need to be wrapped.

For example:

```csharp
[HttpGet]
[AutoWrapIgnore]
public async Task<IActionResult> Get()  
{
    var data = await _personManager.GetAllAsync();
    return Ok(data);
}
```
or

```csharp
[HttpGet]
[AutoWrapIgnore]
public async Task<IEnumerable<Person>> Get()  
{
    return await _personManager.GetAllAsync();
}
```

# RequestDataLogIgnore Attribute
You can use the `[RequestDataLogIgnore]` if you don't want certain endpoints to log the data in the requests:

```csharp
[HttpGet]
[RequestDataLogIgnore]
public async Task<ApiResponse> Post([FromBody] CreatePersonRequest personRequest)  
{
    //Rest of the code here
}
```

You can use the `[AutoWrapIgnore]` attribute and set `ShouldLogRequestData` property to `false` if you have an endpoint that don't need to be wrapped and also don't want to log the data in the requests:

```csharp
[HttpGet]
[AutoWrapIgnore(ShouldLogRequestData = false)]
public async Task<IEnumerable<PersonResponse>> Get()  
{
     //Rest of the code here
}
```
# Support for Logging

Another good thing about `AutoWrapper` is that logging is already pre-configured. .NET Core apps has built-in logging mechanism by default, and any requests and responses that has been intercepted by the wrapper will be automatically logged (thanks to Dependency Injection!). .NET Core supports a logging `API` that works with a variety of built-in and third-party logging providers. Depending on what supported .NET Core logging provider you use and how you configure the location to log the data (e.g text file, Cloud , etc. ), AutoWrapper will automatically write the logs there for you.

You can turn off the default Logging by setting `EnableResponseLogging` and `EnableExceptionLogging` options to `false`.

For example:

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions {  
    EnableResponseLogging = false, 
    EnableExceptionLogging = false 
});
```

You can set the `LogRequestDataOnException` option to `false` if you want to exclude the request body data in the logs when an exception occurs.

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions {  
    LogRequestDataOnException = false 
});
```

# Support for Swagger
[Swagger](https://swagger.io/) provides an advance documentation for your APIs where it allows developers to reference the details of your `API` endpoints and test them when necessary. This is very helpful especially when your `API` is public and you expect many developers to use it.

`AutoWrapper` omit any request with “`/swagger`” in the `URL` so you can still be able to navigate to the Swagger UI for your API documentation.

# Exclude Paths
The ExcludePaths option enables you to set a collection of API paths to be ignored. This feature was added by chen1tian. Thank you so much for this great contribution! Here's how it works:

Excluding Api paths/routes that do not need to be wrapped support three ExcludeMode:

`Strict`: The request path must be exactly the same as the configured path.
`StartWith`: The request path starts at the configuration path.
`Regex`: If the requested path match the configured path regular expression, it will be excluded.
The following is a quick example:

# Support for SignalR
If you have the following SignalR endpoint：

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<NoticeHub>("/notice");
});
```

then you can use the ExcludePaths and set the the "/notice" path as AutoWrapperExcludePaths for the SignalR endpoint to work. For example:

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
{
    ExcludePaths = new AutoWrapperExcludePaths[] {
        new AutoWrapperExcludePaths("/notice/.*|/notice", ExcludeMode.Regex)            
    }
});
```

# Support for Dapr
Prior to `4.5.x` version, the Dapr Pubsub request cannot reach the configured Controller Action after being wrapped by AutoWrapper. The series path starting with "/dapr/" needs to be excluded to make the dapr request take effect:

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
{
    ExcludePaths = new AutoWrapperExcludePaths[] {
        new AutoWrapperExcludePaths("/dapr", ExcludeMode.StartWith)          
    }
});
```

# Support for NetCoreApp2.1 and NetCoreApp2.2
`AutoWrapper` version 2.x - 3.0 also supports both .NET Core 2.1 and 2.2. You just need to install the Nuget package `Newtonsoft.json` first before `AutoWrapper.Core`.

# Unwrapping the Result from .NET Client 
[AutoWrapper.Server](https://github.com/proudmonkey/AutoWrapper.Server) is simple library that enables you unwrap the `Result` property of the AutoWrapper's `ApiResponse` object in your C# .NET Client code. The goal is to deserialize the `Result` object directly to your matching `Model` without having you to create the ApiResponse schema. 

For example:

```csharp
[HttpGet]
public async Task<IEnumerable<PersonDTO>> Get()
{
    var client = HttpClientFactory.Create();
    var httpResponse = await client.GetAsync("https://localhost:5001/api/v1/persons");

    IEnumerable<PersonDTO> persons = null;
    if (httpResponse.IsSuccessStatusCode)
    {
        var jsonString = await httpResponse.Content.ReadAsStringAsync();
        persons = Unwrapper.Unwrap<IEnumerable<PersonDTO>>(jsonString);
    }

    return persons;
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
* **chen1tian** - [Github Profile](https://github.com/chen1tian)

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
