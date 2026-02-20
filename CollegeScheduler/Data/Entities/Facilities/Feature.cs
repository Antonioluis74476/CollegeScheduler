using Microsoft.AspNetCore.Http.Features;

namespace CollegeScheduler.Data.Entities.Facilities;

public class Feature
{
	public int FeatureId { get; set; }
	public string Name { get; set; } = "";

	public List<RoomFeature> RoomFeatures { get; set; } = new();
}
