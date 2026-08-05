# College Scheduler — Local Setup and Run Guide

This guide explains how to clone, configure, prepare, and run the complete **College Scheduler full-stack application** on a Windows computer.

Follow the steps in order when setting up the project on a new machine.

---

## 1. Prerequisites

Install the following software before cloning the repository:

- **Visual Studio 2022**
  - Install the **ASP.NET and web development** workload.
  - Include **SQL Server Express LocalDB** if offered by the installer.
- **.NET 8 SDK**
- **Git**
- **Docker Desktop**
- **SQL Server Management Studio (SSMS)**

Optional but recommended:

- The **.NET Entity Framework CLI tool**
- Microsoft Edge, Google Chrome, or another modern browser

To confirm that .NET is installed, open Command Prompt or PowerShell and run:

```powershell
dotnet --version
```

The returned version should begin with `8`.

To confirm that Git is installed, run:

```powershell
git --version
```

---

## 2. Clone the Repository

Repository:

```text
https://github.com/Antonioluis74476/CollegeScheduler.git
```

Branch:

```text
master
```

### Option A — Clone with Visual Studio

1. Open **Visual Studio 2022**.
2. Select **Clone a repository**.
3. Enter:

```text
https://github.com/Antonioluis74476/CollegeScheduler.git
```

4. Select the folder where the repository should be saved.
5. Click **Clone**.
6. Confirm that Visual Studio opens the `master` branch.

### Option B — Clone from the Terminal

Open Command Prompt or PowerShell and run:

```powershell
cd C:\Users\YourName\source\repos
git clone --branch master https://github.com/Antonioluis74476/CollegeScheduler.git
cd CollegeScheduler
```

---

## 3. Open the Correct Project

Inside the cloned repository, locate and open the solution or project containing:

```text
CollegeScheduler.csproj
```

If Visual Studio opened the repository without loading the project:

1. Select **File → Open → Project/Solution**.
2. Browse to the cloned repository.
3. Open the `.sln` file, or open `CollegeScheduler.csproj` directly.

Open a terminal in the folder that contains `CollegeScheduler.csproj`.

To confirm that you are in the correct folder, run:

```powershell
dir *.csproj
```

You should see:

```text
CollegeScheduler.csproj
```

---

## 4. Restore NuGet Packages

The required package versions are already included in the project file. Do not manually add or downgrade MassTransit or Identity packages.

Run:

```powershell
dotnet restore
```

Then build the project:

```powershell
dotnet build
```

The build should finish with:

```text
Build succeeded.
```

---

## 5. Check `appsettings.json`

Open:

```text
appsettings.json
```

Confirm that the local database connection uses:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-CollegeScheduler-ebb7c0ad-ee39-44a7-ac1d-dafc225dea1e;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Confirm that the frontend API base URL is:

```json
"ApiBaseUrl": "http://localhost:5119/"
```

The application also requires a local RabbitMQ URL. Ensure that this section exists:

```json
"RabbitMQ": {
  "Url": "rabbitmq://localhost"
}
```

A valid local configuration should contain the following structure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-CollegeScheduler-ebb7c0ad-ee39-44a7-ac1d-dafc225dea1e;Trusted_Connection=True;MultipleActiveResultSets=true"
  },

  "ApiBaseUrl": "http://localhost:5119/",

  "RabbitMQ": {
    "Url": "rabbitmq://localhost"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

  "AllowedHosts": "*",

  "Smtp": {
    "Host": "sandbox.smtp.mailtrap.io",
    "Port": 587,
    "FromAddress": "noreply@collegescheduler.local",
    "FromName": "College Scheduler"
  }
}
```

Do not place real Mailtrap, CloudAMQP, database, or other private credentials in a public repository.

---

## 6. Start Docker Desktop

1. Open **Docker Desktop**.
2. Wait until Docker reports that it is running.
3. Open Command Prompt or PowerShell.

Check whether the RabbitMQ container already exists:

```powershell
docker ps -a
```

### First-time RabbitMQ setup

If there is no container named `rabbitmq`, create it:

```powershell
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### Existing RabbitMQ container

If the container already exists, start it:

```powershell
docker start rabbitmq
```

Confirm that it is running:

```powershell
docker ps
```

You should see a container named:

```text
rabbitmq
```

Open the RabbitMQ dashboard:

```text
http://127.0.0.1:15672
```

Use:

| Field | Value |
|---|---|
| Username | `guest` |
| Password | `guest` |

Docker Desktop and the RabbitMQ container must be running before starting the application locally.

---

## 7. Install or Check Entity Framework Tools

Check whether the Entity Framework CLI tool is installed:

```powershell
dotnet ef --version
```

