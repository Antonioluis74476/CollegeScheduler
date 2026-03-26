namespace CollegeScheduler.Services;

public sealed class AdminCampusState
{
    // Default keeps the current behavior (Campus 1) until we add the dropdown.
    public int SelectedCampusId { get; set; } = 1;
}