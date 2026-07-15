using System.Net.Http.Json;
using CollegeScheduler.DTOs.Lecturer;
using CollegeScheduler.DTOs.Profiles;
using CollegeScheduler.DTOs.Requests;
using CollegeScheduler.Services.Interfaces;

namespace CollegeScheduler.Services.Implementations;

public sealed class LecturerService : ILecturerService
{
	private readonly HttpClient _httpClient;

	public LecturerService(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<LecturerProfileDto?> GetProfileAsync()
	{
		using var response = await _httpClient.GetAsync(
			"api/v1/lecturer/profile");

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<LecturerProfileDto>();
	}

	public async Task<IReadOnlyList<LecturerTimetableItemDto>>
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

		var endpoint = "api/v1/lecturer/timetable";

		if (parameters.Count > 0)
		{
			endpoint += "?" + string.Join("&", parameters);
		}

		using var response = await _httpClient.GetAsync(endpoint);

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<List<LecturerTimetableItemDto>>()
			?? [];
	}

	public async Task<IReadOnlyList<LecturerNotificationDto>>
		GetNotificationsAsync(bool unreadOnly = false)
	{
		var endpoint =
			$"api/v1/lecturer/notifications" +
			$"?unreadOnly={unreadOnly.ToString().ToLowerInvariant()}";

		using var response = await _httpClient.GetAsync(endpoint);

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<List<LecturerNotificationDto>>()
			?? [];
	}

	public async Task MarkNotificationAsReadAsync(long notificationId)
	{
		using var response = await _httpClient.PostAsync(
			$"api/v1/lecturer/notifications/{notificationId}/read",
			content: null);

		await EnsureSuccessAsync(response);
	}

	public async Task<IReadOnlyList<LecturerRequestDto>>
		GetRequestsAsync()
	{
		using var response = await _httpClient.GetAsync(
			"api/v1/lecturer/requests");

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<List<LecturerRequestDto>>()
			?? [];
	}

	public async Task<LecturerRequestResponseDto?>
		CreateScheduleChangeRequestAsync(
			ScheduleChangeRequestCreateDto dto)
	{
		using var response = await _httpClient.PostAsJsonAsync(
			"api/v1/lecturer/requests/schedule-change",
			dto);

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<LecturerRequestResponseDto>();
	}

	public async Task<LecturerRequestResponseDto?>
		CreateCancelClassRequestAsync(
			CancelClassRequestCreateDto dto)
	{
		using var response = await _httpClient.PostAsJsonAsync(
			"api/v1/lecturer/requests/cancel-class",
			dto);

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<LecturerRequestResponseDto>();
	}

	public async Task<LecturerRequestResponseDto?>
		CreateRoomBookingRequestAsync(
			LecturerRoomBookingRequestCreateDto dto)
	{
		using var response = await _httpClient.PostAsJsonAsync(
			"api/v1/lecturer/requests/room-booking",
			dto);

		await EnsureSuccessAsync(response);

		return await response.Content
			.ReadFromJsonAsync<LecturerRequestResponseDto>();
	}

	public async Task<ApiMessageResponseDto?> ChangePasswordAsync(
		ChangePasswordDto dto)
	{
		using var response = await _httpClient.PostAsJsonAsync(
			"api/v1/lecturer/profile/change-password",
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
				$"{(int)response.StatusCode} ({response.StatusCode}).";
		}

		throw new HttpRequestException(error);
	}
}