If the command is not recognised, install it:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

If it is already installed but needs updating:

```powershell
dotnet tool update --global dotnet-ef --version 8.*
```

Close and reopen the terminal if `dotnet ef` is still not recognised after installation.

---

## 8. Create the Local Database

Make sure the terminal is in the folder containing `CollegeScheduler.csproj`.

Run:

```powershell
dotnet ef database update
```

This creates or updates the LocalDB database using the Entity Framework migrations included in the repository.

The expected database name is:

```text
aspnet-CollegeScheduler-ebb7c0ad-ee39-44a7-ac1d-dafc225dea1e
```

The expected LocalDB server is:

```text
(localdb)\MSSQLLocalDB
```

If `dotnet ef database update` fails because of a build issue, run:

```powershell
dotnet build
```

Fix the build error before running the migration command again.

---

## 9. Confirm the Database in SSMS

1. Open **SQL Server Management Studio**.
2. Select **Connect → Database Engine**.
3. Enter:

| Setting | Value |
|---|---|
| Server name | `(localdb)\MSSQLLocalDB` |
| Authentication | `Windows Authentication` |

4. Click **Connect**.
5. Expand **Databases**.
6. Confirm that this database exists:

```text
aspnet-CollegeScheduler-ebb7c0ad-ee39-44a7-ac1d-dafc225dea1e
```

If the database does not appear:

1. Right-click **Databases**.
2. Select **Refresh**.
3. Confirm that `dotnet ef database update` completed successfully.

---

## 10. Load the Demo Data

Use the SQL file included with the project:

```text
CollegeScheduler_Full_DemoData_v10_Complete.sql
```

If the repository contains a numbered copy such as:

```text
CollegeScheduler_Full_DemoData_v10_Complete(2).sql
```

it may be renamed locally to:

```text
CollegeScheduler_Full_DemoData_v10_Complete.sql
```

In SSMS:

1. Select **File → Open → File**.
2. Open the demo-data SQL file.
3. In the database dropdown above the query editor, select:

```text
aspnet-CollegeScheduler-ebb7c0ad-ee39-44a7-ac1d-dafc225dea1e
```

4. Confirm that the correct database is selected.
5. Press **F5**, or select **Execute**.
6. Wait until the script finishes.

Do not run the demo-data script against `master` or another unrelated database.

The script populates the system with:

- Admin, lecturer, and student accounts
- Campuses, buildings, rooms, room types, and features
- Departments, programs, academic years, terms, cohorts, and modules
- Timetable events and assignments
- Requests, statuses, and notification-related data

The complete list of demo accounts is available in:

```text
DataBaseCredentials.xlsx
```

---

## 11. Build the Application

Return to the terminal in the project folder and run:

```powershell
dotnet build
```

Do not continue until the build succeeds.

---

## 12. Run the Full-Stack Application

Start the application with the HTTP launch profile:

```powershell
dotnet run --launch-profile http
```

The application should start at:

```text
http://localhost:5119/
```

Expected terminal output includes:

```text
Now listening on: http://localhost:5119
Application started. Press Ctrl+C to shut down.
```

Keep this terminal open while using the application.

To stop the application, press:

```text
Ctrl + C
```

---

## 13. Verify the Local Services

Open the following URLs:

| Service | URL |
|---|---|
| College Scheduler application | `http://localhost:5119/` |
| Swagger API | `http://localhost:5119/swagger` |
| SignalR test page | `http://localhost:5119/signalr-test.html` |
| RabbitMQ dashboard | `http://127.0.0.1:15672` |

The Blazor application and ASP.NET Core API run together as one full-stack project.

Swagger can be used to inspect and test API endpoints, while the Blazor interface provides the role-based frontend.

---

## 14. Demo Login Accounts

Use the same demo accounts documented in the repository README.

| Role | Email | Password |
|---|---|---|
| Admin | `admin@college.ie` | `Admin123!` |
| Lecturer | `elias.thornton@collegescheduler.ie` | `Password123!` |
| Student | `sophie.byrne@student.collegescheduler.ie` | `Password123!` |

Important:

```text
Password123!
```

contains two letters `s` in `Password`.

Additional accounts are available in:

```text
DataBaseCredentials.xlsx
```

---

## 15. Basic Test Checklist

After logging in, perform these checks.

### Admin

- Open the Admin dashboard.
- Confirm that campuses, buildings, rooms, departments, programs, and modules load.
- Open the timetable section.
- Test the available room finder.
- Test the clash checker.
- Confirm that requests and notifications pages load.

### Lecturer

- Open the lecturer timetable.
- Confirm that assigned classes are displayed.
- Open the request and notification sections.

