# Hardware Acceleration Club of KUET (ASP.NET MVC)

This project is now an ASP.NET MVC dynamic website with a modern vibrant UI.

## Dynamic pages

- Home + Newsletter subscription
- About
- Projects + comments + reporting moderation flow
- Events + member registration
- Meet the Team
- Advisors
- Contact + inquiry submission
- Register/Login + session-based auth state

## Stack

- ASP.NET Core MVC
- Razor Views
- In-memory repository service (demo data)
- Session for lightweight member auth state
- Custom vibrant UI in [wwwroot/css/site.css](wwwroot/css/site.css)

## Project structure

- [Program.cs](Program.cs)
- [Controllers](Controllers)
- [Models](Models)
- [Services](Services)
- [Views](Views)
- [wwwroot](wwwroot)

## Run

1. Install .NET SDK 8+
2. Run:
	- `dotnet restore`
	- `dotnet run`
3. Open the local URL shown in terminal
