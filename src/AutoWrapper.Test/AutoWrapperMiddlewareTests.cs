using AutoFixture;
using AutoWrapper.Models;
using AutoWrapper.Test.Helper;
using AutoWrapper.Wrappers;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AutoWrapper.Test
{
    public class AutoWrapperMiddlewareTests
    {
        protected readonly Fixture _fixture;
        public AutoWrapperMiddlewareTests()
        {
            _fixture = new Fixture();
        }
        [Fact(DisplayName = "DefaultTemplateNotResultData")]
        public async Task AutoWrapperDefaultTemplateNotResultData_Test()
        {
            var builder = new WebHostBuilder()
                 .ConfigureServices(services => { services.AddMvcCore(); })
                 .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();
                    app.Run(context => Task.FromResult(0));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            //var json = JsonHelper.ToJson(new ApiResponse("GET Request successful.", "", 0, null), null);
            //Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            //content.ShouldBe(json);
        }

        [Fact(DisplayName = "DefaultTemplateWithResultData")]
        public async Task AutoWrapperDefaultTemplateWithResultData_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();
                    app.Run(context => context.Response.WriteAsync("HueiFeng"));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            //Convert.ToInt32(rep.StatusCode).ShouldBe(200);
            //var json = JsonHelper.ToJson(new ApiResponse("GET Request successful.", "HueiFeng", 0, null), null);
            //content.ShouldBe(json);
        }
        [Fact(DisplayName = "CustomMessage")]
        public async Task AutoWrapperCustomMessage_Test()
        {
            var builder = new WebHostBuilder()
            .ConfigureServices(services => { services.AddMvcCore(); })
            .Configure(app =>
            {
                app.UseApiResponseAndExceptionWrapper();
                app.Run(context => context.Response.WriteAsync(
                    new ApiResponse("My Custom Message", "Test", 200).ToJson()));
            });
            var server = new TestServer(builder);

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            var response = await server.CreateClient().SendAsync(request);

            response.StatusCode.Should().Be((int)StatusCodes.Status200OK);

            var content = await response.Content.ReadAsStringAsync();

            var actual = JsonSerializer.Deserialize<ApiResponse>(content);

            actual.Message.Should().Be("My Custom Message");
            actual.Result.Should().Be("Test");
        }

        [Fact(DisplayName = "CapturingModelStateApiException")]
        public async Task AutoWrapperCapturingModelState_ApiException_Test()
        {
            var dictionary = new ModelStateDictionary();
            dictionary.AddModelError("name", "some error");
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();

                    app.Run(context => throw new ApiException(dictionary["name"]));
                });
            Exception ex;
            try
            {
                throw new ApiException(dictionary["name"]);
            }
            catch (Exception e)
            {
                ex = e;
            }
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();

        }

        [Fact(DisplayName = "CapturingModelStateApiProblemDetailsException")]
        public async Task AutoWrapperCapturingModelState_ApiProblemDetailsException_Test()
        {
            var dictionary = new ModelStateDictionary();
            dictionary.AddModelError("name", "some error");
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseApiProblemDetailsException = true });
                    app.Run(context => throw new ApiProblemDetailsException(dictionary));
                });

            var server = new TestServer(builder);

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            var response = await server.CreateClient().SendAsync(request);

            response.StatusCode.Should().Be((int)StatusCodes.Status422UnprocessableEntity);

            var content = await response.Content.ReadAsStringAsync();

            var actual = JsonSerializer.Deserialize<ApiProblemDetailsValidationErrorResponse>(content);

            actual.Detail.Should().Be("Your request parameters didn't validate.");
          
        }

        [Fact(DisplayName = "ThrowingExceptionMessageApiException")]
        public async Task AutoWrapperThrowingExceptionMessage_ApiException_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper();
                    app.Run(context => throw new ApiException("does not exist.", 404));
                });
            var server = new TestServer(builder);
            var req = new HttpRequestMessage(HttpMethod.Get, "");
            var rep = await server.CreateClient().SendAsync(req);
            var content = await rep.Content.ReadAsStringAsync();
            //Convert.ToInt32(rep.StatusCode).ShouldBe(404);
            //var ex1 = new ApiException("does not exist.", 404);
            //var json = JsonHelper.ToJson(
            //    new ApiResponse(0, new ApiError(ex1.Message) { ReferenceErrorCode = ex1.ReferenceErrorCode, ReferenceDocumentLink = ex1.ReferenceDocumentLink })
            //    , null);
            //content.ShouldBe(json);

        }

        [Fact(DisplayName = "ThrowingExceptionMessageApiProblemDetailsException")]
        public async Task AutoWrapperThrowingExceptionMessage_ApiProblemDetailsException_Test()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddMvcCore(); })
                .Configure(app =>
                {
                    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseApiProblemDetailsException = true });
                    app.Run(context => throw new ApiProblemDetailsException("does not exist.", 404));
                });
            var server = new TestServer(builder);
            var request = new HttpRequestMessage(HttpMethod.Get, "");
            var response = await server.CreateClient().SendAsync(request);

            response.StatusCode.Should().Be((int)StatusCodes.Status404NotFound);

        }
    }
}
