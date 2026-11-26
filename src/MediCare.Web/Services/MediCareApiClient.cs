using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MediCare.Web.Models.Api;

namespace MediCare.Web.Services;

// JSON shapes used by the doctor-schedule endpoints.
public class ApiLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ApiUser
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? DoctorApprovalStatus { get; set; }
}

public class ApiLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public ApiUser User { get; set; } = new();
}

public class ApiScheduleDay
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotLengthMinutes { get; set; } = 30;
}

public class ApiDoctorSchedule
{
    public int ScheduleId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotLengthMinutes { get; set; }
    public bool IsAvailable { get; set; }
}

public class ApiSaveScheduleRequest
{
    public List<ApiScheduleDay> Days { get; set; } = new();
}

public class MediCareApiClient : IMediCareApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;

    public MediCareApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Legacy login shape used by the doctor-schedule module.
    public async Task<ApiLoginResponse> LoginAsync(string email, string password)
    {
        ApiLoginRequest request = new ApiLoginRequest
        {
            Email = email,
            Password = password
        };

        return await SendAsync<ApiLoginResponse>(HttpMethod.Post, "auth/login", request);
    }

    // Doctor schedules.
    public async Task<List<ApiDoctorSchedule>> GetMySchedulesAsync(string token)
    {
        _ = token;
        return await SendAsync<List<ApiDoctorSchedule>>(HttpMethod.Get, "doctor/schedules");
    }

    public async Task<List<ApiDoctorSchedule>> SaveMySchedulesAsync(
        string token,
        List<ApiScheduleDay> days)
    {
        _ = token;
        ApiSaveScheduleRequest request = new ApiSaveScheduleRequest { Days = days };
        return await SendAsync<List<ApiDoctorSchedule>>(HttpMethod.Put, "doctor/schedules", request);
    }

    // Authentication.
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        return await SendAsync<LoginResponseDto>(HttpMethod.Post, "auth/login", request);
    }

    public async Task<LoginResponseDto> RegisterPatientAsync(RegisterPatientRequestDto request)
    {
        return await SendAsync<LoginResponseDto>(HttpMethod.Post, "auth/register/patient", request);
    }

    public async Task<LoginResponseDto> RegisterDoctorAsync(RegisterDoctorRequestDto request)
    {
        return await SendAsync<LoginResponseDto>(HttpMethod.Post, "auth/register/doctor", request);
    }

    public async Task<UserDto> GetMeAsync()
    {
        return await SendAsync<UserDto>(HttpMethod.Get, "auth/me");
    }

    // Notifications.
    public async Task<PagedResultDto<NotificationDto>> GetNotificationsAsync(
        bool unreadOnly,
        int page,
        int pageSize)
    {
        string unread = unreadOnly ? "true" : "false";
        string url = "notifications/me?unreadOnly=" + unread
            + "&page=" + page
            + "&pageSize=" + pageSize;

        return await SendAsync<PagedResultDto<NotificationDto>>(HttpMethod.Get, url);
    }

    public async Task<int> GetUnreadCountAsync()
    {
        UnreadCountDto result = await SendAsync<UnreadCountDto>(HttpMethod.Get, "notifications/unread-count");
        return result.Count;
    }

    public async Task MarkNotificationAsReadAsync(int notificationId)
    {
        await SendAsync(HttpMethod.Patch, "notifications/" + notificationId + "/read");
    }

    public async Task MarkAllNotificationsAsReadAsync()
    {
        await SendAsync(HttpMethod.Post, "notifications/read-all");
    }

    public async Task<HubTokenDto> GetHubTokenAsync()
    {
        return await SendAsync<HubTokenDto>(HttpMethod.Post, "notifications/hub-token");
    }

    // Sends a request and reads the JSON answer.
    private async Task<T> SendAsync<T>(HttpMethod method, string url, object? body = null)
    {
        using HttpResponseMessage response = await SendRequestAsync(method, url, body);

        T? result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        if (result == null)
        {
            throw new ApiException(500, "The server returned an empty answer.");
        }

        return result;
    }

    // Sends a request when no response body is needed.
    private async Task SendAsync(HttpMethod method, string url, object? body = null)
    {
        using HttpResponseMessage response = await SendRequestAsync(method, url, body);
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpMethod method,
        string url,
        object? body)
    {
        using HttpRequestMessage request = new HttpRequestMessage(method, url);
        if (body != null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, body.GetType(), JsonOptions),
                Encoding.UTF8,
                "application/json");
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            throw new ApiException(503, "The server is not reachable right now. Please try again later.");
        }

        if (!response.IsSuccessStatusCode)
        {
            ApiException exception = await CreateExceptionAsync(response);
            response.Dispose();
            throw exception;
        }

        return response;
    }

    private static async Task<ApiException> CreateExceptionAsync(HttpResponseMessage response)
    {
        int statusCode = (int)response.StatusCode;
        ApiProblemDetails? problem = null;

        try
        {
            problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(JsonOptions);
        }
        catch (JsonException)
        {
            // The body was not valid JSON; use the default message below.
        }
        catch (NotSupportedException)
        {
            // The body was not JSON at all, for example an empty response.
        }

        string message = problem?.Detail ?? problem?.Title ?? GetDefaultMessage(statusCode);
        return new ApiException(statusCode, message, problem);
    }

    private static string GetDefaultMessage(int statusCode)
    {
        return statusCode switch
        {
            400 => "The request is not valid.",
            401 => "Please log in again.",
            403 => "You do not have permission to do this.",
            404 => "Not found.",
            409 => "This conflicts with existing data.",
            423 => "Too many failed attempts. Please wait a few minutes.",
            429 => "Too many requests. Please wait a minute and try again.",
            _ => "A server error occurred. Please try again later."
        };
    }
}
