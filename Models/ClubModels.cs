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
    public string ProfileKey { get; set; } = string.Empty;
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

public class MemberDirectoryItem
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class DashboardPersonItem
{
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
}

public class AdminProfileTarget
{
    public string ProfileKey { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class AdminBroadcastInput
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Audience { get; set; } = "All";
    public List<string> SelectedRecipients { get; set; } = [];
}

public class AdminAnnouncementItem
{
    public string Subject { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int RecipientCount { get; set; }
    public string BodyPreview { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}

public class ContactSubmission
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
}

public class AdminDashboardViewModel
{
    public List<DashboardStat> Stats { get; set; } = [];
    public List<DashboardPersonItem> People { get; set; } = [];
    public List<AdminProfileTarget> ProfileTargets { get; set; } = [];
    public List<AdminAnnouncementItem> RecentAnnouncements { get; set; } = [];
    public List<AdminCommentItem> RecentComments { get; set; } = [];
    public List<AdminContactItem> RecentContacts { get; set; } = [];
    public List<AdminRegistrationItem> RecentRegistrations { get; set; } = [];
    public string LiveUpdatedAtUtc { get; set; } = string.Empty;
}

public class DashboardStat
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Subtext { get; set; } = string.Empty;
}

public class AdminCommentItem
{
    public int Id { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public string ModerationStatus { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class AdminContactItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
}

public class AdminRegistrationItem
{
    public string EventName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public DateTime RegisteredAtUtc { get; set; }
}