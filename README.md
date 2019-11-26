<img align="right" src="/AutoWrapper/logo.png" />

# AutoWrapper  [![Nuget](https://img.shields.io/nuget/v/AutoWrapper.Core?color=blue)](https://www.nuget.org/packages/AutoWrapper.Core) [![Nuget downloads](https://img.shields.io/nuget/dt/AutoWrapper.Core?color=green)](https://www.nuget.org/packages/AutoWrapper.Core)

`AutoWrapper` is a simple, yet customizable global exception handler and response wrapper for ASP.NET Core APIs. It uses an ASP.NET Core `middleware` to intercept incoming `HTTP` requests and automatically wraps the responses for you by providing a consistent response format for both successful and error results. The goal is to let you focus on your business code specific requirements and let the wrapper automatically handle the `HTTP` response. This can speedup the development time when building your APIs while enforcing own standards for your `HTTP` responses.

`AutoWrapper` is a project fork based from [VMD.RESTApiResponseWrapper.Core](https://github.com/proudmonkey/RESTApiResponseWrapper.Core) which is designed to support .NET Core 2.1, 2.2, 3.x and above. The implementation of this package was refactored to provide a more convenient way to use the middleware with added flexibility.

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
* Enable property name mappings for the default `ApiResponse` properties.
* Add support for implementing your own user-defined `Response` and `Error` schema / object.
* Add support for ignoring action methods that don't need to be wrapped using `[AutoWrapIgnore]` filter attribute.
* Enable backwards compatibility support for `netcoreapp2.1` and `netcoreapp.2.2` .NET Core frameworks.

# Installation
1. Download and Install the latest `AutoWrapper.Core` from NuGet or via CLI:

```
PM> Install-Package AutoWrapper.Core -Version 2.1.0
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

The `ApiException` object contains the following overload constructors that you can use to define an exception:

```csharp
ApiException(string message, int statusCode = 500, string errorCode = "", string refLink = "")
ApiException(IEnumerable<ValidationError> errors, int statusCode = 400)
ApiException(System.Exception ex, int statusCode = 500)
ApiException(object custom, int statusCode = 400)
```
# Enable Property Mappings
If you don’t like how the default properties are named, then you can now map whatever names you want for the property using the `AutoWrapperPropertyMap` attribute. For example, let's say you want to change the name of the default `result` property to something else like `data`, then you can simply define your own schema for mapping it like in the following:

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
You can now define your own `Error` object and pass it to the `ApiException()` method. For example, if you have the following `Error` model with mapping configured:

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
If mapping wont work for you and you need to add additional attributes to the default `API` response schema, then you can now use your own custom schema/model to achieve that by setting the `UseCustomSchema` to true in `AutoWrapperOptions` as shown in the following code below:

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

    public MyCustomApiResponse(DateTime sentDate, object payload = null, string message = "", int statusCode = 200, Pagination pagination = null)
    {
        this.Code = statusCode;
        this.Message = message == string.Empty ? "Success" : message;
        this.Payload = payload;
        this.SentDate = sentDate;
        this.Pagination = pagination;
    }

    public MyCustomApiResponse(DateTime sentDate, object payload = null, Pagination pagination = null)
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
The following properties are the options that you can set:

### Version 1.0.0
* `ApiVersion`
* `ShowApiVersion`
* `ShowStatusCode`
* `IsDebug`

### Version 1.x.x Additions
* `IsApiOnly`
* `WrapWhenApiPathStartsWith`

### Version 2.0.x Additions
* `IgnoreNullValue`
* `UseCamelCaseNamingStrategy`
* `UseCustomSchema`

### Version 2.x.x Additions
* `EnableResponseLogging`
* `EnableExceptionLogging`

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
`AutoWrapper` is meant to be used for ASP.NET Core API project templates only. If you are combining `API Controllers` within your front-end projects like Angular, MVC, React, Blazor Server and other SPA frameworks that supports .NET Core, then use this property to enable it.

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { IsApiOnly = false} );
```

#### WrapWhenApiPathStartsWith
If you set the `IsApiOnly` option to `false`, you can also specify the segment of your `API` path for validation. By default it was set to `"/api"`. If you want to set it to something else, then you can do:

```csharp
app.UseApiResponseAndExceptionWrapper( new AutoWrapperOptions { 
          IsApiOnly = false, 
          WrapWhenApiPathStartsWith = "/myapi" 
});
```
This will activate the `AutoWrapper` to intercept HTTP responses when a request contains the `WrapWhenApiPathStartsWith` value.

> Note that I would still recommend you to implement your `API Controllers` in a seperate project to value the separation of concerns and to avoid mixing route configurations for your `SPAs` and `APIs`.

# AutoWrapIgnore Attribute
You can now use the `[AutoWrapIgnore]` filter attribute for enpoints that you don't need to be wrapped.

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

# Support for Logging

Another good thing about `AutoWrapper` is that logging is already pre-configured. .NET Core apps has built-in logging mechanism by default, and any requests and responses that has been intercepted by the wrapper will be automatically logged (thanks to Dependency Injecton!). .NET Core supports a logging `API` that works with a variety of built-in and third-party logging providers. Depending on what supported .NET Core logging provider you use and how you configure the location to log the data (e.g text file, Cloud , etc. ), AutoWrapper will automatically write the logs there for you.

You can turn off Logging by setting `EnableResponseLogging` and `EnableExceptionLogging` options to `false`.

For example:

```csharp
app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions {  
              EnableResponseLogging = false, 
              EnableExceptionLogging = false 
});
```

# Support for Swagger
[Swagger](https://swagger.io/) provides an advance documentation for your APIs where it allows developers to reference the details of your `API` endpoints and test them when necessary. This is very helpful especially when your `API` is public and you expect many developers to use it.

`AutoWrapper` omit any request with “`/swagger`” in the `URL` so you can still be able to navigate to the Swagger UI for your API documentation.

# Support for NetCoreApp2.1 and NetCoreApp2.2
`AutoWrapper` version 2.x also now supports both .NET Core 2.1 and 2.2. You just need to install the Nuget package `Newtonsoft.json` first before `AutoWrapper.Core`.

# Samples
* [AutoWrapper: Prettify Your ASP.NET Core APIs with Meaningful Responses](http://vmsdurano.com/autowrapper-prettify-your-asp-net-core-apis-with-meaningful-responses/)
* [AutoWrapper: Customizing the Default Response Output](http://vmsdurano.com/asp-net-core-with-autowrapper-customizing-the-default-response-output/)

# Feedback
I’m pretty sure there are still lots of things to improve in this project, so feel free to try it out and let me know your thoughts.
Feel free to request an issue on github if you find bugs or request a new feature. Your valuable feedback is much appreciated to better improve this project. If you find this useful, please give it a star to show your support for this project.

Thank you!

# Contributor

* **Vincent Maverick Durano** - [Blog](http://vmsdurano.com/)

# Release History 

* 11/09/2019: AutoWrapper version `2.1.0` - added new options and features.
* 11/05/2019: AutoWrapper version `2.0.2` - added UnAuthorize and BadRequest method response.
* 10/17/2019: AutoWrapper version `2.0.1` - added new features.
* 10/06/2019: AutoWrapper version `1.2.0` - refactor, cleanup and bugfixes for SPA support.
* 10/04/2019: AutoWrapper version `1.1.0` - with newly added options.
* 09/23/2019: AutoWrapper version `1.0.0` - offcial release. 
* 09/14/2019: AutoWrapper version `1.0.0-rc` - prerelease. 

# License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE) file for details
