**College Scheduler**

**Setup & Run Guide — Backend | Master Branch**

**Overview**
This guide walks you through setting up and running the College Scheduler backend on a new machine. Follow each step in order.

**Technologies used:**
   - .NET 8 / ASP.NET Core + Blazor Server
   -  SQL Server LocalDB (database)
   -  ASP.NET Identity (role-based authentication)
   -  SignalR (real-time updates)
   -  RabbitMQ + MassTransit (async messaging)
   -  Swagger (API testing)


**Step 1 — Prerequisites**

Make sure the following are installed before continuing:

* Visual Studio 2022 (with ASP.NET and web development workload)
* .NET 8 SDK — included with Visual Studio 2022
* SQL Server LocalDB — included with Visual Studio 2022
* SSMS (SQL Server Management Studio) — for loading demo data
* Docker Desktop — required to run RabbitMQ
* Git — to clone the repository
* DemoData.sql file


**Step 2 — Clone the Repository**

* Open the Windows terminal (Command Prompt) and navigate to the folder where you want to place the project:
    
         - cd C:\Users\YourName\source\repos

* Clone the repository:
    
         - git clone https://github.com/Antonioluis74476/CollegeScheduler.git CollegeScheduler

* Navigate into the project folder:
    
         - cd CollegeScheduler\CollegeScheduler
  
**Step 3 — Start RabbitMQ**

Open Docker Desktop from the Start menu and wait for it to fully load (whale icon appears in the taskbar).

* If this is your first time running it, use this command:
    
        - docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

* Then run in the terminal:
    
        - docker start rabbitmq
  
**!****!** Verify RabbitMQ is running by opening http://127.0.0.1:15672 and logging in with 
    
    **User**: guest
    
    **Password**: guest
    
**!****!** Docker Desktop must be open and running every time you use this project.

**Step 4 — Fix MassTransit Package Version**

* The project requires MassTransit version 8.3.5. Run these two commands:
    
        - dotnet add package MassTransit --version 8.3.5
    
        - dotnet add package MassTransit.RabbitMQ --version 8.3.5

**!****!** This step is required. Without it the application will fail to start with a license error.


**Step 5 — Scaffold Identity Pages**

* Run these commands in order:
    
        - dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design --version 8.0.7
    
        - dotnet add package Microsoft.AspNetCore.Identity.UI --version 8.0.0
    
        - dotnet aspnet-codegenerator identity --useDefaultUI

**!****!** This step is required. Without it the project will fail to build.


**Step 6 — Build and Run**

* First verify the build succeeds:
    
         - dotnet build

* Then run the project:
    
        - dotnet run

* You should see these three lines in the terminal:
    
  - "Now listening on: http://localhost:5119"
    
  - "Application started. Press Ctrl+C to shut down."
    
  - "Bus started: rabbitmq://localhost/"

**!****!** If the Bus started line is missing, RabbitMQ is not running. **Go back to Step 3**.

Once the app is running, open the following in your browser:

| Interface              | URL                              |
|-----------------------|----------------------------------|
| Main App              | http://localhost:5119            |
| Swagger (API testing) | http://localhost:5119/swagger    |
| SignalR Test Page     | http://localhost:5119/signalr-test.html |
| RabbitMQ Dashboard    | http://127.0.0.1:15672           |

**!** You won't be able to log in yet — demo data must be loaded first **(Step 7)**.


**Step 7 — Load Demo Data**

Open SQL Server Management Studio (SSMS) and:

1. Click **Connect** and fill in:
    
    - Server name: **(localdb)\MSSQLLocalDB**
    
    - Authentication**: **Windows Authentication**

2. Go to File → Open → File and open **DemoData.sql**

3. In the database dropdown at the top, select:
   
    - aspnet-CollegeScheduler-ebb7c0ad-ee39-44a7-ac1d-dafc225dea1e
   
4. Press F5 to execute

**!****!** This database is created automatically when you first run **dotnet run**. Always run the app before loading the demo data.

The script will populate the database with:
    * Admin, Lecturer and Student users
    * Departments, programs, academic years, terms, cohorts
    * Rooms and facilities
    * Modules and recurring timetable events
    * Requests and notifications


**Step 8 — Log In and Test**

Go to **http://localhost:5119** and click Login. Use these credentials:

| Role     | Email                     | Password     |
|----------|--------------------------|--------------|
| Admin    | admin@college.ie         | Admin123!    |
| Lecturer | lecturertest@college.ie  | Lecturer.123 |
| Student  | studenttest@college.ie   | Student.123  |

**Important — Blazor UI (Frontend)**

The frontend interface at **_http://localhost:5119_** is a basic Blazor UI used only for testing user login with the three roles.
All actual functionality — timetable management, scheduling, room bookings, clash detection, notifications, and real-time updates — is accessed and tested through **Swagger** at **_http://localhost:5119/swagger_**.
The Blazor UI does not expose the full system features and should not be used to evaluate the backend functionality.


**Troubleshooting**

  * App fails with a license error on startup MassTransit version is wrong. **Repeat Step 4**.
  * App fails to build with a Pages namespace error Identity pages are missing. **Repeat Step 5**.
  * Unable to connect to web server 'https' Run these commands to fix the SSL certificate:
        
        - dotnet dev-certs https --clean
        
        - dotnet dev-certs https --trust
        
  * Or switch to the http profile in Visual Studio instead of https.
  * RabbitMQ not connecting / Bus not started Make sure Docker Desktop is open and run:
        
        - docker start rabbitmq
        
  * Database not found in SSMS dropdown The app must run at least once before the database is created. Run dotnet run first, then load the demo data.

**Quick Start Checklist**
Every time you open the project, make sure:
    - Docker Desktop is open
    - RabbitMQ is running: **docker start rabbitmq**
    - You are in the correct folder: **CollegeScheduler\CollegeScheduler**
    - **dotnet run** is running in the terminal
    - **DemoData.sql** has been loaded into SSMS (only needed once)
