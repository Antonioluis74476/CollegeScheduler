using CollegeScheduler.Data.Entities.Academic;
using CollegeScheduler.Data.Entities.Membership;
using CollegeScheduler.Data.Entities.Profiles;
using CollegeScheduler.Data.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace CollegeScheduler.Data.Seed;

public static class TestAcademicSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Prevent reseeding
        if (await db.Departments.AnyAsync() ||
            await db.AcademicPrograms.AnyAsync() ||
            await db.Cohorts.AnyAsync() ||
            await db.Modules.AnyAsync())
        {
            return;
        }

        // -----------------------
        // Academic Year
        // -----------------------
        var academicYear = new AcademicYear
        {
            Label = "2025/2026",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2026, 6, 30),
            IsCurrent = true
        };

        db.AcademicYears.Add(academicYear);
        await db.SaveChangesAsync();

        // -----------------------
        // Terms
        // -----------------------
        var autumn = new Term
        {
            AcademicYearId = academicYear.AcademicYearId,
            TermNumber = 1,
            Name = "Autumn",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2025, 12, 20)
        };

        var spring = new Term
        {
            AcademicYearId = academicYear.AcademicYearId,
            TermNumber = 2,
            Name = "Spring",
            StartDate = new DateTime(2026, 1, 15),
            EndDate = new DateTime(2026, 5, 31)
        };

        db.Terms.AddRange(autumn, spring);
        await db.SaveChangesAsync();

        // -----------------------
        // Departments
        // -----------------------
        var departments = new List<Department>
        {
            new() { Code = "CS",   Name = "Computing Science", IsActive = true },
            new() { Code = "BUS",  Name = "Business",          IsActive = true },
            new() { Code = "ENG",  Name = "Engineering",       IsActive = true },
            new() { Code = "ART",  Name = "Arts",              IsActive = true }
        };

        db.Departments.AddRange(departments);
        await db.SaveChangesAsync();

        var deptCs = departments.Single(x => x.Code == "CS");
        var deptBus = departments.Single(x => x.Code == "BUS");
        var deptEng = departments.Single(x => x.Code == "ENG");
        var deptArt = departments.Single(x => x.Code == "ART");

        // -----------------------
        // Programs
        // -----------------------
        var programs = new List<AcademicProgram>
        {
            new() { Code = "CS-BSC",   Name = "BSc Computer Science",      DepartmentId = deptCs.DepartmentId,  Level = "Bachelor", DurationYears = 4, IsActive = true },
            new() { Code = "SD-BSC",   Name = "BSc Software Development",  DepartmentId = deptCs.DepartmentId,  Level = "Bachelor", DurationYears = 4, IsActive = true },

            new() { Code = "BM-BA",    Name = "BA Business Management",    DepartmentId = deptBus.DepartmentId, Level = "Bachelor", DurationYears = 3, IsActive = true },
            new() { Code = "MKT-BA",   Name = "BA Marketing",              DepartmentId = deptBus.DepartmentId, Level = "Bachelor", DurationYears = 3, IsActive = true },

            new() { Code = "ME-BSC",   Name = "BSc Mechanical Engineering",DepartmentId = deptEng.DepartmentId, Level = "Bachelor", DurationYears = 4, IsActive = true },
            new() { Code = "EE-BSC",   Name = "BSc Electrical Engineering",DepartmentId = deptEng.DepartmentId, Level = "Bachelor", DurationYears = 4, IsActive = true },

            new() { Code = "DES-BA",   Name = "BA Design",                 DepartmentId = deptArt.DepartmentId, Level = "Bachelor", DurationYears = 3, IsActive = true },
            new() { Code = "MEDIA-BA", Name = "BA Media Studies",          DepartmentId = deptArt.DepartmentId, Level = "Bachelor", DurationYears = 3, IsActive = true }
        };

        db.AcademicPrograms.AddRange(programs);
        await db.SaveChangesAsync();

        // -----------------------
        // Cohorts
        // 2 per program: Year 1 A, Year 1 B
        // -----------------------
        var cohorts = new List<Cohort>();

        foreach (var program in programs)
        {
            cohorts.Add(new Cohort
            {
                ProgramId = program.ProgramId,
                AcademicYearId = academicYear.AcademicYearId,
                YearOfStudy = 1,
                Code = $"{program.Code}-Y1A",
                Name = $"{program.Name} Year 1 - A",
                ExpectedSize = 25,
                IsActive = true
            });

            cohorts.Add(new Cohort
            {
                ProgramId = program.ProgramId,
                AcademicYearId = academicYear.AcademicYearId,
                YearOfStudy = 1,
                Code = $"{program.Code}-Y1B",
                Name = $"{program.Name} Year 1 - B",
                ExpectedSize = 25,
                IsActive = true
            });
        }

        db.Cohorts.AddRange(cohorts);
        await db.SaveChangesAsync();

        // -----------------------
        // Modules
        // -----------------------
        var modules = new List<Module>
        {
            new() { Code = "PROG101", Title = "Programming Fundamentals", Credits = 5, HoursPerWeek = 4, MinRoomCapacity = 20, DepartmentId = deptCs.DepartmentId,  IsActive = true },
            new() { Code = "DB101",   Title = "Databases",                Credits = 5, HoursPerWeek = 4, MinRoomCapacity = 20, DepartmentId = deptCs.DepartmentId,  IsActive = true },
            new() { Code = "WEB101",  Title = "Web Development",          Credits = 5, HoursPerWeek = 4, MinRoomCapacity = 20, DepartmentId = deptCs.DepartmentId,  IsActive = true },

            new() { Code = "BUS101",  Title = "Business Essentials",      Credits = 5, HoursPerWeek = 3, MinRoomCapacity = 25, DepartmentId = deptBus.DepartmentId, IsActive = true },
            new() { Code = "MKT101",  Title = "Marketing Basics",         Credits = 5, HoursPerWeek = 3, MinRoomCapacity = 25, DepartmentId = deptBus.DepartmentId, IsActive = true },

            new() { Code = "ENG101",  Title = "Engineering Maths",        Credits = 5, HoursPerWeek = 4, MinRoomCapacity = 20, DepartmentId = deptEng.DepartmentId, IsActive = true },
            new() { Code = "PHY101",  Title = "Engineering Physics",      Credits = 5, HoursPerWeek = 4, MinRoomCapacity = 20, DepartmentId = deptEng.DepartmentId, IsActive = true },

            new() { Code = "ART101",  Title = "Digital Design",           Credits = 5, HoursPerWeek = 3, MinRoomCapacity = 20, DepartmentId = deptArt.DepartmentId, IsActive = true },
            new() { Code = "MED101",  Title = "Media Production",         Credits = 5, HoursPerWeek = 3, MinRoomCapacity = 20, DepartmentId = deptArt.DepartmentId, IsActive = true },

            new() { Code = "COM101",  Title = "Communication Skills",     Credits = 5, HoursPerWeek = 2, MinRoomCapacity = 20, DepartmentId = deptBus.DepartmentId, IsActive = true }
        };

        db.Modules.AddRange(modules);
        await db.SaveChangesAsync();

        // -----------------------
        // CohortModules
        // Give each program-cohort 3 modules based on department
        // -----------------------
        var csPrograms = new[] { "CS-BSC", "SD-BSC" };
        var busPrograms = new[] { "BM-BA", "MKT-BA" };
        var engPrograms = new[] { "ME-BSC", "EE-BSC" };
        var artPrograms = new[] { "DES-BA", "MEDIA-BA" };

        var csModules = modules.Where(m => new[] { "PROG101", "DB101", "WEB101", "COM101" }.Contains(m.Code)).ToList();
        var busModules = modules.Where(m => new[] { "BUS101", "MKT101", "COM101" }.Contains(m.Code)).ToList();
        var engModules = modules.Where(m => new[] { "ENG101", "PHY101", "COM101" }.Contains(m.Code)).ToList();
        var artModules = modules.Where(m => new[] { "ART101", "MED101", "COM101" }.Contains(m.Code)).ToList();

        var cohortModules = new List<CohortModule>();

        foreach (var cohort in cohorts)
        {
            var cohortProgram = programs.Single(p => p.ProgramId == cohort.ProgramId);

            List<Module> assignedModules;
            if (csPrograms.Contains(cohortProgram.Code))
                assignedModules = csModules.Take(3).ToList();
            else if (busPrograms.Contains(cohortProgram.Code))
                assignedModules = busModules.Take(3).ToList();
            else if (engPrograms.Contains(cohortProgram.Code))
                assignedModules = engModules.Take(3).ToList();
            else
                assignedModules = artModules.Take(3).ToList();

            foreach (var module in assignedModules)
            {
                cohortModules.Add(new CohortModule
                {
                    CohortId = cohort.CohortId,
                    ModuleId = module.ModuleId,
                    TermId = autumn.TermId,
                    IsRequired = true
                });
            }
        }

        db.CohortModules.AddRange(cohortModules);
        await db.SaveChangesAsync();

        // -----------------------
        // Lecturers
        // -----------------------
        var lecturers = new List<LecturerProfile>
        {
            new() { Name = "John Smith",     StaffNumber = "L001", Email = "john@college.com",     DepartmentId = deptCs.DepartmentId,  EmploymentType = "FullTime", MaxWeeklyHours = 40, IsActive = true },
            new() { Name = "Anna Brown",     StaffNumber = "L002", Email = "anna@college.com",     DepartmentId = deptCs.DepartmentId,  EmploymentType = "FullTime", MaxWeeklyHours = 40, IsActive = true },

            new() { Name = "Mark Lee",       StaffNumber = "L003", Email = "mark@college.com",     DepartmentId = deptBus.DepartmentId, EmploymentType = "FullTime", MaxWeeklyHours = 40, IsActive = true },
            new() { Name = "Sara Kelly",     StaffNumber = "L004", Email = "sara@college.com",     DepartmentId = deptBus.DepartmentId, EmploymentType = "PartTime", MaxWeeklyHours = 20, IsActive = true },

            new() { Name = "David Murphy",   StaffNumber = "L005", Email = "david@college.com",    DepartmentId = deptEng.DepartmentId, EmploymentType = "FullTime", MaxWeeklyHours = 40, IsActive = true },
            new() { Name = "Nina O'Brien",   StaffNumber = "L006", Email = "nina@college.com",     DepartmentId = deptEng.DepartmentId, EmploymentType = "PartTime", MaxWeeklyHours = 24, IsActive = true },

            new() { Name = "Laura Green",    StaffNumber = "L007", Email = "laura@college.com",    DepartmentId = deptArt.DepartmentId, EmploymentType = "FullTime", MaxWeeklyHours = 40, IsActive = true },
            new() { Name = "Tom Wilson",     StaffNumber = "L008", Email = "tom@college.com",      DepartmentId = deptArt.DepartmentId, EmploymentType = "PartTime", MaxWeeklyHours = 18, IsActive = true }
        };

        db.LecturerProfiles.AddRange(lecturers);
        await db.SaveChangesAsync();

        // -----------------------
        // ModuleLecturers
        // -----------------------
        var lecturerMap = new Dictionary<string, string>
        {
            ["PROG101"] = "L001",
            ["DB101"] = "L002",
            ["WEB101"] = "L001",
            ["BUS101"] = "L003",
            ["MKT101"] = "L004",
            ["ENG101"] = "L005",
            ["PHY101"] = "L006",
            ["ART101"] = "L007",
            ["MED101"] = "L008",
            ["COM101"] = "L003"
        };

        var moduleLecturers = new List<ModuleLecturer>();

        foreach (var module in modules)
        {
            var staffNumber = lecturerMap[module.Code];
            var lecturer = lecturers.Single(l => l.StaffNumber == staffNumber);

            moduleLecturers.Add(new ModuleLecturer
            {
                ModuleId = module.ModuleId,
                LecturerId = lecturer.LecturerId,
                TermId = autumn.TermId,
                Role = "Lead",
                AssignedAtUtc = DateTime.UtcNow
            });
        }

        db.ModuleLecturers.AddRange(moduleLecturers);
        await db.SaveChangesAsync();

        // -----------------------
        // Students
        // 12 students, spread across first 6 cohorts
        // -----------------------
        var students = new List<StudentProfile>
        {
            new() { Name = "Student One",    StudentNumber = "S001", Email = "s1@college.com",  Status = "Active" },
            new() { Name = "Student Two",    StudentNumber = "S002", Email = "s2@college.com",  Status = "Active" },
            new() { Name = "Student Three",  StudentNumber = "S003", Email = "s3@college.com",  Status = "Active" },
            new() { Name = "Student Four",   StudentNumber = "S004", Email = "s4@college.com",  Status = "Active" },
            new() { Name = "Student Five",   StudentNumber = "S005", Email = "s5@college.com",  Status = "Active" },
            new() { Name = "Student Six",    StudentNumber = "S006", Email = "s6@college.com",  Status = "Active" },
            new() { Name = "Student Seven",  StudentNumber = "S007", Email = "s7@college.com",  Status = "Active" },
            new() { Name = "Student Eight",  StudentNumber = "S008", Email = "s8@college.com",  Status = "Active" },
            new() { Name = "Student Nine",   StudentNumber = "S009", Email = "s9@college.com",  Status = "Active" },
            new() { Name = "Student Ten",    StudentNumber = "S010", Email = "s10@college.com", Status = "Active" },
            new() { Name = "Student Eleven", StudentNumber = "S011", Email = "s11@college.com", Status = "Active" },
            new() { Name = "Student Twelve", StudentNumber = "S012", Email = "s12@college.com", Status = "Active" }
        };

        db.StudentProfiles.AddRange(students);
        await db.SaveChangesAsync();

        // -----------------------
        // StudentCohortMemberships
        // -----------------------
        var studentMemberships = new List<StudentCohortMembership>();
        var targetCohorts = cohorts.Take(6).ToList();

        for (int i = 0; i < students.Count; i++)
        {
            var assignedCohort = targetCohorts[i % targetCohorts.Count];

            studentMemberships.Add(new StudentCohortMembership
            {
                StudentId = students[i].StudentId,
                CohortId = assignedCohort.CohortId,
                AcademicYearId = academicYear.AcademicYearId,
                MembershipType = "Primary",
                StartDate = DateOnly.FromDateTime(academicYear.StartDate),
                EndDate = DateOnly.FromDateTime(academicYear.EndDate)
            });
        }

        db.StudentCohortMemberships.AddRange(studentMemberships);
        await db.SaveChangesAsync();

        // -----------------------
        // Timetable Events
        // Requires at least one Room and one EventStatus from existing seeders
        // -----------------------
        var room = await db.Rooms.OrderBy(r => r.RoomId).FirstOrDefaultAsync();
        var scheduledStatus = await db.EventStatuses.FirstOrDefaultAsync(x => x.Name == "Scheduled");

        if (room != null && scheduledStatus != null)
        {
            var timetableEvents = new List<TimetableEvent>();
            var eventCohorts = new List<EventCohort>();
            var eventLecturers = new List<EventLecturer>();

            var weekdayDates = new[]
            {
                new DateTime(2025, 9, 8, 9, 0, 0),   // Monday
                new DateTime(2025, 9, 8, 11, 0, 0),  // Monday
                new DateTime(2025, 9, 9, 10, 0, 0),  // Tuesday
                new DateTime(2025, 9, 10, 14, 0, 0), // Wednesday
                new DateTime(2025, 9, 11, 9, 0, 0)   // Thursday
            };

            var targetEventCohorts = cohorts.Take(8).ToList();

            foreach (var cohort in targetEventCohorts)
            {
                var assignedModules = cohortModules
                    .Where(cm => cm.CohortId == cohort.CohortId && cm.TermId == autumn.TermId)
                    .Take(3)
                    .ToList();

                for (int i = 0; i < assignedModules.Count; i++)
                {
                    var cohortModule = assignedModules[i];
                    var module = modules.Single(m => m.ModuleId == cohortModule.ModuleId);
                    var assignedModuleLecturer = moduleLecturers.Single(ml => ml.ModuleId == module.ModuleId && ml.TermId == autumn.TermId);

                    var start = weekdayDates[i % weekdayDates.Length].AddDays(i);
                    var end = start.AddHours(2);

                    var evt = new TimetableEvent
                    {
                        TermId = autumn.TermId,
                        ModuleId = module.ModuleId,
                        RoomId = room.RoomId,
                        StartUtc = DateTime.SpecifyKind(start, DateTimeKind.Utc),
                        EndUtc = DateTime.SpecifyKind(end, DateTimeKind.Utc),
                        EventStatusId = scheduledStatus.EventStatusId,
                        SessionType = "Lecture",
                        Notes = $"Seeded event for {cohort.Name}",
                        CreatedByUserId = "seed"
                    };

                    timetableEvents.Add(evt);
                }
            }

            db.TimetableEvents.AddRange(timetableEvents);
            await db.SaveChangesAsync();

            foreach (var evt in timetableEvents)
            {
                var cohortForEvent = targetEventCohorts.FirstOrDefault(c =>
                    evt.Notes != null && evt.Notes.Contains(c.Name));

                if (cohortForEvent == null)
                    continue;

                eventCohorts.Add(new EventCohort
                {
                    TimetableEventId = evt.TimetableEventId,
                    CohortId = cohortForEvent.CohortId
                });

                var lecturerForModule = moduleLecturers.Single(ml => ml.ModuleId == evt.ModuleId && ml.TermId == evt.TermId);

                eventLecturers.Add(new EventLecturer
                {
                    TimetableEventId = evt.TimetableEventId,
                    LecturerId = lecturerForModule.LecturerId
                });
            }

            db.EventCohorts.AddRange(eventCohorts);
            db.EventLecturers.AddRange(eventLecturers);
            await db.SaveChangesAsync();
        }
    }
}