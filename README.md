College Scheduler
Overview

The College Scheduler is a full-stack web application designed to manage academic timetables, room allocation, and scheduling workflows within a college environment.

The system replaces manual scheduling methods such as spreadsheets and static documents with a structured and centralized platform that improves accuracy, efficiency and visibility.

It aims to:

Prevent timetable conflicts
Automate scheduling processes
Support multiple user roles
Provide real-time updates and notifications
Project Value

This system improves scheduling efficiency by eliminating manual processes and reducing human error. It provides a centralized solution where administrators, lecturers, and students can interact with timetable data in a controlled and consistent way.

The project demonstrates the practical application of modern software engineering concepts such as API-driven design, role-based access control, and scalable system architecture.

Key Features
Admin
Manage campuses, buildings, rooms, and modules
Create and manage timetable events
Search for available rooms using filters
Detect scheduling clashes before creating events
Generate recurring timetable events for full academic terms
Review and manage requests through an approval workflow

Lecturer
View personal timetable
Submit schedule change requests
Submit room booking requests

Student
View personal timetable
Submit room booking requests
Track request status
Receive notifications

Core Functionality
Room availability search
Clash detection (room, lecturer, and cohort)
Recurring event generation
Request and approval workflow
Notification system (structure implemented)

System Flow
Admin creates timetable events or recurring schedules
The system validates inputs and checks for clashes
Events are stored in the database
Lecturers and students retrieve their timetables through the API
Users submit requests such as rescheduling or room bookings
Requests go through an approval process
Notifications are generated and delivered to users

This workflow ensures data consistency, prevents conflicts and supports controlled scheduling operations.

Architecture

The project follows Clean Architecture principles to ensure separation of concerns, maintainability and scalability.

Presentation Layer → Blazor Server UI
Application Layer → Business logic and services
Domain Layer → Core entities and rules
Infrastructure Layer → Database access and external integrations

The backend is designed as an API-first system, meaning all functionality is exposed through REST endpoints and tested independently before frontend integration.

This structure allows different parts of the system to evolve independently and supports future expansion.

Technologies Used
.NET 8 / ASP.NET Core
Blazor Server
Entity Framework Core
SQL Server (LocalDB)
ASP.NET Identity (authentication and roles)
SignalR (real-time updates)
RabbitMQ with MassTransit (asynchronous messaging)
Swagger (API testing)

Technology Roles
SignalR enables real-time notifications when timetable changes or request updates occur.
RabbitMQ enables asynchronous communication between components, supporting scalability and decoupled system design.
Swagger allows testing and validation of API endpoints independently of the frontend.

User Roles
Role	Description
Admin	Manages scheduling, rooms, modules, and approves or rejects requests
Lecturer	Views assigned timetable and submits schedule or booking requests
Student	Views timetable and submits room booking requests
API Overview

The backend exposes RESTful endpoints covering all system functionality.

Examples:

GET /api/v1/admin/scheduling/rooms/available → Find available rooms
POST /api/v1/admin/scheduling/check-clashes → Validate timetable slot
POST /api/v1/admin/scheduling/recurring-events → Create recurring events
GET /api/v1/lecturer/timetable → Retrieve lecturer timetable
GET /api/v1/student/timetable → Retrieve student timetable

If run locally 
Swagger UI is available at:

http://localhost:5119/swagger
or
https://localhost:7209/swagger

Swagger is used to test and verify backend functionality before interacting with the frontend.

Setup and Installation to run locally

A full setup guide is available in:

SETUP.md

Hosting

The application is deployed and accessible online:

http://collegescheduler.runasp.net/

Swagger (API testing):

http://collegescheduler.runasp.net/swagger/index.html

The system can be tested directly through API endpoints.

Demo Credentials
Role	Email	Password
Admin	admin@college.ie	Admin123!
Lecturer	lecturertest@college.ie	Lecturer.123
Student	studenttest@college.ie	Student.123

Notes
The Blazor UI currently provides a basic interface focused on authentication and structure.
Full system functionality is accessible through the API using Swagger.
The project prioritizes backend design, correctness, and architecture over full UI implementation.
Project Status
Backend fully implemented and tested
Scheduling system completed
Request workflows implemented
API endpoints validated through Swagger
Frontend partially implemented
Conclusion

The College Scheduler demonstrates a structured and scalable approach to solving scheduling challenges in an academic environment. It combines a layered architecture, role-based access, and API-driven design to deliver a reliable and extensible system.

The project reflects real-world software development practices and provides a strong foundation for future improvements such as enhanced user interfaces and extended integrations.
