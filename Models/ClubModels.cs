namespace KUETHardwareAccelerationClub.Models;

public class HomeViewModel
{
    public string ClubName { get; set; } = string.Empty;
    public string IntroText { get; set; } = string.Empty;
    public string Vision { get; set; } = string.Empty;
    public string Mission { get; set; } = string.Empty;
    public List<Achievement> Achievements { get; set; } = [];
}

public class Achievement
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}

public class PersonProfile
{
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string LinkedIn { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}

public class RegisterInput
{
    public string FullName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginInput
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Lead { get; set; } = string.Empty;
    public string Stack { get; set; } = string.Empty;
    public List<Comment> Comments { get; set; } = [];
}

public class Comment
{
    public int Id { get; set; }
    public string AuthorEmail { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public int ReportCount { get; set; }
    public string ModerationStatus { get; set; } = "Visible";
}

public class ProjectsPageViewModel
{
    public List<Project> Projects { get; set; } = [];
    public string CurrentUserEmail { get; set; } = string.Empty;
}

public class EventItem
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DateAndVenue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ContactSubmission
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
}