using System.Net.Http.Json;
using System.Text.Json.Serialization;
using static System.Net.WebRequestMethods;

namespace CollegeScheduler.Services;

public sealed class AdminBuildingsApi
{
    private readonly HttpClient _http;

    public AdminBuildingsApi(HttpClient http)
        => _http = http;


    // This method retrieves a paginated list of buildings for a specific campus.
    public async Task<PagedResult<BuildingDto>> GetBuildingsAsync(int campusId, int page = 1, int pageSize = 20)
    {
        // If your API supports query params, keep them.
        // If not, we can remove them later safely.
        var url = $"api/v1/admin/campuses/{campusId}/buildings?page={page}&pageSize={pageSize}";

        return await _http.GetFromJsonAsync<PagedResult<BuildingDto>>(url)
               ?? new PagedResult<BuildingDto>();
    }


    // This method creates a new building for the specified campus.
    public async Task CreateBuildingAsync(int campusId, BuildingCreateDto dto)
    {
        var url = $"api/v1/admin/campuses/{campusId}/buildings";
        var res = await _http.PostAsJsonAsync(url, dto);
        res.EnsureSuccessStatusCode();
    }


    // This method retrieves a specific building by its ID.
    public async Task<BuildingDto?> GetBuildingByIdAsync(int id)
    {
        var url = $"api/v1/admin/buildings/{id}";
        return await _http.GetFromJsonAsync<BuildingDto>(url);
    }

    // This method updates an existing building with the provided data.
    public async Task UpdateBuildingAsync(int id, BuildingUpdateDto dto)
    {
        var url = $"api/v1/admin/buildings/{id}";
        var res = await _http.PutAsJsonAsync(url, dto);
        res.EnsureSuccessStatusCode();
    }

    // This method deletes a building by its ID.
    public async Task<(bool ok, int statusCode, string body)> DeleteBuildingDebugAsync(int id)
    {
        var url = $"api/v1/admin/buildings/{id}";
        var res = await _http.DeleteAsync(url);
        var body = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, (int)res.StatusCode, body);
    }


}

// This is a generic paged result DTO that can be used for any paginated API response.
public sealed class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

// This is the DTO for a building.
public sealed class BuildingDto
{
    [JsonPropertyName("buildingId")]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public bool IsActive { get; set; }
}


// This is the DTO for creating a building.
public sealed class BuildingCreateDto
{
    public string Name { get; set; } = "";
    public string? Code { get; set; }
}


// This is the DTO for updating a building.
public sealed class BuildingUpdateDto
{
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    }

