using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using KUETHardwareAccelerationClub.Models;

namespace KUETHardwareAccelerationClub.Services
{
    public static class ClubRepository
    {
        private static readonly List<string> _members = new List<string>
        {
            "member1@kuet.ac.bd",
            "member2@kuet.ac.bd",
            "member3@kuet.ac.bd"
        };

        private static readonly List<Project> _projects = new List<Project>
        {
            new Project
            {
                Id = 1,
                Title = "FPGA-Based CNN Accelerator",
                Summary = "Designed an FPGA pipeline for low-latency image classification.",
                Lead = "Arafat Hossain",
                Stack = "Verilog, Xilinx Vivado, Python",
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = 1,
                        AuthorEmail = "member1@kuet.ac.bd",
                        Content = "Impressive throughput! Can you share LUT usage details?",
                        CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
                        ModerationStatus = "Visible"
                    }
                }
            },
            new Project
            {
                Id = 2,
                Title = "RISC-V Vector Processing Unit",
                Summary = "Custom vector extension prototype focused on matrix operations.",
                Lead = "Nabila Sultana",
                Stack = "SystemVerilog, C++, RISC-V Toolchain"
            }
        };

        private static readonly List<CommentReport> _reports = new List<CommentReport>();
        private static readonly TimeSpan _duplicateSuppressionWindow = BuildDuplicateSuppressionWindow();

        private static TimeSpan BuildDuplicateSuppressionWindow()
        {
            var configured = ConfigurationManager.AppSettings["DuplicateSuppressionHours"];
            if (double.TryParse(configured, out var hours) && hours > 0)
            {
                return TimeSpan.FromHours(hours);
            }

            return TimeSpan.FromHours(24);
        }

        public static HomeViewModel GetHomeContent()
        {
            return new HomeViewModel
            {
                ClubName = "Hardware Acceleration Club of KUET",
                IntroText = "We empower students to build high-performance systems using FPGA, GPU, and domain-specific architectures.",
                Vision = "To become Bangladesh's leading student platform for hardware acceleration research and innovation.",
                Mission = "Build practical skills, foster research culture, and launch impactful acceleration projects solving real-world challenges.",
                Achievements = new List<Achievement>
                {
                    new Achievement { Title = "National Embedded Contest - Champion", Description = "Secured first place with energy-efficient inference hardware.", Year = "2024" },
                    new Achievement { Title = "Open Hardware Showcase - Best Innovation", Description = "Recognized for custom RISC-V acceleration extension.", Year = "2025" },
                    new Achievement { Title = "Inter-University Hackfest - Top 3", Description = "Developed low-latency traffic analytics accelerator.", Year = "2025" }
                }
            };
        }

        public static List<PersonProfile> GetExecutives()
        {
            return new List<PersonProfile>
            {
                new PersonProfile { Name = "Arafat Hossain", Position = "President", Department = "EEE", Email = "president@hack.kuet.ac.bd", Phone = "+8801XXXXXXXXX", LinkedIn = "https://linkedin.com", ImageUrl = "https://placehold.co/300x300" },
                new PersonProfile { Name = "Nabila Sultana", Position = "General Secretary", Department = "CSE", Email = "secretary@hack.kuet.ac.bd", Phone = "+8801XXXXXXXXX", LinkedIn = "https://linkedin.com", ImageUrl = "https://placehold.co/300x300" },
                new PersonProfile { Name = "Tanvir Hasan", Position = "Technical Lead", Department = "ECE", Email = "techlead@hack.kuet.ac.bd", Phone = "+8801XXXXXXXXX", LinkedIn = "https://linkedin.com", ImageUrl = "https://placehold.co/300x300" }
            };
        }

        public static List<PersonProfile> GetAdvisors()
        {
            return new List<PersonProfile>
            {
                new PersonProfile { Name = "Dr. Farhana Rahman", Position = "Faculty Advisor", Department = "ECE", Email = "farhana.rahman@kuet.ac.bd", Phone = "+8801XXXXXXXXX", LinkedIn = "https://kuet.ac.bd", ImageUrl = "https://placehold.co/300x300" },
                new PersonProfile { Name = "Dr. Saifuddin Ahmed", Position = "Co-Advisor", Department = "CSE", Email = "saifuddin.ahmed@kuet.ac.bd", Phone = "+8801XXXXXXXXX", LinkedIn = "https://kuet.ac.bd", ImageUrl = "https://placehold.co/300x300" }
            };
        }

        public static ProjectsPageViewModel GetProjectsPage(string currentUserEmail)
        {
            return new ProjectsPageViewModel
            {
                Projects = _projects,
                CurrentUserEmail = currentUserEmail
            };
        }

        public static bool IsMember(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && _members.Contains(email.Trim().ToLowerInvariant());
        }

        public static void AddComment(int projectId, string authorEmail, string content)
        {
            var project = _projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null || string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            var nextId = _projects.SelectMany(p => p.Comments).DefaultIfEmpty(new Comment { Id = 0 }).Max(c => c.Id) + 1;
            project.Comments.Add(new Comment
            {
                Id = nextId,
                AuthorEmail = authorEmail,
                Content = content.Trim(),
                CreatedAtUtc = DateTime.UtcNow,
                ModerationStatus = "Visible"
            });
        }

        public static (bool success, string message) ReportComment(int commentId, string memberEmail, string reason)
        {
            if (!IsMember(memberEmail))
            {
                return (false, "Only registered members can report comments.");
            }

            var comment = _projects.SelectMany(p => p.Comments).FirstOrDefault(c => c.Id == commentId);
            if (comment == null)
            {
                return (false, "Comment not found.");
            }

            var normalizedReason = (reason ?? "Other").Trim().ToLowerInvariant();
            var fingerprint = $"{commentId}:{normalizedReason}";
            var utcNow = DateTime.UtcNow;
            var duplicateExists = _reports.Any(r =>
                r.MemberEmail == memberEmail.Trim().ToLowerInvariant() &&
                r.CommentId == commentId &&
                r.ContentFingerprint == fingerprint &&
                utcNow - r.CreatedAtUtc <= _duplicateSuppressionWindow);

            _reports.Add(new CommentReport
            {
                CommentId = commentId,
                MemberEmail = memberEmail.Trim().ToLowerInvariant(),
                ReasonCode = reason,
                ContentFingerprint = fingerprint,
                CreatedAtUtc = utcNow,
                IsSuppressedDuplicate = duplicateExists
            });

            if (duplicateExists)
            {
                return (false, "Duplicate report blocked. You already reported this recently.");
            }

            comment.ReportCount += 1;
            if (comment.ReportCount >= 3)
            {
                comment.ModerationStatus = "UnderReview";
            }

            return (true, "Report submitted successfully.");
        }

        public static void RegisterMember(string email)
        {
            var normalized = email?.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(normalized) && !_members.Contains(normalized))
            {
                _members.Add(normalized);
            }
        }
    }
}
