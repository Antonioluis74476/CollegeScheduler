namespace CollegeScheduler.DTOs.Requests;

public sealed class PendingRequestDto
{
    public long RequestId { get; set; }

    public string? Title { get; set; }

    public string? Notes { get; set; }

    public string RequestType { get; set; } = string.Empty;

    public string RequestStatus { get; set; } = string.Empty;

    public string RequestedByUserId { get; set; } = string.Empty;

    public string? RequestedByName { get; set; }

    public string? RequestedByEmail { get; set; }

    public string RequestedByRole { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public PendingScheduleChangeDetailDto? ScheduleChangeDetail { get; set; }

    public PendingRoomBookingDetailDto? RoomBookingDetail { get; set; }
}

public sealed class PendingScheduleChangeDetailDto
{
    public long TimetableEventId { get; set; }

    public string? ModuleCode { get; set; }

    public string? ModuleTitle { get; set; }

    public int CurrentRoomId { get; set; }

    public string? CurrentRoomCode { get; set; }

    public string? CurrentRoomName { get; set; }

    public DateTime CurrentStartUtc { get; set; }

    public DateTime CurrentEndUtc { get; set; }

    public int? ProposedRoomId { get; set; }

    public string? ProposedRoomCode { get; set; }

    public string? ProposedRoomName { get; set; }

    public DateTime? ProposedStartUtc { get; set; }

    public DateTime? ProposedEndUtc { get; set; }

    public string Reason { get; set; } = string.Empty;
}

public sealed class PendingRoomBookingDetailDto
{
    public int RoomId { get; set; }

    public string? RoomCode { get; set; }

    public string? RoomName { get; set; }


    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public int ExpectedAttendees { get; set; }
}