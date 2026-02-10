using Edi.AspNetCore.Utils.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace Edi.AspNetCore.Utils.Tests.Filters;

public class ProblemDetailsResultFilterTests
{
    private readonly Mock<ProblemDetailsFactory> _mockProblemDetailsFactory;
    private readonly ProblemDetailsResultFilter _filter;
    private readonly DefaultHttpContext _httpContext;
    private readonly ResultExecutingContext _resultExecutingContext;

    public ProblemDetailsResultFilterTests()
    {
        _mockProblemDetailsFactory = new Mock<ProblemDetailsFactory>();
        _filter = new ProblemDetailsResultFilter(_mockProblemDetailsFactory.Object);
        _httpContext = new DefaultHttpContext();
        _httpContext.TraceIdentifier = "test-trace-id";
        _httpContext.Request.Path = "/test/path";

        var actionContext = new ActionContext(
            _httpContext,
            new RouteData(),
            new ActionDescriptor());

        _resultExecutingContext = new ResultExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new ObjectResult(null),
            new object());
    }

    [Fact]
    public void OnResultExecuting_WithObjectResult404AndStringValue_ConvertsToProblemDetails()
    {
        // Arrange
        var errorMessage = "Resource not found";
        var objectResult = new ObjectResult(errorMessage) { StatusCode = 404 };
        _resultExecutingContext.Result = objectResult;

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 404,
            Detail = errorMessage,
            Instance = "/test/path"
        };

        _mockProblemDetailsFactory
            .Setup(x => x.CreateProblemDetails(
                _httpContext,
                404,
                null,
                null,
                errorMessage,
                "/test/path"))
            .Returns(expectedProblemDetails);

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(404, problemDetails.Status);
        Assert.Equal(errorMessage, problemDetails.Detail);
        Assert.Equal("/test/path", problemDetails.Instance);
        Assert.Equal("test-trace-id", problemDetails.Extensions["traceId"]);
        Assert.Contains("application/problem+json", result.ContentTypes);
        Assert.Single(result.ContentTypes);
    }

    [Fact]
    public void OnResultExecuting_WithObjectResult400AndStringValue_ConvertsToProblemDetails()
    {
        // Arrange
        var errorMessage = "Invalid request";
        var objectResult = new ObjectResult(errorMessage) { StatusCode = 400 };
        _resultExecutingContext.Result = objectResult;

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 400,
            Detail = errorMessage,
            Instance = "/test/path"
        };

        _mockProblemDetailsFactory
            .Setup(x => x.CreateProblemDetails(
                _httpContext,
                400,
                null,
                null,
                errorMessage,
                "/test/path"))
            .Returns(expectedProblemDetails);

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal(errorMessage, problemDetails.Detail);
        Assert.Equal("test-trace-id", problemDetails.Extensions["traceId"]);
        Assert.Contains("application/problem+json", result.ContentTypes);
    }

    [Fact]
    public void OnResultExecuting_WithObjectResult500AndStringValue_ConvertsToProblemDetails()
    {
        // Arrange
        var errorMessage = "Internal server error";
        var objectResult = new ObjectResult(errorMessage) { StatusCode = 500 };
        _resultExecutingContext.Result = objectResult;

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 500,
            Detail = errorMessage,
            Instance = "/test/path"
        };

        _mockProblemDetailsFactory
            .Setup(x => x.CreateProblemDetails(
                _httpContext,
                500,
                null,
                null,
                errorMessage,
                "/test/path"))
            .Returns(expectedProblemDetails);

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(500, problemDetails.Status);
        Assert.Equal(errorMessage, problemDetails.Detail);
        Assert.Equal("test-trace-id", problemDetails.Extensions["traceId"]);
        Assert.Contains("application/problem+json", result.ContentTypes);
    }

    [Fact]
    public void OnResultExecuting_WithObjectResult409AndStringValue_ConvertsToProblemDetails()
    {
        // Arrange
        var errorMessage = "Conflict occurred";
        var objectResult = new ObjectResult(errorMessage) { StatusCode = 409 };
        _resultExecutingContext.Result = objectResult;

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 409,
            Detail = errorMessage,
            Instance = "/test/path"
        };

        _mockProblemDetailsFactory
            .Setup(x => x.CreateProblemDetails(
                _httpContext,
                409,
                null,
                null,
                errorMessage,
                "/test/path"))
            .Returns(expectedProblemDetails);

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(409, problemDetails.Status);
        Assert.Equal(errorMessage, problemDetails.Detail);
        Assert.Equal("test-trace-id", problemDetails.Extensions["traceId"]);
        Assert.Contains("application/problem+json", result.ContentTypes);
    }

    [Fact]
    public void OnResultExecuting_WithObjectResult404AndNullValue_ConvertsToProblemDetails()
    {
        // Arrange
        var objectResult = new ObjectResult(null) { StatusCode = 404 };
        _resultExecutingContext.Result = objectResult;

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 404,
            Detail = null,
            Instance = "/test/path"
        };

        _mockProblemDetailsFactory
            .Setup(x => x.CreateProblemDetails(
                _httpContext,
                404,
                null,
                null,
                null,
                "/test/path"))
            .Returns(expectedProblemDetails);

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(404, problemDetails.Status);
        Assert.Null(problemDetails.Detail);
        Assert.Equal("test-trace-id", problemDetails.Extensions["traceId"]);
        Assert.Contains("application/problem+json", result.ContentTypes);
    }

    [Fact]
    public void OnResultExecuting_WithObjectResult404AndNonStringValue_ConvertsToProblemDetails()
    {
        // Arrange
        var customObject = new { ErrorCode = "ERR001", Message = "Not found" };
        var objectResult = new ObjectResult(customObject) { StatusCode = 404 };
        _resultExecutingContext.Result = objectResult;

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 404,
            Detail = null,
            Instance = "/test/path"
        };

        _mockProblemDetailsFactory
            .Setup(x => x.CreateProblemDetails(
                _httpContext,
                404,
                null,
                null,
                null,
                "/test/path"))
            .Returns(expectedProblemDetails);

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("test-trace-id", problemDetails.Extensions["traceId"]);
        Assert.Contains("application/problem+json", result.ContentTypes);
    }

    [Fact]
    public void OnResultExecuting_WithObjectResultAlreadyProblemDetails_DoesNotConvert()
    {
        // Arrange
        var existingProblemDetails = new ProblemDetails
        {
            Status = 404,
            Detail = "Already a problem details",
            Title = "Not Found"
        };
        var objectResult = new ObjectResult(existingProblemDetails) { StatusCode = 404 };
        _resultExecutingContext.Result = objectResult;

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(existingProblemDetails, problemDetails);
        Assert.Equal("Already a problem details", problemDetails.Detail);
        Assert.Equal("Not Found", problemDetails.Title);
        // Should not add traceId since it's already a ProblemDetails
        Assert.DoesNotContain("traceId", problemDetails.Extensions.Keys);
    }

    [Fact]
    public void OnResultExecuting_WithObjectResult200_DoesNotConvert()
    {
        // Arrange
        var successMessage = "Success";
        var objectResult = new ObjectResult(successMessage) { StatusCode = 200 };
        _resultExecutingContext.Result = objectResult;

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        Assert.IsType<string>(result.Value);
        Assert.Equal(successMessage, result.Value);
        Assert.DoesNotContain("application/problem+json", result.ContentTypes);
    }

    [Fact]
    public void OnResultExecuting_WithObjectResult399_DoesNotConvert()
    {
        // Arrange
        var message = "Some message";
        var objectResult = new ObjectResult(message) { StatusCode = 399 };
        _resultExecutingContext.Result = objectResult;

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        Assert.IsType<string>(result.Value);
        Assert.Equal(message, result.Value);
    }

    [Fact]
    public void OnResultExecuting_WithObjectResultNoStatusCode_DoesNotConvert()
    {
        // Arrange
        var message = "Some message";
        var objectResult = new ObjectResult(message);
        _resultExecutingContext.Result = objectResult;

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        Assert.IsType<string>(result.Value);
        Assert.Equal(message, result.Value);
    }

    [Fact]
    public void OnResultExecuting_WithNonObjectResult_DoesNotConvert()
    {
        // Arrange
        var viewResult = new ViewResult();
        _resultExecutingContext.Result = viewResult;

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        Assert.IsType<ViewResult>(_resultExecutingContext.Result);
    }

    [Fact]
    public void OnResultExecuting_WithObjectResult_ClearsExistingContentTypes()
    {
        // Arrange
        var errorMessage = "Error";
        var objectResult = new ObjectResult(errorMessage) { StatusCode = 400 };
        objectResult.ContentTypes.Add("application/json");
        objectResult.ContentTypes.Add("text/plain");
        _resultExecutingContext.Result = objectResult;

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 400,
            Detail = errorMessage,
            Instance = "/test/path"
        };

        _mockProblemDetailsFactory
            .Setup(x => x.CreateProblemDetails(
                _httpContext,
                400,
                null,
                null,
                errorMessage,
                "/test/path"))
            .Returns(expectedProblemDetails);

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        Assert.Single(result.ContentTypes);
        Assert.Contains("application/problem+json", result.ContentTypes);
        Assert.DoesNotContain("application/json", result.ContentTypes);
        Assert.DoesNotContain("text/plain", result.ContentTypes);
    }

    [Fact]
    public void OnResultExecuting_AddsTraceIdToExtensions()
    {
        // Arrange
        _httpContext.TraceIdentifier = "custom-trace-12345";
        var errorMessage = "Error";
        var objectResult = new ObjectResult(errorMessage) { StatusCode = 500 };
        _resultExecutingContext.Result = objectResult;

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 500,
            Detail = errorMessage,
            Instance = "/test/path"
        };

        _mockProblemDetailsFactory
            .Setup(x => x.CreateProblemDetails(
                _httpContext,
                500,
                null,
                null,
                errorMessage,
                "/test/path"))
            .Returns(expectedProblemDetails);

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("custom-trace-12345", problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public void OnResultExecuted_DoesNothing()
    {
        // Arrange
        var resultExecutedContext = new ResultExecutedContext(
            new ActionContext(
                _httpContext,
                new RouteData(),
                new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new ObjectResult(null),
            new object());

        // Act & Assert - should not throw
        _filter.OnResultExecuted(resultExecutedContext);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(409)]
    [InlineData(422)]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    public void OnResultExecuting_WithVariousErrorStatusCodes_ConvertsToProblemDetails(int statusCode)
    {
        // Arrange
        var errorMessage = $"Error with status {statusCode}";
        var objectResult = new ObjectResult(errorMessage) { StatusCode = statusCode };
        _resultExecutingContext.Result = objectResult;

        var expectedProblemDetails = new ProblemDetails
        {
            Status = statusCode,
            Detail = errorMessage,
            Instance = "/test/path"
        };

        _mockProblemDetailsFactory
            .Setup(x => x.CreateProblemDetails(
                _httpContext,
                statusCode,
                null,
                null,
                errorMessage,
                "/test/path"))
            .Returns(expectedProblemDetails);

        // Act
        _filter.OnResultExecuting(_resultExecutingContext);

        // Assert
        var result = Assert.IsType<ObjectResult>(_resultExecutingContext.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(statusCode, problemDetails.Status);
        Assert.Equal(errorMessage, problemDetails.Detail);
        Assert.Equal("test-trace-id", problemDetails.Extensions["traceId"]);
        Assert.Contains("application/problem+json", result.ContentTypes);
    }
}
