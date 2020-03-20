using AutoWrapper.Test.Helper;
using AutoWrapper.Test.Models;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AutoWrapper.Test
{
    public class AutoWrapperMiddlewareTests
    {

        [Fact(DisplayName = "DefaultTemplateNotResultData")]
        public async Task AutoWrapperDefaultTemplateNotResultData_Test()
        {
            var builder = new WebHostBuilder()
                 .ConfigureServices(services => { services.AddMvcCore(); })
                 .ConfigureLogging(
                     (hostingContext, logging) =>
                     {
                         logging.AddConsole();
                     })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();
                    app.Run(context => Task.FromResult(0));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            "{\"message\":\"GET Request successful.\",\"isError\":false,\"result\":\"\"}".ShouldBe(content);
        }



        [Fact(DisplayName = "DefaultTemplateWithResultData")]
        public async Task AutoWrapperDefaultTemplateWithResultData_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .ConfigureLogging(
                    (hostingContext, logging) =>
                    {
                        logging.AddConsole();
                    })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();
                    app.Run(context => context.Response.WriteAsync("HueiFeng"));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            "{\"message\":\"GET Request successful.\",\"isError\":false,\"result\":\"HueiFeng\"}".ShouldBe(content);
        }
        [Fact(DisplayName = "CustomMessage")]
        public async Task AutoWrapperCustomMessage_Test()
        {
            var builder = new WebHostBuilder()
            .ConfigureServices(services => { services.AddMvcCore(); })
            .ConfigureLogging(
                (hostingContext, logging) =>
                {
                    logging.AddConsole();
                })
            .Configure(app =>
            {
                app.UseApiResponseAndExceptionWrapper();
                app.Run(context => context.Response.WriteAsync(
                    new ApiResponse("customMessage.", "Test", 200).ToJson()));
            });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            "{\"message\":\"customMessage.\",\"isError\":false,\"result\":\"Test\"}".ShouldBe(content);
        }

        [Fact(DisplayName = "CapturingModelStateApiException")]
        public async Task AutoWrapperCapturingModelState_ApiException_Test()
        {
            var dictionary = new ModelStateDictionary();
            dictionary.AddModelError("name", "some error");
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .ConfigureLogging(
                    (hostingContext, logging) =>
                    {
                        logging.AddConsole();
                    })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();


                    app.Run(context => throw new ApiException(dictionary["name"]));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(400);
            var str = "{\"isError\":true,\"responseException\":{\"key\":\"name\",\"subKey\":{\"buffer\":\"name\",\"offset\":0,\"length\":4,\"value\":\"name\",\"hasValue\":true},\"isContainerNode\":false,\"errors\":[{\"errorMessage\":\"some error\"}],\"validationState\":\"Invalid\"}}";
            str.ShouldBe(content);
        }
        [Fact(DisplayName = "CapturingModelStateApiProblemDetailsException")]
        public async Task AutoWrapperCapturingModelState_ApiProblemDetailsException_Test()
        {
            var dictionary = new ModelStateDictionary();
            dictionary.AddModelError("name", "some error");
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .ConfigureLogging(
                    (hostingContext, logging) =>
                    {
                        logging.AddConsole();
                    })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseApiProblemDetailsException = true });


                    app.Run(context => throw new ApiProblemDetailsException(dictionary));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(422);
            var str = "{\"isError\":true,\"errors\":null,\"validationErrors\":[{\"name\":\"name\",\"reason\":\"some error\"}],\"details\":null,\"type\":\"https://httpstatuses.com/422\",\"title\":\"Unprocessable Entity\",\"status\":422,\"detail\":\"Your request parameters didn't validate.\",\"instance\":\"/\"}";
            str.ShouldBe(content);
        }

        [Fact(DisplayName = "ThrowingExceptionMessageApiException")]
        public async Task AutoWrapperThrowingExceptionMessage_ApiException_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .ConfigureLogging(
                    (hostingContext, logging) =>
                    {
                        logging.AddConsole();
                    })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();
                    app.Run(context => throw new ApiException("does not exist.", 404));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(404);
            var str = "{\"isError\":true,\"responseException\":{\"exceptionMessage\":\"does not exist.\"}}";
            str.ShouldBe(content);
        }

        [Fact(DisplayName = "ThrowingExceptionMessageApiProblemDetailsException")]
        public async Task AutoWrapperThrowingExceptionMessage_ApiProblemDetailsException_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .ConfigureLogging(
                    (hostingContext, logging) =>
                    {
                        logging.AddConsole();
                    })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseApiProblemDetailsException = true });
                    app.Run(context => throw new ApiProblemDetailsException("does not exist.", 404));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(404);
            var str = "{\"isError\":true,\"errors\":null,\"validationErrors\":null,\"details\":null,\"type\":\"https://httpstatuses.com/404\",\"title\":\"does not exist.\",\"status\":404,\"detail\":null,\"instance\":\"/\"}";
            str.ShouldBe(content);
        }

        [Fact(DisplayName = "ModelValidations")]
        public async Task AutoWrapperModelValidations_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.Configure<ApiBehaviorOptions>(options =>
                    {
                        options.SuppressModelStateInvalidFilter = true;
                    });
                    services.AddMvcCore();
                })
                .ConfigureLogging(
                    (hostingContext, logging) =>
                    {
                        logging.AddConsole();
                    })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper<MapResponseObject>();
                    app.Run(context => context.Response.WriteAsync(
                        new ApiResponse("customMessage.", "Test", 200).ToJson()));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            var str = "{\"message\":\"customMessage.\",\"isError\":false,\"data\":\"Test\"}";
            str.ShouldBe(content);
        }

        [Fact(DisplayName = "CustomErrorObject")]
        public async Task AutoWrapperCustomErrorObject_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddMvcCore();
                })
                .ConfigureLogging(
                    (hostingContext, logging) =>
                    {
                        logging.AddConsole();
                    })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper<MapResponseCustomErrorObject>();
                    app.Run(context =>
                 throw new ApiException(
                        new Error("An error blah.", "InvalidRange",
                            new InnerError("12345678", "2020-03-20")
                        )));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(400);
            var str = "{\"isError\":true,\"error\":{\"message\":\"An error blah.\",\"code\":\"InvalidRange\",\"innerError\":{\"requestId\":\"12345678\",\"date\":\"2020-03-20\"}}}";
            str.ShouldBe(content);
        }


        [Fact(DisplayName = "CustomResponse")]
        public async Task AutoWrapperCustomResponse_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddMvcCore();
                })
                .ConfigureLogging(
                    (hostingContext, logging) =>
                    {
                        logging.AddConsole();
                    })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseCustomSchema = true });
                    app.Run(context => context.Response.WriteAsync(new MyCustomApiResponse("Mr.A").ToJson())); 
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            var str = "{\"Code\":200,\"Payload\":\"Mr.A\",\"SentDate\":\"0001-01-01 00:00:00\"}";
            str.ShouldBe(content);
        }

    }
}
