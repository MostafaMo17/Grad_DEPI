using System;
using System.Collections.Generic;
using MediCare.Web.Models.Api;

namespace MediCare.Web.Services;

// Thrown by the API client when the API answers with an error
// Message is safe to show to the user
public class ApiException : Exception
{
    public ApiException(int statusCode, string message, ApiProblemDetails? problem = null) : base(message)
    {
        StatusCode = statusCode;
        Problem = problem;
    }

    public int StatusCode { get; }
    public ApiProblemDetails? Problem { get; }

    public string? TraceId
    {
        get { return Problem?.TraceId; }
    }

    // Field validation messages (field name -> messages), or null
    public Dictionary<string, string[]>? Errors
    {
        get { return Problem?.Errors; }
    }
}