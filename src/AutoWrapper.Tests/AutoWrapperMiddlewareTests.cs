using AutoFixture;
using AutoWrapper.Constants;
using AutoWrapper.Models;
using AutoWrapper.Tests.Extensions;
using AutoWrapper.Tests.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AutoWrapper.Tests
{
    public static class WebHostBuilderExtension
    {
        public static IWebHostBuilder ConfigureAutoWrapper(this IWebHostBuilder webHostBuilder, RequestDelegate requestDelegate)
        {
            return webHostBuilder
                 .ConfigureServices(services => 
                 {
                     services.AddControllers();
                 })
                 .Configure(app =>
                 {
                     app.UseAutoWrapper();
                     app.Run(requestDelegate);
                 });
        }
    }

    public class AutoWrapperMiddlewareTests
    {
        protected readonly Fixture _fixture;

        private readonly JsonSerializerOptions _jsonSerializerOptionsDefault = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            IgnoreNullValues = true,
            WriteIndented = true
        };

        public AutoWrapperMiddlewareTests()
        {
            _fixture = new Fixture();
        }

        private IWebHostBuilder ConfigureAutoWrapper(IWebHostBuilder webHostBuilder, RequestDelegate requestDelegate)
        {
            return webHostBuilder
                 .ConfigureServices(services => { services.AddControllers(); })
                 .Configure(app =>
                 {
                     app.UseAutoWrapper();
                     app.Run(requestDelegate);
                 });
        }

        private ApiResponse ApiResponseBuilder(string message, object result, bool showStatusCode = false, int statusCode = 200)
        {
            if (showStatusCode)
            {
                return _fixture.Build<ApiResponse>()
                    .With(p => p.IsError, false)
                    .With(p => p.StatusCode, statusCode)
                    .With(p => p.Message, message)
                    .With(p => p.Result, result)
                    .Create();
            }

            return _fixture.Build<ApiResponse>()
                .With(p => p.IsError, false)
                .Without(p => p.StatusCode)
                .With(p => p.Message, message)
                .With(p => p.Result, result)
                .Create();
        }

        [Fact]
        public async Task WhenResult_IsEmpty_Returns_200()
        {

            var webhostBuilder = new WebHostBuilder()
                .ConfigureAutoWrapper(context => Task.CompletedTask);

            var server = new TestServer(webhostBuilder);

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            var response = await server.CreateClient().SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            var apiResponse = ApiResponseBuilder($"{request.Method} {ResponseMessage.Success}", "");

            var expectedJson = apiResponse.ToJson(_jsonSerializerOptionsDefault);

            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            content.Should().Be(expectedJson);
        }

        [Fact]
        public async Task WhenResult_HasData_Returns_200()
        {
            var testData = "HueiFeng";

            var webhostBuilder = new WebHostBuilder()
                .ConfigureAutoWrapper(context => context.Response.WriteAsync(testData));

            var server = new TestServer(webhostBuilder);

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            var response = await server.CreateClient().SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            var apiResponse = ApiResponseBuilder($"{request.Method} {ResponseMessage.Success}", testData);

            var expectedJson = apiResponse.ToJson(_jsonSerializerOptionsDefault);

            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            content.Should().Be(expectedJson);
        }

        [Fact]
        public async Task WhenMessage_MatchesCustomValue_Returns200()
        {
            var expectedJson = new ApiResponse("My Custom Message").ToJson(_jsonSerializerOptionsDefault);

            var webhostBuilder = new WebHostBuilder()
               .ConfigureAutoWrapper(context => context.Response.WriteAsync(expectedJson));
 
            var server = new TestServer(webhostBuilder);

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            var response = await server.CreateClient().SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            content.Should().Be(expectedJson);
        }


        [Fact]
        public async Task WhenCustomModel_MatchesTheResponse_Returns200()
        {
            var testData = new TestModel
            {
                Id = Guid.NewGuid(),
                FirstName = _fixture.Create<string>(),
                LastName = _fixture.Create<string>(),
                DateOfBirth = _fixture.Create<DateTime>(),
            };

            var webhostBuilder = new WebHostBuilder()
               .ConfigureAutoWrapper(context => context.Response.WriteAsync(testData.ToJson(_jsonSerializerOptionsDefault)));

            var server = new TestServer(webhostBuilder);

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            var response = await server.CreateClient().SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            var apiResponse = ApiResponseBuilder($"{request.Method} {ResponseMessage.Success}", testData);
            var expectedJson = apiResponse.ToJson(_jsonSerializerOptionsDefault);

            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            content.Should().Be(expectedJson);
        }

        //[Fact(DisplayName = "CapturingModelStateApiException")]
        //public async Task AutoWrapperCapturingModelState_ApiException_Test()
        //{
        //    var dictionary = new ModelStateDictionary();
        //    dictionary.AddModelError("name", "some error");
        //    var builder = new WebHostBuilder()
        //        .ConfigureServices(services => { services.AddMvcCore(); })
        //        .Configure(app =>
        //        {
        //            app.UseAutoWrapper();

        //            app.Run(context => throw new ApiException(dictionary["name"]));
        //        });
        //    Exception ex;
        //    try
        //    {
        //        throw new ApiException(dictionary["name"]);
        //    }
        //    catch (Exception e)
        //    {
        //        ex = e;
        //    }
        //    var server = new TestServer(builder);
        //    var req = new HttpRequestMessage(HttpMethod.Get, "");
        //    var rep = await server.CreateClient().SendAsync(req);
        //    var content = await rep.Content.ReadAsStringAsync();

        //}

        //[Fact(DisplayName = "CapturingModelStateApiProblemDetailsException")]
        //public async Task AutoWrapperCapturingModelState_ApiProblemDetailsException_Test()
        //{
        //    var dictionary = new ModelStateDictionary();
        //    dictionary.AddModelError("name", "some error");
        //    var builder = new WebHostBuilder()
        //        .ConfigureServices(services => { services.AddMvcCore(); })
        //        .Configure(app =>
        //        {
        //            app.UseAutoWrapper(new AutoWrapperOptions { UseApiProblemDetailsException = true });
        //            app.Run(context => throw new ApiProblemDetailsException(dictionary));
        //        });

        //    var server = new TestServer(builder);

        //    var request = new HttpRequestMessage(HttpMethod.Get, "");
        //    var response = await server.CreateClient().SendAsync(request);

        //    response.StatusCode.Should().Be((int)StatusCodes.Status422UnprocessableEntity);

        //    var content = await response.Content.ReadAsStringAsync();

        //    var actual = JsonSerializer.Deserialize<ApiProblemDetailsValidationErrorResponse>(content);

        //    actual.Detail.Should().Be("Your request parameters didn't validate.");
          
        //}

        //[Fact(DisplayName = "ThrowingExceptionMessageApiException")]
        //public async Task AutoWrapperThrowingExceptionMessage_ApiException_Test()
        //{
        //    var builder = new WebHostBuilder()
        //        .ConfigureServices(services => { services.AddMvcCore(); })
        //        .Configure(app =>
        //        {
        //            app.UseAutoWrapper();
        //            app.Run(context => throw new ApiException("does not exist.", 404));
        //        });
        //    var server = new TestServer(builder);
        //    var req = new HttpRequestMessage(HttpMethod.Get, "");
        //    var rep = await server.CreateClient().SendAsync(req);
        //    var content = await rep.Content.ReadAsStringAsync();
        //    //Convert.ToInt32(rep.StatusCode).ShouldBe(404);
        //    //var ex1 = new ApiException("does not exist.", 404);
        //    //var json = JsonHelper.ToJson(
        //    //    new ApiResponse(0, new ApiError(ex1.Message) { ReferenceErrorCode = ex1.ReferenceErrorCode, ReferenceDocumentLink = ex1.ReferenceDocumentLink })
        //    //    , null);
        //    //content.ShouldBe(json);

        //}

        //[Fact(DisplayName = "ThrowingExceptionMessageApiProblemDetailsException")]
        //public async Task AutoWrapperThrowingExceptionMessage_ApiProblemDetailsException_Test()
        //{
        //    var builder = new WebHostBuilder()
        //        .ConfigureServices(services => { services.AddMvcCore(); })
        //        .Configure(app =>
        //        {
        //            app.UseAutoWrapper(new AutoWrapperOptions { UseApiProblemDetailsException = true });
        //            app.Run(context => throw new ApiProblemDetailsException("does not exist.", 404));
        //        });
        //    var server = new TestServer(builder);
        //    var request = new HttpRequestMessage(HttpMethod.Get, "");
        //    var response = await server.CreateClient().SendAsync(request);

        //    response.StatusCode.Should().Be((int)StatusCodes.Status404NotFound);
        //}
    }
}
