using CollegeScheduler.Components;
using CollegeScheduler.Components.Account;
using CollegeScheduler.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CollegeScheduler.Services;


// usings for API clients
using CollegeScheduler.Services.Implementations;
using CollegeScheduler.Services.Interfaces;
using CollegeScheduler.Hubs;

var builder = WebApplication.CreateBuilder(args);

// API clients // FrontEnd

// FirstPart Branch
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CollegeScheduler.Services.ForwardAuthCookieHandler>();

builder.Services.AddHttpClient<CollegeScheduler.Services.AdminBuildingsApi>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
})
.AddHttpMessageHandler<CollegeScheduler.Services.ForwardAuthCookieHandler>();


// SecondPart Branch
builder.Services.AddScoped<CollegeScheduler.Services.AdminCampusState>();


// API clients
builder.Services.AddHttpClient<ICampusService, CampusService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
})
.AddHttpMessageHandler<CollegeScheduler.Services.ForwardAuthCookieHandler>();

builder.Services.AddHttpClient<IBuildingService, BuildingService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
})
.AddHttpMessageHandler<CollegeScheduler.Services.ForwardAuthCookieHandler>();

builder.Services.AddHttpClient<IRoomService, RoomService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
})
.AddHttpMessageHandler<CollegeScheduler.Services.ForwardAuthCookieHandler>();

builder.Services.AddHttpClient<IAdminSchedulingService, AdminSchedulingService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
})
.AddHttpMessageHandler<CollegeScheduler.Services.ForwardAuthCookieHandler>();



// UI (Blazor)
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// Identity + auth state
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = IdentityConstants.ApplicationScheme;
	options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

// Db
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity + roles
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddSignInManager()
	.AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Controllers (API)
builder.Services.AddControllers();

//Services
builder.Services.AddScoped<ISchedulingService, SchedulingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRequestService, RequestService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<TimetableHubNotifier>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();

	await CollegeScheduler.Data.Identity.IdentitySeeder.SeedAsync(app.Services, app.Configuration);
	await CollegeScheduler.Data.Seed.FacilitiesSeeder.SeedAsync(app.Services);
	await CollegeScheduler.Data.Seed.SchedulingLookupSeeder.SeedAsync(app.Services);


	app.UseSwagger();
	app.UseSwaggerUI();
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// API endpoints
app.MapControllers();
app.MapHub<TimetableHub>("/hubs/timetable");

// Blazor endpoints
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();



app.Run();
