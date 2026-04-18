using System;
using System.Collections.Generic;

namespace KUETHardwareAccelerationClub.Models
{
    public class HomeViewModel
    {
        public string ClubName { get; set; }
        public string IntroText { get; set; }
        public string Vision { get; set; }
        public string Mission { get; set; }
        public List<Achievement> Achievements { get; set; } = new List<Achievement>();
    }

    public class Achievement
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Year { get; set; }
    }

    public class PersonProfile
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string LinkedIn { get; set; }
        public string ImageUrl { get; set; }
    }

    public class AuthPageViewModel
    {
        public RegisterInput Register { get; set; } = new RegisterInput();
        public LoginInput Login { get; set; } = new LoginInput();
    }

    public class RegisterInput
    {
        public string FullName { get; set; }
        public string StudentId { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Password { get; set; }
    }

    public class LoginInput
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ProjectsPageViewModel
    {
        public List<Project> Projects { get; set; } = new List<Project>();
        public string CurrentUserEmail { get; set; }
    }

    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Lead { get; set; }
        public string Stack { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }

    public class Comment
    {
        public int Id { get; set; }
        public string AuthorEmail { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int ReportCount { get; set; }
        public string ModerationStatus { get; set; }
    }

    public class CommentReport
    {
        public int CommentId { get; set; }
        public string MemberEmail { get; set; }
        public string ReasonCode { get; set; }
        public string ContentFingerprint { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public bool IsSuppressedDuplicate { get; set; }
    }
}