### Student

- Open the student timetable.
- Confirm that cohort classes are displayed.
- Open the room-booking, requests, and notification sections.

### Swagger

- Open `http://localhost:5119/swagger`.
- Confirm that the API controller groups appear.
- Log in through the main application before testing endpoints that require authentication.

---

## 16. Run the Tests

Running automated tests is optional for normal local use, but recommended before final assessment or after code changes.

From the repository or solution folder, run:

```powershell
dotnet test
```

If the repository contains a separate test project, the command will restore, build, and execute its tests.

---

## 17. Normal Startup After the First Setup

After the database and demo data have already been created, the normal startup process is:

1. Open Docker Desktop.
2. Start RabbitMQ:

```powershell
docker start rabbitmq
```

3. Open the project folder in a terminal.
4. Build:

```powershell
dotnet build
```

5. Run:

```powershell
dotnet run --launch-profile http
```

6. Open:

```text
http://localhost:5119/
```

The database migration and demo-data steps normally need to be completed only once per machine unless the database is deleted or reset.

---

## 18. Troubleshooting

### `RabbitMQ:Url not found`

Cause: the `RabbitMQ` configuration section is missing from `appsettings.json`.

Add:

```json
"RabbitMQ": {
  "Url": "rabbitmq://localhost"
}
```

---

### RabbitMQ connection fails

Confirm that Docker Desktop is running:

```powershell
docker info
```

Start the RabbitMQ container:

```powershell
docker start rabbitmq
```

Confirm the container status:

```powershell
docker ps
```

---

### Docker says the container name is already in use

The RabbitMQ container already exists. Do not run `docker run` again.

Use:

```powershell
docker start rabbitmq
```

---

### `dotnet ef` is not recognised

Install the EF tool:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

Close and reopen the terminal.

---

### Database does not appear in SSMS

Run:

```powershell
dotnet ef database update
```

Then refresh the **Databases** folder in SSMS.

Confirm that SSMS is connected to:

```text
(localdb)\MSSQLLocalDB
```

---

### Demo-data script reports duplicate-key errors

The script may already have been executed.

Do not immediately execute it again. First check whether the tables already contain data.

For a clean test on another computer, create the database through migrations and run the demo-data file only once.

---

### Application opens but pages contain no data

Possible causes:

- The demo-data script was not executed.
- The script was executed against the wrong database.
- The application connection string points to a different database.
- The page requires a different user role.

Confirm that both SSMS and `appsettings.json` use:

```text
aspnet-CollegeScheduler-ebb7c0ad-ee39-44a7-ac1d-dafc225dea1e
```

---

### Unable to connect to `http://localhost:5119`

Run the application with the correct profile:

```powershell
dotnet run --launch-profile http
```

Confirm that the terminal displays:

```text
Now listening on: http://localhost:5119
```

---

### Build fails after cloning

Run:

```powershell
dotnet restore
dotnet clean
dotnet build
```

Do not run the old Identity scaffolding commands. Identity is already included in the repository.

Do not manually reinstall MassTransit. Its required package version is already defined in the project file.

---

### Swagger returns `401 Unauthorized` or `403 Forbidden`

The endpoint requires authentication or a specific role.

1. Log in through the main application.
2. Use an account with the required role.
3. Return to Swagger and test the endpoint again.

---

## 19. Important Local Files

| File | Purpose |
|---|---|
| `CollegeScheduler.csproj` | Project target framework and NuGet dependencies |
| `Program.cs` | Application startup, services, Identity, API, SignalR, and RabbitMQ configuration |
| `appsettings.json` | Local database, API, RabbitMQ, logging, and SMTP configuration |
| `Properties/launchSettings.json` | Local launch profiles and ports |
| `CollegeScheduler_Full_DemoData_v10_Complete.sql` | Final demo database content |
| `DataBaseCredentials.xlsx` | Complete list of demo user accounts |
| `README.md` | Project overview, features, technologies, deployment, and demo information |
| `SETUP.md` | This local installation and run guide |

---

## 20. Local Setup Summary

For a completely new machine:

```powershell
git clone --branch master https://github.com/Antonioluis74476/CollegeScheduler.git
cd CollegeScheduler
dotnet restore
dotnet build
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef database update
```

Then:

1. Load `CollegeScheduler_Full_DemoData_v10_Complete.sql` in SSMS.
2. Select the correct LocalDB database.
3. Execute the script once.
4. Run:

```powershell
dotnet run --launch-profile http
```

5. Open:

```text
http://localhost:5119/
```

For normal use after the first setup:

```powershell
docker start rabbitmq
dotnet build
dotnet run --launch-profile http
```
