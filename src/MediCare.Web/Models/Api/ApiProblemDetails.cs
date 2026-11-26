using System.Collections.Generic;

namespace MediCare.Web.Models.Api;

// The JSON error that every API error returns
public class ApiProblemDetails
{
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? TraceId { get; set; }

    // Only for field validation errors (400): field name -> messages
    public Dictionary<string, string[]>? Errors { get; set; }
}