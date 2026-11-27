using System.Threading.Tasks;
using MediCare.Web.Models.Api;

namespace MediCare.Web.Services;

// The ONLY way MediCare.Web talks to the API. Every method throws ApiException when the API returns an error
public interface IMediCareApiClient
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RegisterPatientAsync(RegisterPatientRequestDto request);
    Task<LoginResponseDto> RegisterDoctorAsync(RegisterDoctorRequestDto request);
    Task<UserDto> GetMeAsync();

    Task<PagedResultDto<NotificationDto>> GetNotificationsAsync(bool unreadOnly, int page, int pageSize);
    Task<int> GetUnreadCountAsync();
    Task MarkNotificationAsReadAsync(int notificationId);
    Task MarkAllNotificationsAsReadAsync();
    Task<HubTokenDto> GetHubTokenAsync();

    // TEMPORARY (development only): creates a test notification for the logged in user
    Task CreateTestNotificationAsync();
}