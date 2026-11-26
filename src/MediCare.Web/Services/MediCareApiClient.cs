using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediCare.Web.Services;

// Thrown when the API answers with an error status (400, 401, 403, ...).
public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}

// JSON shapes of MediCare.API (kept here so the Web project stays independent).
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

public class MediCareApiClient
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

    public async Task<ApiLoginResponse> LoginAsync(string email, string password)
    {
        var body = new ApiLoginRequest { Email = email, Password = password };
        using var response = await _httpClient.PostAsync(
            "api/v1/auth/login", JsonContent(body));

        await ThrowIfNotSuccess(response);

        return await response.Content.ReadFromJsonAsync<ApiLoginResponse>(JsonOptions)
            ?? throw new ApiException(500, "The API returned an empty login response.");
    }

    public async Task<List<ApiDoctorSchedule>> GetMySchedulesAsync(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/doctor/schedules");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request);
        await ThrowIfNotSuccess(response);

        return await response.Content.ReadFromJsonAsync<List<ApiDoctorSchedule>>(JsonOptions)
            ?? new List<ApiDoctorSchedule>();
    }

    public async Task<List<ApiDoctorSchedule>> SaveMySchedulesAsync(string token, List<ApiScheduleDay> days)
    {
        var body = new ApiSaveScheduleRequest { Days = days };

        using var request = new HttpRequestMessage(HttpMethod.Put, "api/v1/doctor/schedules")
        {
            Content = JsonContent(body)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request);
        await ThrowIfNotSuccess(response);

        return await response.Content.ReadFromJsonAsync<List<ApiDoctorSchedule>>(JsonOptions)
            ?? new List<ApiDoctorSchedule>();
    }

    private static StringContent JsonContent<T>(T value)
    {
        return new StringContent(
            JsonSerializer.Serialize(value, JsonOptions), Encoding.UTF8, "application/json");
    }

    private static async Task ThrowIfNotSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        // The API sends ProblemDetails JSON: { "status": 400, "title": "...", "detail": "..." }
        string message = $"The API request failed ({(int)response.StatusCode}).";
        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("detail", out var detail))
            {
                message = detail.GetString() ?? message;
            }
        }
        catch (JsonException)
        {
            // Keep the generic message.
        }

        throw new ApiException((int)response.StatusCode, message);
    }
}