using System.Net.Http.Json;
using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.DTOs.Student;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations;

public sealed class StudentService : IStudentService
{
	private readonly HttpClient _httpClient;

	public StudentService(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<StudentDto?> GetProfileAsync()
	{
		using var response = await _httpClient.GetAsync(
			"api/v1/student/profile");

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<StudentDto>();
	}

	public async Task<IReadOnlyList<StudentTimetableItemDto>>
		GetTimetableAsync(
			DateTime? fromUtc = null,
			DateTime? toUtc = null)
	{
		var parameters = new List<string>();

		if (fromUtc.HasValue)
		{
			parameters.Add(
				$"fromUtc={Uri.EscapeDataString(fromUtc.Value.ToString("O"))}");
		}

		if (toUtc.HasValue)
		{
			parameters.Add(
				$"toUtc={Uri.EscapeDataString(toUtc.Value.ToString("O"))}");
		}

		var endpoint = "api/v1/student/timetable";

		if (parameters.Count > 0)
		{
			endpoint += "?" + string.Join("&", parameters);
		}

		using var response = await _httpClient.GetAsync(endpoint);

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<List<StudentTimetableItemDto>>()
			?? [];
	}

	public async Task<IReadOnlyList<StudentNotificationDto>>
		GetNotificationsAsync(bool unreadOnly = false)
	{
		var endpoint =
			$"api/v1/student/notifications" +
			$"?unreadOnly={unreadOnly.ToString().ToLowerInvariant()}";

		using var response = await _httpClient.GetAsync(endpoint);

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<List<StudentNotificationDto>>()
			?? [];
	}

	public async Task MarkNotificationAsReadAsync(
		long notificationId)
	{
		using var response = await _httpClient.PostAsync(
			$"api/v1/student/notifications/{notificationId}/read",
			null);

		await EnsureSuccessAsync(response);
	}

	public async Task<RoomBookingResponseDto?>
		CreateRoomBookingRequestAsync(
			RoomBookingRequestCreateDto dto)
	{
		using var response = await _httpClient.PostAsJsonAsync(
			"api/v1/student/requests/room-booking",
			dto);

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<RoomBookingResponseDto>();
	}

	public async Task<IReadOnlyList<StudentRequestDto>>
		GetRequestsAsync()
	{
		using var response = await _httpClient.GetAsync(
			"api/v1/student/requests");

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<List<StudentRequestDto>>()
			?? [];
	}

	public async Task<ApiMessageResponseDto?> ChangePasswordAsync(
		ChangePasswordDto dto)
	{
		using var response = await _httpClient.PostAsJsonAsync(
			"api/v1/student/profile/change-password",
			dto);

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<ApiMessageResponseDto>();
	}

	private static async Task EnsureSuccessAsync(
		HttpResponseMessage response)
	{
		if (response.IsSuccessStatusCode)
		{
			return;
		}

		var error = await response.Content.ReadAsStringAsync();

		if (string.IsNullOrWhiteSpace(error))
		{
			error =
				$"The request failed with status code " +
				$"{(int)response.StatusCode} " +
				$"({response.StatusCode}).";
		}

		throw new HttpRequestException(error);
	}
}