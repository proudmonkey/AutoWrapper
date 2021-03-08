<img align="right" src="/src/AutoWrapper/logo.png" />

# AutoWrapper  [![Nuget](https://img.shields.io/nuget/v/AutoWrapper.Core?color=blue)](https://www.nuget.org/packages/AutoWrapper.Core) [![Nuget downloads](https://img.shields.io/nuget/dt/AutoWrapper.Core?color=green)](https://www.nuget.org/packages/AutoWrapper.Core) ![.NET Core](https://github.com/proudmonkey/AutoWrapper/workflows/.NET%20Core/badge.svg)

`AutoWrapper`是一个简单，可自定义的全局`HTTP`异常处理程序和针对ASP.NET Core API的响应包装器。它使用ASP.NET Core `middleware` 来拦截传入的`HTTP`请求，并通过为成功和错误结果提供一致的响应格式来自动为您包装响应。目的是让您专注于特定于业务代码的要求，并让包装器自动处理`HTTP`响应。这可以加快构建API的开发时间，同时为`HTTP`响应强制执行自己的标准。

  #### 主要特点：

  * 异常处理
  * `ModelState` 验证错误机制 (同时支持 `Data Annotation` 和 `FluentValidation`)
  * 可配置`API`异常
  * `Result`和Errors一致性响应格式
  * 详细的`Result`响应
  * 详细的`Error`响应
  * 可配置`HTTP` `StatusCodes`和消息
  * 添加对`Swagger`的支持
  * 添加对`Request` `Response` 和`Exceptions`的日志支持
  * 一个可配置的中间件`选项`来配置包装器。有关详情，请参见下面**选项**部分。
  * 为默认`ApiResponse`属性启动属性名称映射
  * 添加支持以实现您自己的用户定义的`Response`和`Error` schema / object
  * 添加对问题详细信息的异常格式的支持
  * 添加对忽略不需要使用`[AutoWrapIgnore]`过滤器属性包装的操作方法的支持。
  * V3.x启用了对`netcoreapp2.1`和`netcoreapp2.2` .NET Core框架的向后兼容性支持
  * 增加排除的路径，依赖于此，增加了对`SignalR`的支持,同时也能支持`Dapr`方法。

  # 安装

  1. AutoWrapper.Core从NuGet或通过CLI下载并安装：

     ```
     PM> Install-Package AutoWrapper.Core -Version 4.3.0
     ```

  2. 在`Startup.cs`下声明命名空间

     ```csharp
     using AutoWrapper;
     ```

  3. 在`UseRouting()` 中间件之前的`Startup.cs`的`Configure()`方法中注册以下中间件：

     ```csharp
     app.UseApiResponseAndExceptionWrapper();
     ```

  很简单！默认的ASP.NET Core API模板`WeatherForecastController` API的响应如下所示：

  ```json
  {
      "message": "GET Request successful.",
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

  # 定义自己的自定义消息

  要在响应中显示自定义消息，请使用`AutoWrapper.Wrappers`名称空间中的`ApiResponse`对象。例如，如果要在成功执行`POST`后显示一条消息，则可以执行以下操作：

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

  成功运行代码将为您提供以下结果：

  ```json
  {
      "message": "New record has been created in the database.",
      "isError": false,
      "result": 100
  }
  ```

  `ApiResponse`对象具有以下可使用的重载构造函数：

  ```csharp
  ApiResponse(string message, object result = null, int statusCode = 200, string apiVersion = "1.0.0.0")
  ApiResponse(object result, int statusCode = 200)
  ApiResponse(int statusCode, object apiError)
  ApiResponse()
  ```

  # 定义自己的Api异常

  `AutoWrapper`提供了两种类型，您可以用来定义自己的自定义异常：

  * `ApiException`-默认

  * `ApiProblemDetailsException`-仅在 version 4及更高版本中可用

  这里是一些抛出您自己的异常消息的示例。

  #### 捕获ModelState验证错误

  ```csharp
  if (!ModelState.IsValid)
  {
      throw new ApiException(ModelState.AllErrors());
  }
  ```

  验证失败时，异常结果的格式如下所示：

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

  要将问题详细信息用作错误格式，只需将`UseApiProblemDetailsException`设置为`true`即可：

  ```csharp
  app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseApiProblemDetailsException = true }); 
  ```

  然后，您可以像下面这样使用`ApiProblemDetailsException`对象：

  ```csharp
  if (!ModelState.IsValid)
  {
      throw new ApiProblemDetailsException(ModelState);
  }
  
  ```

  验证失败时，异常结果的格式现在应类似于以下内容：

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

  您可以看到如何使用模型中的`名称`自动填充`validationErrors`属性。

  #### 抛出自己的异常消息

  一个使用`ApiException`的例子：

  ```csharp
  throw new ApiException($"Record with id: {id} does not exist.", Status404NotFound);
  ```

  结果看起来像这样：

  ```json
  {
      "isError": true,
      "responseException": {
          "exceptionMessage": "Record with id: 1001 does not exist.",
      }
  }
  ```

  一个使用`ApiProblemDetailsException`的例子：

  ```csharp
  throw new ApiProblemDetailsException($"Record with id: {id} does not exist.", Status404NotFound);  
  ```

  结果看起来像这样：

  ```csharp
  throw new ApiProblemDetailsException($"Record with id: {id} does not exist.", Status404NotFound);  
  ```

  结果看起来像这样：

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

  `ApiException`对象包含以下重载构造函数，可用于定义异常：

  ```csharp
  ApiException(string message, int statusCode = 400, string errorCode = "", string refLink = "")
  ApiException(IEnumerable<ValidationError> errors, int statusCode = 400)
  ApiException(System.Exception ex, int statusCode = 500)
  ApiException(object custom, int statusCode = 400)
  ```

  `ApiProblemDetailsException`对象包含以下重载构造函数，可用于定义异常：

  ```csharp
  ApiProblemDetailsException(int statusCode)
  ApiProblemDetailsException(string title, int statusCode)
  ApiProblemDetailsException(ProblemDetails details)
  ApiProblemDetailsException(ModelStateDictionary modelState, int statusCode = Status422UnprocessableEntity)
  ```

  有关更多信息，请在下面的**示例**部分中查看链接。

  # 实施Model验证

  `Model`验证可让您在`class` /`property`级别强制执行预定义的验证规则。通常，您将使用此验证技术来保持关注点的清晰分离，因此验证代码的编写，维护和测试变得更加简单。

  众所周知，从ASP.NET Core 2.1开始，它引入了`ApiController`属性，该属性针对`400 Bad Request`错误执行自动模型状态验证。当Controller用ApiController属性修饰时，框架将自动注册一个ModelStateInvalidFilter，它在OnActionExecuting事件上运行。这将检查`模型状态`的有效性，并相应地返回响应。这是一个很棒的功能，但是由于我们要返回自定义响应对象而不是`400 Bad Request`错误，因此我们将禁用此功能。

  要禁用自动模型状态验证，只需在`Startup.cs`文件中的`ConfigureServices()`方法中添加以下代码：

  ```csharp
  public void ConfigureServices(IServiceCollection services) {  
      services.Configure<ApiBehaviorOptions>(options =>
      {
          options.SuppressModelStateInvalidFilter = true;
      });
  
  }
  ```

  # 启用属性映射

  > Note: Property Mappings is not available for Problem Details attributes.

  使用`AutoWrapperPropertyMap`属性将AutoWrapper的默认属性映射到其他属性。举例来说，假设您想将`result`属性的名称更改为诸如`data`之类的名称，然后只需定义自己的架构即可进行映射，如下所示：

  ```csharp
  public class MapResponseObject  
  {
      [AutoWrapperPropertyMap(Prop.Result)]
      public object Data { get; set; }
  }
  ```
  然后可以将`apResponseObject`类传递给`AutoWrapper`中间件，如下所示：

  ```csharp
  app.UseApiResponseAndExceptionWrapper<MapResponseObject>();  
  ```

  成功请求后，映射后，您的响应现在应如下所示：

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

  注意，默认的`result`属性现在已被`data`属性取代。

  请记住，您可以自由选择要映射的任何属性。这是可以映射的默认属性的列表：

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

  # 使用自己的错误架构

  您可以定义自己的`Error`对象，并将其传递给`ApiException()`方法。例如，如果您具有配置了映射的以下`Error`模型：

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

  然后，您可以引发如下错误：

  ```csharp
  throw new ApiException(  
        new Error("An error blah.", "InvalidRange",
        new InnerError("12345678", DateTime.Now.ToShortDateString())
  ));
  ```

  现在，输出格式将如下所示：

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

  # 使用您自己的API响应架构

  如果映射对您不起作用，并且您需要向默认的API响应模式中添加其他属性，则可以通过在AutoWrapperOptions中将UseCustomSchema设置为true来使用自己的自定义模式/模型来实现，如图所示。下面的代码：

  ```csharp
  app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseCustomSchema = true }); 
  ```

  现在，假设您想将属性`SentDate`和` Pagination`对象作为主要API响应的一部分，您可能希望将API响应架构定义为以下形式：

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

  为了测试结果，您可以为以下内容创建一个`GET`方法：

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

  运行代码现在应该为您提供以下响应格式：

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

  而已。这里要注意的一件事是，一旦您对API响应使用了自己的模式，就可以完全控制想要格式化数据的方式，但是同时会丢失一些默认的选项配置。 API响应。好消息是，您仍然可以利用`ApiException()`方法抛出用户定义的错误消息。

  # 选项

  以下属性是可以设置的可用选项：
  
  ### 版本4.3.x添加
  * `ShouldLogRequestData`
  * `ShowIsErrorFlagForSuccessfulResponse`
  
  ### 版本4.2.x添加
  * `IgnoreWrapForOkRequests`

  ### 版本4.1.0添加
  * `LogRequestDataOnException`

  ### 版本4.0.0添加
  * `UseApiProblemDetailsException`
  * `UseCustomExceptionFormat`

  ### 版本3.0.0添加
  * `BypassHTMLValidation`
  * `ReferenceLoopHandling`

  ### 版本2.x.x添加
  * `EnableResponseLogging`
  * `EnableExceptionLogging`

  ### 版本2.0.x添加
  * `IgnoreNullValue`
  * `UseCamelCaseNamingStrategy`
  * `UseCustomSchema`

  ### 版本1.x.x添加
  * `IsApiOnly`
  * `WrapWhenApiPathStartsWith`

  ### 版本1.0.0
  * `ApiVersion`
  * `ShowApiVersion`
  * `ShowStatusCode`
  * `IsDebug`

  #### ShowApiVersion
  如果您想在响应中显示API版本，则可以执行以下操作：

  ```csharp
  app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { ShowApiVersion = true });
  ```

  API的默认版本格式设置为`1.0.0.0`

  #### ApiVersion
  如果您希望指定其他版本格式，则可以执行以下操作：

  ```csharp
  app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { ShowApiVersion = true, ApiVersion = "2.0" });
  ```

  #### ShowStatusCode
  如果您想在响应中显示`StatusCode`，则可以执行以下操作：

  ```csharp
  app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { ShowStatusCode = true });
  ```

  #### IsDebug
  默认情况下，`AutoWrapper`禁止显示堆栈跟踪信息。如果要在开发阶段从响应中查看错误的实际详细信息，只需将AutoWrapperOptions`IsDebug`设置为true：

  ```csharp
  app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { IsDebug = true }); 
  ```

  #### IsApiOnly

  `AutoWrapper`仅可用于ASP.NET Core API项目模板。如果您要将`API Controllers`组合到您的前端项目（例如Angular，MVC，React，Blazor Server和其他支持.NET Core的SPA框架）中，请使用此属性将其启用。

  ```csharp
  app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { IsApiOnly = false} );
  ```

  #### WrapWhenApiPathStartsWith


  如果将`IsApiOnly`选项设置为`false`，则还可以指定 API路径的分段以进行验证。默认情况下，它设置为`/ api`。如果要将其设置为其他内容，则可以执行以下操作：

  ```csharp
  app.UseApiResponseAndExceptionWrapper( new AutoWrapperOptions { 
            IsApiOnly = false, 
            WrapWhenApiPathStartsWith = "/myapi" 
  });
  ```
  当请求包含`WrapWhenApiPathStartsWith`值时，这将激活`AutoWrapper`以拦截HTTP响应。


  >请注意，我仍然建议您在单独的项目中实现` API Controllers`，以重视关注点的分离，并避免将SPA和API的路由配置混合在一起。

  # AutoWrapIgnore Attribute


  您可以使用`[AutoWrapIgnore]`过滤器属性来指定不需要包装的点。

  例如：


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

  如果您不希望某些端点在请求中记录数据，则可以使用`[RequestDataLogIgnore]`

  ```csharp
  [HttpGet]
  [RequestDataLogIgnore]
  public async Task<ApiResponse> Post([FromBody] CreatePersonRequest personRequest)  
  {
      //Rest of the code here
  }
  ```


  如果您有不需要包装的端点并且也不想在请求中记录数据，则可以使用`[AutoWrapIgnore]`属性并`将ShouldLogRequestData`属性设置为`false`。

  ```csharp
  [HttpGet]
  [AutoWrapIgnore(ShouldLogRequestData = false)]
  public async Task<IEnumerable<PersonResponse>> Get()  
  {
       //Rest of the code here
  }
  ```

  # Support for Swagger
  [Swagger](https://swagger.io/) 提供API的高级文档，使开发人员可以引用API端点的详细信息并在必要时进行测试。这非常有用，特别是当您的`API`是公开的并且您希望许多开发人员使用它时。

  `AutoWrapper` 
  省略网址中带有`/swagger`的任何请求，因此您仍然可以导航到Swagger UI以获得API文档。

  # Exclude Paths
  排除不需要包装的Api路径，支持三种排除方式：

    - 严格：请求路径与配置的路径必须完全一致才排除。
    - 起始于：请求路径开始于配置路径，便会被排除。
    - 正则：请求路径满足配置路径正则表达式的话，便会被排除。

  ```csharp
  app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
  {
      ShowIsErrorFlagForSuccessfulResponse = true,
      ExcludePaths = new AutoWrapperExcludePaths[] {
          // 严格匹配
          new AutoWrapperExcludePaths("/Strict", ExcludeMode.Strict),
          // 匹配起始于路径
          new AutoWrapperExcludePaths("/dapr", ExcludeMode.StartWith),
          // 正则匹配
          new AutoWrapperExcludePaths("/notice/.*|/notice", ExcludeMode.Regex)          
      }
  });
  ```


  # Support for SignalR
  如果你有一个SigalR终结的，例如：
  ```csharp
  app.UseEndpoints(endpoints =>
  {
      endpoints.MapControllers();
      endpoints.MapHub<NoticeHub>("/notice");
  });
  ```
  那么可以使用ExcludePaths排除它，以便让SignalR终结点生效
  ```csharp
    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
    {
        ShowIsErrorFlagForSuccessfulResponse = true,
        ExcludePaths = new AutoWrapperExcludePaths[] {
            new AutoWrapperExcludePaths("/notice/.*|/notice", ExcludeMode.Regex)            
        }
    });
  ```

  # Support for Dapr
  Dapr Pubsub请求被AutoWrapper包装后无法到达配置的Controller Action，需要排除`/dapr/`起始的系列路径，使dapr请求生效：
  ```csharp
    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
    {
        ShowIsErrorFlagForSuccessfulResponse = true,
        ExcludePaths = new AutoWrapperExcludePaths[] {
            new AutoWrapperExcludePaths("/dapr", ExcludeMode.StartWith)          
        }
    });
  ```
  


  # Support for NetCoreApp2.1 and NetCoreApp2.2
  `AutoWrapper` 
  2.x-3.0版还支持.NET Core 2.1和2.2。您只需要先在`AutoWrapper.Core`之前安装Nuget包`Newtonsoft.json`即可。

  # Unwrapping the Result from .NET Client 
  [AutoWrapper.Server](https://github.com/proudmonkey/AutoWrapper.Server) i
  是一个简单的库，使您可以在C＃.NET客户端代码中解开AutoWrapper的`ApiResponse`对象的`Result`属性。目的是将结果对象直接反序列化为匹配的模型，而无需创建`ApiResponse`模式。

  例如:

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


  有关更多信息，请参见： [AutoWrapper.Server](https://github.com/proudmonkey/AutoWrapper.Server)
  # Samples
  * [AutoWrapper: Prettify Your ASP.NET Core APIs with Meaningful Responses](http://vmsdurano.com/autowrapper-prettify-your-asp-net-core-apis-with-meaningful-responses/)
  * [AutoWrapper: Customizing the Default Response Output](http://vmsdurano.com/asp-net-core-with-autowrapper-customizing-the-default-response-output/)
  * [AutoWrapper Now Supports Problem Details For Your ASP.NET Core APIs](http://vmsdurano.com/autowrapper-now-supports-problemdetails/)
  * [AutoWrapper.Server: Sample Usage](http://vmsdurano.com/autowrapper-server-is-now-available/)

# 反馈并给予好评！:star:
  我敢肯定，这个项目还有很多事情需要改进。试试看，让我知道您的想法。

  如果发现错误或要求新功能，请随时提交[ticket](https://github.com/proudmonkey/AutoWrapper/issues)。非常感谢您宝贵的反馈意见，以更好地改进该项目。如果您觉得此功能有用，请给它加星号，以表示您对该项目的支持。

  # 贡献者

  * **Vincent Maverick Durano** - [Blog](http://vmsdurano.com/)
  * **Huei Feng** - [Github Profile](https://github.com/hueifeng)
  * **ITninja04** - [Github Profile](https://github.com/ITninja04)
  * **Rahmat Slamet** - [Github Profile](https://github.com/arhen)
  * **abelfiore** - [Github Profile](https://github.com/abelfiore)
  * **chen1tian** - [Github Profile](https://github.com/chen1tian)

  想要贡献？请阅读贡献文档 [here](https://github.com/proudmonkey/AutoWrapper/blob/master/CONTRIBUTING.md).

  # Release History 

  See: [Release Log](https://github.com/proudmonkey/AutoWrapper/blob/master/RELEASE.MD)

  # License

  该项目已获得MIT许可证的许可-有关详细信息，请参见[LICENSE.md](LICENSE)文件。

  # Donate

  如果您觉得这个项目有用-或只是感到宽容，请考虑向我购买啤酒或咖啡。干杯! :beers: :coffee:
  |                                                              |                                                              |
  | ------------------------------------------------------------ | :----------------------------------------------------------: |
  | <a href="https://www.paypal.me/vmsdurano"><img src="https://github.com/proudmonkey/Resources/blob/master/donate_paypal.svg" height="40"></a> | [![BMC](https://github.com/proudmonkey/Resources/blob/master/donate_coffee.png)](https://www.buymeacoffee.com/ImP9gONBW) |


  谢谢！
