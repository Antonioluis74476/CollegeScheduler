using CollegeScheduler.Data.Entities.Facilities;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Data.Seed;

public static class FacilitiesSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Prevent reseeding
        if (await db.Rooms.AnyAsync() || await db.Buildings.AnyAsync())
            return;

        // -----------------------
        // Campuses
        // -----------------------
        var campuses = await db.Campuses.ToListAsync();

        if (!campuses.Any())
        {
            campuses = new List<Campus>
            {
                new() { Code = "DUB", Name = "Dublin Campus", City = "Dublin", Address = "Main Street", IsActive = true },
                new() { Code = "GAL", Name = "Galway Campus", City = "Galway", Address = "Main Street", IsActive = true },
                new() { Code = "COR", Name = "Cork Campus", City = "Cork", Address = "Main Street", IsActive = true }
            };

            db.Campuses.AddRange(campuses);
            await db.SaveChangesAsync();
        }

        var dub = campuses.Single(x => x.Code == "DUB");
        var gal = campuses.Single(x => x.Code == "GAL");
        var cor = campuses.Single(x => x.Code == "COR");

        // -----------------------
        // Buildings
        // -----------------------
        var buildings = new List<Building>
        {
            new() { CampusId = dub.CampusId, Code = "D-A", Name = "Dublin Academic Block", Faculty = "Science", IsActive = true },
            new() { CampusId = dub.CampusId, Code = "D-B", Name = "Dublin Business Block", Faculty = "Business", IsActive = true },

            new() { CampusId = gal.CampusId, Code = "G-A", Name = "Galway Teaching Centre", Faculty = "Engineering", IsActive = true },
            new() { CampusId = gal.CampusId, Code = "G-B", Name = "Galway Labs Block", Faculty = "Computing", IsActive = true },

            new() { CampusId = cor.CampusId, Code = "C-A", Name = "Cork Main Building", Faculty = "Arts", IsActive = true },
            new() { CampusId = cor.CampusId, Code = "C-B", Name = "Cork Technology Block", Faculty = "Technology", IsActive = true }
        };

        db.Buildings.AddRange(buildings);
        await db.SaveChangesAsync();

        // -----------------------
        // Room Types
        // -----------------------
        var roomTypes = new List<RoomType>
        {
            new() { Name = "Lecture Hall" },
            new() { Name = "Lab" },
            new() { Name = "Seminar Room" },
            new() { Name = "Exam Room" }
        };

        db.RoomTypes.AddRange(roomTypes);
        await db.SaveChangesAsync();

        var lectureHall = roomTypes.Single(x => x.Name == "Lecture Hall");
        var lab = roomTypes.Single(x => x.Name == "Lab");
        var seminar = roomTypes.Single(x => x.Name == "Seminar Room");
        var exam = roomTypes.Single(x => x.Name == "Exam Room");

        // -----------------------
        // Features
        // -----------------------
        var features = new List<Feature>
        {
            new() { Name = "Projector" },
            new() { Name = "Computers" },
            new() { Name = "Whiteboard" },
            new() { Name = "Video Conferencing" },
            new() { Name = "Wheelchair Access" }
        };

        db.Features.AddRange(features);
        await db.SaveChangesAsync();

        var projector = features.Single(x => x.Name == "Projector");
        var computers = features.Single(x => x.Name == "Computers");
        var whiteboard = features.Single(x => x.Name == "Whiteboard");
        var video = features.Single(x => x.Name == "Video Conferencing");
        var accessible = features.Single(x => x.Name == "Wheelchair Access");

        // -----------------------
        // Rooms
        // -----------------------
        var b1 = buildings[0];
        var b2 = buildings[1];
        var b3 = buildings[2];
        var b4 = buildings[3];
        var b5 = buildings[4];
        var b6 = buildings[5];

        var rooms = new List<Room>
        {
            new() { BuildingId = b1.BuildingId, RoomTypeId = lectureHall.RoomTypeId, Code = "A101", Name = "Lecture Hall A101", Floor = "1", Capacity = 80, IsBookableByStudents = false, RequiresApproval = true, IsActive = true, Notes = "Main lecture room" },
            new() { BuildingId = b1.BuildingId, RoomTypeId = seminar.RoomTypeId, Code = "A102", Name = "Seminar A102", Floor = "1", Capacity = 30, IsBookableByStudents = true, RequiresApproval = true, IsActive = true, Notes = "Small seminar room" },

            new() { BuildingId = b2.BuildingId, RoomTypeId = seminar.RoomTypeId, Code = "B201", Name = "Business Seminar B201", Floor = "2", Capacity = 40, IsBookableByStudents = true, RequiresApproval = true, IsActive = true, Notes = "Business teaching room" },
            new() { BuildingId = b2.BuildingId, RoomTypeId = lectureHall.RoomTypeId, Code = "B202", Name = "Lecture Hall B202", Floor = "2", Capacity = 100, IsBookableByStudents = false, RequiresApproval = true, IsActive = true, Notes = "Large lecture hall" },

            new() { BuildingId = b3.BuildingId, RoomTypeId = lab.RoomTypeId, Code = "G101", Name = "Engineering Lab G101", Floor = "1", Capacity = 25, IsBookableByStudents = false, RequiresApproval = true, IsActive = true, Notes = "Engineering practical lab" },
            new() { BuildingId = b3.BuildingId, RoomTypeId = exam.RoomTypeId, Code = "G102", Name = "Exam Hall G102", Floor = "1", Capacity = 60, IsBookableByStudents = false, RequiresApproval = true, IsActive = true, Notes = "Exam and assessments" },

            new() { BuildingId = b4.BuildingId, RoomTypeId = lab.RoomTypeId, Code = "L201", Name = "Computing Lab L201", Floor = "2", Capacity = 28, IsBookableByStudents = false, RequiresApproval = true, IsActive = true, Notes = "Computer lab" },
            new() { BuildingId = b4.BuildingId, RoomTypeId = seminar.RoomTypeId, Code = "L202", Name = "Teaching Room L202", Floor = "2", Capacity = 35, IsBookableByStudents = true, RequiresApproval = true, IsActive = true, Notes = "General teaching room" },

            new() { BuildingId = b5.BuildingId, RoomTypeId = seminar.RoomTypeId, Code = "C101", Name = "Arts Room C101", Floor = "1", Capacity = 20, IsBookableByStudents = true, RequiresApproval = true, IsActive = true, Notes = "Arts seminar room" },
            new() { BuildingId = b5.BuildingId, RoomTypeId = lectureHall.RoomTypeId, Code = "C102", Name = "Lecture Hall C102", Floor = "1", Capacity = 70, IsBookableByStudents = false, RequiresApproval = true, IsActive = true, Notes = "Shared lecture hall" },

            new() { BuildingId = b6.BuildingId, RoomTypeId = lab.RoomTypeId, Code = "T301", Name = "Technology Lab T301", Floor = "3", Capacity = 24, IsBookableByStudents = false, RequiresApproval = true, IsActive = true, Notes = "Technology practical room" },
            new() { BuildingId = b6.BuildingId, RoomTypeId = seminar.RoomTypeId, Code = "T302", Name = "Technology Seminar T302", Floor = "3", Capacity = 32, IsBookableByStudents = true, RequiresApproval = true, IsActive = true, Notes = "Seminar room" }
        };

        db.Rooms.AddRange(rooms);
        await db.SaveChangesAsync();

        // -----------------------
        // Room Features
        // -----------------------
        var roomFeatures = new List<RoomFeature>();

        foreach (var room in rooms)
        {
            roomFeatures.Add(new RoomFeature { RoomId = room.RoomId, FeatureId = projector.FeatureId });
            roomFeatures.Add(new RoomFeature { RoomId = room.RoomId, FeatureId = whiteboard.FeatureId });
            roomFeatures.Add(new RoomFeature { RoomId = room.RoomId, FeatureId = accessible.FeatureId });
        }

        foreach (var room in rooms.Where(r => r.Name.Contains("Lab")))
        {
            roomFeatures.Add(new RoomFeature { RoomId = room.RoomId, FeatureId = computers.FeatureId });
        }

        foreach (var room in rooms.Where(r => r.Capacity >= 70))
        {
            roomFeatures.Add(new RoomFeature { RoomId = room.RoomId, FeatureId = video.FeatureId });
        }

        db.RoomFeatures.AddRange(roomFeatures);
        await db.SaveChangesAsync();
    }
}