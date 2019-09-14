# AutoWrapper

The AutoWrapper is a global exception handler and response wrapper for ASP.NET Core APIs. It uses a middleware to capture exceptions and to capture HTTP response to build a consistent response object for both successful and error requests.

## Prerequisite

Install Newtonsoft.Json package

## Installing

Below are the steps to use the AutoWrapper middleware into your ASP.NET Core app:

1) Declare the following namespace within Startup.cs

using AutoWrapper;

2) Register the middleware below within the Configure() method of Startup.cs

  app.UseApiResponseAndExceptionWrapper();

  The default version format is "1.0.0.0". If you wish to specify the version for your API, then you can do:

  app.UseApiResponseAndExceptionWrapper(new ApiResponseOptions { ApiVersion = "2.0" });

Note: Make sure to register it "before" the UseRouting() middleware

3) Done. 


Once you have configured the middleware, you can then start using the Api

## Sample Output 

The following are examples of response output:

Here's the format for successful request with data:

```
{
    
    "version": "1.0.0.0",
    "statusCode": 200,
    "message": "Request successful.",
    "isError": false,
	"result": [
		"value1",
		"value2"
	]

}
  
```

Here's the format for successful request without data:

```
{
    
	"version": "1.0.0.0",
    "statusCode": 201,
    "message": "Object has been created.",
    "isError": false,

}
```

Here's the format for error request with validation errors:

```
{
    
	"version": "1.0.0.0",
    "statusCode": 400,
    "isError": true,
	"responseException": {
		"ExceptionMessage": "Validation Field Error.",
		"Details": null,
		"ReferenceErrorCode": null,
        
		"ReferenceDocumentLink": null,
        
		"ValidationErrors": [
            
			{
                
				"Field": "LastName",
                
				"Message": "'Last Name' should not be empty."
            
			},
            
			{
                
				"Field": "FirstName",
                
				"Message": "'First Name' should not be empty."
            
			}
        ]
    
	}

}
``` 

Here's the format for error request

```
{
    "version": "1.0.0.0",
    "statusCode": 400,
    "isError": true,
    "responseException": {
        "exceptionMessage": "Request responded with validation error(s). Please correct the specified validation errors and try again.",
        "details": null,
        "referenceErrorCode": null,
        "referenceDocumentLink": null,
        "validationErrors": [
            {
                "field": "LastName",
                "message": "'Last Name' must not be empty."
            },
            {
                "field": "FirstName",
                "message": "'First Name' must not be empty."
            }
        ]
    }
}
```  
          
 

## Using Custom Exception

This library isn't just a middleware, it also provides some objects that you can use for defining your own exception. For example, if you want to throw your own exception message, you could simply do:

```
//for capturing ModelState validation errors
throw new ApiException(ModelState.AllErrors());

//for throwing your own exception message
throw new ApiException($"Record with id: {id} does not exist.", 400);
```

The ApiException object contains the following three overload constructors that you can use to define an exception:

```
ApiException(string message, int statusCode = 500, string errorCode = "", string refLink = "")

ApiException(IEnumerable<ValidationError> errors, int statusCode = 400)

ApiException(System.Exception ex, int statusCode = 500)

```


## Defining Your Own Response Object

Aside from throwing your own custom exception, You could also return your own custom defined JSON Response  by using the ApiResponse object in your API controller. For example:

```
return new ApiResponse("Created Successfully.", true, 201);
```

The APIResponse has the following parameters:

```
ApiResponse(string message, object result = null, int statusCode = 200, string apiVersion = "1.0.0.0")
```

## Source Code

The source code for this can be found at: (https://github.com/proudmonkey/RESTApiResponseWrapper.Core) 
 

## Author

* **Vincent Maverick Durano** - [Blog](http://vmsdurano.com/)


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

