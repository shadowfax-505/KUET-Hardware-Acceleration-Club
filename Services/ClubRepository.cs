using KUETHardwareAccelerationClub.Models;
using Microsoft.Data.Sqlite;

namespace KUETHardwareAccelerationClub.Services;

public static class ClubRepository
{
    private static readonly object SyncRoot = new();
    private static bool _initialized;

    private static readonly string DatabaseDirectory = Path.Combine(AppContext.BaseDirectory, "App_Data");
    private static readonly string DatabasePath = Path.Combine(DatabaseDirectory, "club.db");
    private static string ConnectionString => new SqliteConnectionStringBuilder { DataSource = DatabasePath }.ToString();

    public static HomeViewModel GetHomeContent() => new()
    {
        ClubName = "Hardware Acceleration Club of KUET",
        IntroText = "We empower students to build high-performance systems using FPGA, GPU, and domain-specific architectures.",
        Vision = "To become Bangladesh's leading student platform for hardware acceleration research and innovation.",
        Mission = "Build practical skills, foster research culture, and launch impactful acceleration projects solving real-world challenges.",
        Achievements =
        [
            new Achievement { Year = "2024", Title = "National Embedded Contest - Champion", Description = "Secured first place with energy-efficient inference hardware." },
            new Achievement { Year = "2025", Title = "Open Hardware Showcase - Best Innovation", Description = "Recognized for custom RISC-V acceleration extension." },
            new Achievement { Year = "2025", Title = "Inter-University Hackfest - Top 3", Description = "Developed low-latency traffic analytics accelerator." }
        ]
    };

    public static List<PersonProfile> GetExecutives()
    {
        EnsureInitialized();

        var profiles = new List<PersonProfile>
        {
            new() { ProfileKey = "executive:president@hack.kuet.ac.bd", Name = "Arafat Hossain", Position = "President", Department = "EEE", Email = "president@hack.kuet.ac.bd", Phone = "+8801XXXXXXXXX", LinkedIn = "https://linkedin.com", ImageUrl = "https://placehold.co/300x300", Bio = "Leads strategic planning, partnerships, and annual roadmap." },
            new() { ProfileKey = "executive:secretary@hack.kuet.ac.bd", Name = "Nabila Sultana", Position = "General Secretary", Department = "CSE", Email = "secretary@hack.kuet.ac.bd", Phone = "+8801XXXXXXXXX", LinkedIn = "https://linkedin.com", ImageUrl = "https://placehold.co/300x300", Bio = "Coordinates operations and cross-team execution." },
            new() { ProfileKey = "executive:techlead@hack.kuet.ac.bd", Name = "Tanvir Hasan", Position = "Technical Lead", Department = "ECE", Email = "techlead@hack.kuet.ac.bd", Phone = "+8801XXXXXXXXX", LinkedIn = "https://linkedin.com", ImageUrl = "https://placehold.co/300x300", Bio = "Owns workshop curriculum and project mentoring." }
        };

        return ApplyProfileImageOverrides("executive", profiles);
    }

    public static List<PersonProfile> GetAdvisors()
    {
        EnsureInitialized();

        var profiles = new List<PersonProfile>
        {
            new() { ProfileKey = "advisor:farhana.rahman@kuet.ac.bd", Name = "Dr. Farhana Rahman", Position = "Faculty Advisor", Department = "ECE", Email = "farhana.rahman@kuet.ac.bd", Phone = "+8801XXXXXXXXX", LinkedIn = "https://kuet.ac.bd", ImageUrl = "https://placehold.co/300x300", Bio = "Mentors VLSI/FPGA research initiatives." },
            new() { ProfileKey = "advisor:saifuddin.ahmed@kuet.ac.bd", Name = "Dr. Saifuddin Ahmed", Position = "Co-Advisor", Department = "CSE", Email = "saifuddin.ahmed@kuet.ac.bd", Phone = "+8801XXXXXXXXX", LinkedIn = "https://kuet.ac.bd", ImageUrl = "https://placehold.co/300x300", Bio = "Guides systems and benchmark methodology." }
        };

        return ApplyProfileImageOverrides("advisor", profiles);
    }

    public static List<AdminProfileTarget> GetProfileTargets()
    {
        EnsureInitialized();

        var targets = new List<AdminProfileTarget>();
        targets.AddRange(GetExecutives().Select(profile => new AdminProfileTarget
        {
            ProfileKey = profile.ProfileKey,
            Group = "Meet the Team",
            Name = profile.Name,
            Email = profile.Email,
            ImageUrl = profile.ImageUrl
        }));

        targets.AddRange(GetAdvisors().Select(profile => new AdminProfileTarget
        {
            ProfileKey = profile.ProfileKey,
            Group = "Advisors",
            Name = profile.Name,
            Email = profile.Email,
            ImageUrl = profile.ImageUrl
        }));

        return targets;
    }

    public static List<MemberDirectoryItem> GetMemberDirectory()
    {
        EnsureInitialized();
        var items = new List<MemberDirectoryItem>();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT FullName, Email, StudentId, Department, IFNULL(ImageUrl, ''), IFNULL(GitHubProfile, '')
            FROM Members
            ORDER BY FullName COLLATE NOCASE;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new MemberDirectoryItem
            {
                FullName = reader.GetString(0),
                Email = reader.GetString(1),
                StudentId = reader.GetString(2),
                Department = reader.GetString(3),
                ImageUrl = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                GitHubProfile = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
            });
        }

        return items;
    }

    public static ProjectsPageViewModel GetProjectsPage(string currentUserEmail)
    {
        EnsureInitialized();
        var projects = new List<Project>();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT p.Id, p.Title, p.Summary, p.Lead, p.Stack,
                   c.Id, c.AuthorEmail, c.Content, c.CreatedAtUtc, c.ReportCount, c.ModerationStatus
            FROM Projects p
            LEFT JOIN Comments c ON c.ProjectId = p.Id AND c.ModerationStatus <> 'Hidden'
            ORDER BY p.Id, c.CreatedAtUtc DESC;";

        using var reader = command.ExecuteReader();
        Project? currentProject = null;
        var currentProjectId = -1;

        while (reader.Read())
        {
            var projectId = reader.GetInt32(0);
            if (currentProject is null || currentProjectId != projectId)
            {
                currentProject = new Project
                {
                    Id = projectId,
                    Title = reader.GetString(1),
                    Summary = reader.GetString(2),
                    Lead = reader.GetString(3),
                    Stack = reader.GetString(4),
                    Comments = []
                };

                projects.Add(currentProject);
                currentProjectId = projectId;
            }

            if (!reader.IsDBNull(5) && currentProject is not null)
            {
                currentProject.Comments.Add(new Comment
                {
                    Id = reader.GetInt32(5),
                    AuthorEmail = reader.GetString(6),
                    Content = reader.GetString(7),
                    CreatedAtUtc = DateTime.Parse(reader.GetString(8)),
                    ReportCount = reader.GetInt32(9),
                    ModerationStatus = reader.GetString(10)
                });
            }
        }

        return new ProjectsPageViewModel
        {
            Projects = projects,
            CurrentUserEmail = currentUserEmail
        };
    }

    public static (bool success, string message) RegisterMember(RegisterInput input, string? imageUrl = null)
    {
        EnsureInitialized();
        var email = Normalize(input.Email);
        var gitHubProfile = NormalizeGitHubProfile(input.GitHubProfile);
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(input.Password))
        {
            return (false, "Invalid registration request.");
        }

        if (!string.IsNullOrWhiteSpace(input.GitHubProfile) && string.IsNullOrWhiteSpace(gitHubProfile))
        {
            return (false, "Please enter a valid GitHub profile URL.");
        }

        using var connection = OpenConnection();
        if (MemberExists(connection, email))
        {
            return (false, "This email is already registered.");
        }

        using var transaction = connection.BeginTransaction();

        using (var memberCommand = connection.CreateCommand())
        {
            memberCommand.Transaction = transaction;
            memberCommand.CommandText = "INSERT INTO Members (Email, FullName, StudentId, Department, ImageUrl, GitHubProfile) VALUES ($email, $fullName, $studentId, $department, $imageUrl, $gitHubProfile);";
            memberCommand.Parameters.AddWithValue("$email", email);
            memberCommand.Parameters.AddWithValue("$fullName", input.FullName.Trim());
            memberCommand.Parameters.AddWithValue("$studentId", input.StudentId.Trim());
            memberCommand.Parameters.AddWithValue("$department", input.Department.Trim());
            memberCommand.Parameters.AddWithValue("$imageUrl", imageUrl ?? string.Empty);
            memberCommand.Parameters.AddWithValue("$gitHubProfile", gitHubProfile);
            memberCommand.ExecuteNonQuery();
        }

        using (var credentialCommand = connection.CreateCommand())
        {
            credentialCommand.Transaction = transaction;
            credentialCommand.CommandText = "INSERT INTO Credentials (Email, Password) VALUES ($email, $password);";
            credentialCommand.Parameters.AddWithValue("$email", email);
            credentialCommand.Parameters.AddWithValue("$password", input.Password.Trim());
            credentialCommand.ExecuteNonQuery();
        }

        transaction.Commit();
        return (true, "Registration successful.");
    }

    public static bool ValidateLogin(LoginInput input)
    {
        EnsureInitialized();
        var email = Normalize(input.Email);

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Password FROM Credentials WHERE Email = $email LIMIT 1;";
        command.Parameters.AddWithValue("$email", email);

        var password = command.ExecuteScalar() as string;
        return password is not null && password == input.Password.Trim();
    }

    public static bool IsMember(string? email)
    {
        EnsureInitialized();
        var normalized = Normalize(email);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        using var connection = OpenConnection();
        return MemberExists(connection, normalized);
    }

    public static void AddComment(int projectId, string authorEmail, string content)
    {
        EnsureInitialized();
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Comments (ProjectId, AuthorEmail, Content, CreatedAtUtc, ReportCount, ModerationStatus)
            VALUES ($projectId, $authorEmail, $content, $createdAtUtc, 0, 'Visible');";
        command.Parameters.AddWithValue("$projectId", projectId);
        command.Parameters.AddWithValue("$authorEmail", Normalize(authorEmail));
        command.Parameters.AddWithValue("$content", content.Trim());
        command.Parameters.AddWithValue("$createdAtUtc", DateTime.UtcNow.ToString("O"));
        command.ExecuteNonQuery();
    }

    public static (bool success, string message) ReportComment(int commentId, string email, string reason)
    {
        EnsureInitialized();
        if (!IsMember(email))
        {
            return (false, "Only registered members can report comments.");
        }

        var normalizedEmail = Normalize(email);
        var normalizedReason = (reason ?? string.Empty).Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;

        using var connection = OpenConnection();
        using var duplicateCommand = connection.CreateCommand();
        duplicateCommand.CommandText = @"
            SELECT COUNT(1)
            FROM CommentReports
            WHERE CommentId = $commentId
              AND MemberEmail = $memberEmail
              AND Reason = $reason
              AND CreatedAtUtc >= $windowStart;";
        duplicateCommand.Parameters.AddWithValue("$commentId", commentId);
        duplicateCommand.Parameters.AddWithValue("$memberEmail", normalizedEmail);
        duplicateCommand.Parameters.AddWithValue("$reason", normalizedReason);
        duplicateCommand.Parameters.AddWithValue("$windowStart", now.AddHours(-24).ToString("O"));

        var duplicateExists = Convert.ToInt32(duplicateCommand.ExecuteScalar()) > 0;

        using (var reportCommand = connection.CreateCommand())
        {
            reportCommand.CommandText = @"
                INSERT INTO CommentReports (CommentId, MemberEmail, Reason, CreatedAtUtc, IsDuplicate)
                VALUES ($commentId, $memberEmail, $reason, $createdAtUtc, $isDuplicate);";
            reportCommand.Parameters.AddWithValue("$commentId", commentId);
            reportCommand.Parameters.AddWithValue("$memberEmail", normalizedEmail);
            reportCommand.Parameters.AddWithValue("$reason", normalizedReason);
            reportCommand.Parameters.AddWithValue("$createdAtUtc", now.ToString("O"));
            reportCommand.Parameters.AddWithValue("$isDuplicate", duplicateExists ? 1 : 0);
            reportCommand.ExecuteNonQuery();
        }

        if (duplicateExists)
        {
            return (false, "Duplicate report blocked. You already reported this recently.");
        }

        using var updateCommand = connection.CreateCommand();
        updateCommand.CommandText = @"
            UPDATE Comments
            SET ReportCount = ReportCount + 1,
                ModerationStatus = CASE WHEN ReportCount + 1 >= 3 THEN 'UnderReview' ELSE ModerationStatus END
            WHERE Id = $commentId;";
        updateCommand.Parameters.AddWithValue("$commentId", commentId);
        var affected = updateCommand.ExecuteNonQuery();

        if (affected == 0)
        {
            return (false, "Comment not found.");
        }

        return (true, "Report submitted successfully.");
    }

    public static List<EventItem> GetEvents()
    {
        EnsureInitialized();
        var events = new List<EventItem>();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Type, Name, DateAndVenue, Description FROM Events ORDER BY Id;";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            events.Add(new EventItem
            {
                Id = reader.GetInt32(0),
                Type = reader.GetString(1),
                Name = reader.GetString(2),
                DateAndVenue = reader.GetString(3),
                Description = reader.GetString(4)
            });
        }

        return events;
    }

    public static List<int> GetRegisteredEventIds(string email)
    {
        EnsureInitialized();
        var normalizedEmail = Normalize(email);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return [];
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EventId
            FROM EventRegistrations
            WHERE MemberEmail = $email;";
        command.Parameters.AddWithValue("$email", normalizedEmail);

        var eventIds = new List<int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            eventIds.Add(reader.GetInt32(0));
        }

        return eventIds;
    }

    public static (bool success, string message) RegisterForEvent(int eventId, string email)
    {
        EnsureInitialized();
        if (!IsMember(email))
        {
            return (false, "Please login as a member to register.");
        }

        var normalizedEmail = Normalize(email);
        using var connection = OpenConnection();

        using var existsCommand = connection.CreateCommand();
        existsCommand.CommandText = "SELECT COUNT(1) FROM EventRegistrations WHERE EventId = $eventId AND MemberEmail = $email;";
        existsCommand.Parameters.AddWithValue("$eventId", eventId);
        existsCommand.Parameters.AddWithValue("$email", normalizedEmail);
        if (Convert.ToInt32(existsCommand.ExecuteScalar()) > 0)
        {
            return (false, "You are already registered for this event.");
        }

        using var eventExistsCommand = connection.CreateCommand();
        eventExistsCommand.CommandText = "SELECT COUNT(1) FROM Events WHERE Id = $eventId;";
        eventExistsCommand.Parameters.AddWithValue("$eventId", eventId);
        if (Convert.ToInt32(eventExistsCommand.ExecuteScalar()) == 0)
        {
            return (false, "Event not found.");
        }

        using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = @"
            INSERT INTO EventRegistrations (EventId, MemberEmail, RegisteredAtUtc)
            VALUES ($eventId, $email, $registeredAtUtc);";
        insertCommand.Parameters.AddWithValue("$eventId", eventId);
        insertCommand.Parameters.AddWithValue("$email", normalizedEmail);
        insertCommand.Parameters.AddWithValue("$registeredAtUtc", DateTime.UtcNow.ToString("O"));
        insertCommand.ExecuteNonQuery();

        return (true, "Event registration successful.");
    }

    public static (bool success, string message) AddEvent(AdminEventInput input)
    {
        EnsureInitialized();
        if (string.IsNullOrWhiteSpace(input.Type) || string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.DateAndVenue) || string.IsNullOrWhiteSpace(input.Description))
        {
            return (false, "All event fields are required.");
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Events (Type, Name, DateAndVenue, Description)
            VALUES ($type, $name, $dateAndVenue, $description);";
        command.Parameters.AddWithValue("$type", input.Type.Trim());
        command.Parameters.AddWithValue("$name", input.Name.Trim());
        command.Parameters.AddWithValue("$dateAndVenue", input.DateAndVenue.Trim());
        command.Parameters.AddWithValue("$description", input.Description.Trim());
        var affected = command.ExecuteNonQuery();

        return affected > 0
            ? (true, "Event added successfully.")
            : (false, "Failed to add event.");
    }

    public static (bool success, string message) UpdateEvent(AdminEventInput input)
    {
        EnsureInitialized();
        if (input.Id <= 0)
        {
            return (false, "Invalid event.");
        }

        if (string.IsNullOrWhiteSpace(input.Type) || string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.DateAndVenue) || string.IsNullOrWhiteSpace(input.Description))
        {
            return (false, "All event fields are required.");
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Events
            SET Type = $type,
                Name = $name,
                DateAndVenue = $dateAndVenue,
                Description = $description
            WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", input.Id);
        command.Parameters.AddWithValue("$type", input.Type.Trim());
        command.Parameters.AddWithValue("$name", input.Name.Trim());
        command.Parameters.AddWithValue("$dateAndVenue", input.DateAndVenue.Trim());
        command.Parameters.AddWithValue("$description", input.Description.Trim());
        var affected = command.ExecuteNonQuery();

        return affected > 0
            ? (true, "Event updated successfully.")
            : (false, "Event not found.");
    }

    public static (bool success, string message) DeleteEvent(int eventId)
    {
        EnsureInitialized();
        if (eventId <= 0)
        {
            return (false, "Invalid event.");
        }

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        using (var registrationsCommand = connection.CreateCommand())
        {
            registrationsCommand.Transaction = transaction;
            registrationsCommand.CommandText = "DELETE FROM EventRegistrations WHERE EventId = $eventId;";
            registrationsCommand.Parameters.AddWithValue("$eventId", eventId);
            registrationsCommand.ExecuteNonQuery();
        }

        using var eventCommand = connection.CreateCommand();
        eventCommand.Transaction = transaction;
        eventCommand.CommandText = "DELETE FROM Events WHERE Id = $eventId;";
        eventCommand.Parameters.AddWithValue("$eventId", eventId);
        var affected = eventCommand.ExecuteNonQuery();

        transaction.Commit();

        return affected > 0
            ? (true, "Event deleted successfully.")
            : (false, "Event not found.");
    }

    public static (bool success, string message) SubscribeNewsletter(string email)
    {
        EnsureInitialized();
        var normalizedEmail = Normalize(email);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return (false, "Please enter a valid email.");
        }

        using var connection = OpenConnection();
        using var existsCommand = connection.CreateCommand();
        existsCommand.CommandText = "SELECT COUNT(1) FROM NewsletterSubscribers WHERE Email = $email;";
        existsCommand.Parameters.AddWithValue("$email", normalizedEmail);
        if (Convert.ToInt32(existsCommand.ExecuteScalar()) > 0)
        {
            return (false, "This email is already subscribed.");
        }

        using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = "INSERT INTO NewsletterSubscribers (Email, SubscribedAtUtc) VALUES ($email, $createdAtUtc);";
        insertCommand.Parameters.AddWithValue("$email", normalizedEmail);
        insertCommand.Parameters.AddWithValue("$createdAtUtc", DateTime.UtcNow.ToString("O"));
        insertCommand.ExecuteNonQuery();
        return (true, "Subscribed successfully.");
    }

    public static (bool success, string message) SubmitContact(ContactSubmission submission)
    {
        EnsureInitialized();
        if (string.IsNullOrWhiteSpace(submission.Name) || string.IsNullOrWhiteSpace(submission.Email) || string.IsNullOrWhiteSpace(submission.Topic) || string.IsNullOrWhiteSpace(submission.Message))
        {
            return (false, "Please complete all fields.");
        }

        using var connection = OpenConnection();
        using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = @"
            INSERT INTO ContactSubmissions (Name, Email, Topic, Message, SubmittedAtUtc)
            VALUES ($name, $email, $topic, $message, $submittedAtUtc);";
        insertCommand.Parameters.AddWithValue("$name", submission.Name.Trim());
        insertCommand.Parameters.AddWithValue("$email", Normalize(submission.Email));
        insertCommand.Parameters.AddWithValue("$topic", submission.Topic.Trim());
        insertCommand.Parameters.AddWithValue("$message", submission.Message.Trim());
        insertCommand.Parameters.AddWithValue("$submittedAtUtc", DateTime.UtcNow.ToString("O"));
        insertCommand.ExecuteNonQuery();

        return (true, "Message sent successfully.");
    }

    public static AdminDashboardViewModel GetAdminDashboard()
    {
        EnsureInitialized();
        var dashboard = new AdminDashboardViewModel();

        using var connection = OpenConnection();
        dashboard.Stats = new List<DashboardStat>
        {
            new() { Label = "Members", Value = Count(connection, "Members"), Subtext = "Registered users" },
            new() { Label = "Projects", Value = Count(connection, "Projects"), Subtext = "Showcased work" },
            new() { Label = "Comments", Value = Count(connection, "Comments"), Subtext = "Member discussions" },
            new() { Label = "Events", Value = Count(connection, "Events"), Subtext = "Upcoming sessions" },
            new() { Label = "Registrations", Value = Count(connection, "EventRegistrations"), Subtext = "Event sign-ups" },
            new() { Label = "Contacts", Value = Count(connection, "ContactSubmissions"), Subtext = "Inbound inquiries" },
            new() { Label = "Subscribers", Value = Count(connection, "NewsletterSubscribers"), Subtext = "Newsletter audience" }
        };

        dashboard.ProfileTargets = GetProfileTargets();
        dashboard.Events = GetEvents();
        dashboard.EventRegistrationsByEvent = GetEventRegistrationsByEvent(connection);
        dashboard.RecentAnnouncements = GetRecentAnnouncements(connection);
        dashboard.RecentComments = GetRecentComments(connection);
        dashboard.RecentContacts = GetRecentContacts(connection);
        dashboard.RecentRegistrations = GetRecentRegistrations(connection);

        return dashboard;
    }

    public static List<PersonProfile> ApplyProfileImageOverrides(string roleKey, List<PersonProfile> profiles)
    {
        EnsureInitialized();

        using var connection = OpenConnection();
        var imageOverrides = GetProfileImageOverrides(connection);
        var profileOverrides = GetProfileOverrides(connection);

        foreach (var profile in profiles)
        {
            var profileKey = string.IsNullOrWhiteSpace(profile.ProfileKey) ? BuildProfileKey(roleKey, profile.Email) : profile.ProfileKey;
            if (imageOverrides.TryGetValue(profileKey, out var imageUrl))
            {
                profile.ImageUrl = imageUrl;
            }

            if (profileOverrides.TryGetValue(profileKey, out var overrideEntry))
            {
                if (!string.IsNullOrWhiteSpace(overrideEntry.Name)) profile.Name = overrideEntry.Name;
                if (!string.IsNullOrWhiteSpace(overrideEntry.Email)) profile.Email = overrideEntry.Email;
                if (!string.IsNullOrWhiteSpace(overrideEntry.ImageUrl)) profile.ImageUrl = overrideEntry.ImageUrl;
            }
        }

        return profiles;
    }

    public static (bool success, string message) SaveAnnouncement(string subject, string body, string audience, IEnumerable<string> recipients)
    {
        EnsureInitialized();

        var normalizedSubject = (subject ?? string.Empty).Trim();
        var normalizedBody = (body ?? string.Empty).Trim();
        var normalizedAudience = (audience ?? string.Empty).Trim();
        var recipientList = recipients
            .Select(Normalize)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (string.IsNullOrWhiteSpace(normalizedSubject) || string.IsNullOrWhiteSpace(normalizedBody))
        {
            return (false, "Subject and message are required.");
        }

        if (recipientList.Count == 0)
        {
            return (false, "Please choose at least one member.");
        }

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        long announcementId;
        using (var insertAnnouncement = connection.CreateCommand())
        {
            insertAnnouncement.Transaction = transaction;
            insertAnnouncement.CommandText = @"
                INSERT INTO EmailAnnouncements (Subject, Body, Audience, RecipientCount, SentAtUtc)
                VALUES ($subject, $body, $audience, $recipientCount, $sentAtUtc);";
            insertAnnouncement.Parameters.AddWithValue("$subject", normalizedSubject);
            insertAnnouncement.Parameters.AddWithValue("$body", normalizedBody);
            insertAnnouncement.Parameters.AddWithValue("$audience", normalizedAudience);
            insertAnnouncement.Parameters.AddWithValue("$recipientCount", recipientList.Count);
            insertAnnouncement.Parameters.AddWithValue("$sentAtUtc", DateTime.UtcNow.ToString("O"));
            insertAnnouncement.ExecuteNonQuery();
        }

        using (var idCommand = connection.CreateCommand())
        {
            idCommand.Transaction = transaction;
            idCommand.CommandText = "SELECT last_insert_rowid();";
            announcementId = Convert.ToInt64(idCommand.ExecuteScalar());
        }

        foreach (var recipient in recipientList)
        {
            using var recipientCommand = connection.CreateCommand();
            recipientCommand.Transaction = transaction;
            recipientCommand.CommandText = @"
                INSERT INTO EmailAnnouncementRecipients (AnnouncementId, RecipientEmail)
                VALUES ($announcementId, $recipientEmail);";
            recipientCommand.Parameters.AddWithValue("$announcementId", announcementId);
            recipientCommand.Parameters.AddWithValue("$recipientEmail", recipient);
            recipientCommand.ExecuteNonQuery();
        }

        transaction.Commit();
        return (true, $"Broadcast queued for {recipientList.Count} member{(recipientList.Count == 1 ? string.Empty : "s")}." );
    }

    public static (bool success, string message) SetProfileImageOverride(string profileKey, string imageUrl)
    {
        EnsureInitialized();

        var normalizedProfileKey = (profileKey ?? string.Empty).Trim().ToLowerInvariant();
        var normalizedImageUrl = (imageUrl ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedProfileKey) || string.IsNullOrWhiteSpace(normalizedImageUrl))
        {
            return (false, "Profile image could not be saved.");
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO ProfileImageOverrides (ProfileKey, ImageUrl, UpdatedAtUtc)
            VALUES ($profileKey, $imageUrl, $updatedAtUtc)
            ON CONFLICT(ProfileKey) DO UPDATE SET
                ImageUrl = excluded.ImageUrl,
                UpdatedAtUtc = excluded.UpdatedAtUtc;";
        command.Parameters.AddWithValue("$profileKey", normalizedProfileKey);
        command.Parameters.AddWithValue("$imageUrl", normalizedImageUrl);
        command.Parameters.AddWithValue("$updatedAtUtc", DateTime.UtcNow.ToString("O"));

        return command.ExecuteNonQuery() > 0
            ? (true, "Profile image updated.")
            : (false, "Profile image could not be saved.");
    }

    public static bool SetCommentModerationStatus(int commentId, string moderationStatus)
    {
        EnsureInitialized();
        var normalizedStatus = (moderationStatus ?? string.Empty).Trim();
        if (normalizedStatus is not ("Visible" or "Hidden" or "UnderReview"))
        {
            return false;
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Comments SET ModerationStatus = $status WHERE Id = $id;";
        command.Parameters.AddWithValue("$status", normalizedStatus);
        command.Parameters.AddWithValue("$id", commentId);
        return command.ExecuteNonQuery() > 0;
    }

    public static bool ResetCommentReports(int commentId)
    {
        EnsureInitialized();
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        using (var deleteReports = connection.CreateCommand())
        {
            deleteReports.Transaction = transaction;
            deleteReports.CommandText = "DELETE FROM CommentReports WHERE CommentId = $id;";
            deleteReports.Parameters.AddWithValue("$id", commentId);
            deleteReports.ExecuteNonQuery();
        }

        using var updateComment = connection.CreateCommand();
        updateComment.Transaction = transaction;
        updateComment.CommandText = @"
            UPDATE Comments
            SET ReportCount = 0,
                ModerationStatus = CASE WHEN ModerationStatus = 'UnderReview' THEN 'Visible' ELSE ModerationStatus END
            WHERE Id = $id;";
        updateComment.Parameters.AddWithValue("$id", commentId);
        var affected = updateComment.ExecuteNonQuery();

        transaction.Commit();
        return affected > 0;
    }

    public static List<AdminContactItem> GetAllContactsForExport()
    {
        EnsureInitialized();
        var items = new List<AdminContactItem>();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Email, Topic, Message, SubmittedAtUtc
            FROM ContactSubmissions
            ORDER BY SubmittedAtUtc DESC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new AdminContactItem
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Topic = reader.GetString(3),
                Message = reader.GetString(4),
                SubmittedAtUtc = DateTime.Parse(reader.GetString(5))
            });
        }

        return items;
    }

    private static string Count(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(1) FROM {tableName};";
        return Convert.ToInt32(command.ExecuteScalar()).ToString();
    }

    private static List<AdminCommentItem> GetRecentComments(SqliteConnection connection)
    {
        var items = new List<AdminCommentItem>();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT c.Id, p.Title, c.AuthorEmail, c.Content, c.ReportCount, c.ModerationStatus, c.CreatedAtUtc
            FROM Comments c
            INNER JOIN Projects p ON p.Id = c.ProjectId
            ORDER BY c.CreatedAtUtc DESC
            LIMIT 8;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new AdminCommentItem
            {
                Id = reader.GetInt32(0),
                ProjectTitle = reader.GetString(1),
                AuthorEmail = reader.GetString(2),
                Content = reader.GetString(3),
                ReportCount = reader.GetInt32(4),
                ModerationStatus = reader.GetString(5),
                CreatedAtUtc = DateTime.Parse(reader.GetString(6))
            });
        }

        return items;
    }

    private static List<AdminContactItem> GetRecentContacts(SqliteConnection connection)
    {
        var items = new List<AdminContactItem>();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Email, Topic, Message, SubmittedAtUtc
            FROM ContactSubmissions
            ORDER BY SubmittedAtUtc DESC
            LIMIT 8;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new AdminContactItem
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Topic = reader.GetString(3),
                Message = reader.GetString(4),
                SubmittedAtUtc = DateTime.Parse(reader.GetString(5))
            });
        }

        return items;
    }

    private static List<AdminRegistrationItem> GetRecentRegistrations(SqliteConnection connection)
    {
        var items = new List<AdminRegistrationItem>();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT e.Name, r.MemberEmail, r.RegisteredAtUtc
            FROM EventRegistrations r
            INNER JOIN Events e ON e.Id = r.EventId
            ORDER BY r.RegisteredAtUtc DESC
            LIMIT 8;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new AdminRegistrationItem
            {
                EventName = reader.GetString(0),
                MemberEmail = reader.GetString(1),
                RegisteredAtUtc = DateTime.Parse(reader.GetString(2))
            });
        }

        return items;
    }

    private static List<AdminEventRegistrationGroup> GetEventRegistrationsByEvent(SqliteConnection connection)
    {
        var groups = GetEvents().Select(ev => new AdminEventRegistrationGroup
        {
            EventId = ev.Id,
            EventType = ev.Type,
            EventName = ev.Name,
            DateAndVenue = ev.DateAndVenue,
            Registrations = []
        }).ToDictionary(item => item.EventId);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT r.EventId,
                   m.FullName,
                   m.Email,
                   m.StudentId,
                   m.Department,
                   IFNULL(m.GitHubProfile, ''),
                   r.RegisteredAtUtc
            FROM EventRegistrations r
            INNER JOIN Members m ON m.Email = r.MemberEmail
            ORDER BY r.RegisteredAtUtc DESC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var eventId = reader.GetInt32(0);
            if (!groups.TryGetValue(eventId, out var group))
            {
                continue;
            }

            group.Registrations.Add(new AdminEventRegistrationItem
            {
                FullName = reader.GetString(1),
                Email = reader.GetString(2),
                StudentId = reader.GetString(3),
                Department = reader.GetString(4),
                GitHubProfile = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                RegisteredAtUtc = DateTime.Parse(reader.GetString(6))
            });
        }

        return groups.Values.OrderBy(item => item.EventId).ToList();
    }

    private static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        lock (SyncRoot)
        {
            if (_initialized)
            {
                return;
            }

            Directory.CreateDirectory(DatabaseDirectory);
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS Members (
                    Email TEXT PRIMARY KEY,
                    FullName TEXT NOT NULL,
                    StudentId TEXT NOT NULL,
                    Department TEXT NOT NULL,
                    ImageUrl TEXT,
                    GitHubProfile TEXT
                );

                CREATE TABLE IF NOT EXISTS Credentials (
                    Email TEXT PRIMARY KEY,
                    Password TEXT NOT NULL,
                    FOREIGN KEY (Email) REFERENCES Members(Email) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS Projects (
                    Id INTEGER PRIMARY KEY,
                    Title TEXT NOT NULL,
                    Summary TEXT NOT NULL,
                    Lead TEXT NOT NULL,
                    Stack TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Comments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProjectId INTEGER NOT NULL,
                    AuthorEmail TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    CreatedAtUtc TEXT NOT NULL,
                    ReportCount INTEGER NOT NULL DEFAULT 0,
                    ModerationStatus TEXT NOT NULL DEFAULT 'Visible',
                    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS CommentReports (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CommentId INTEGER NOT NULL,
                    MemberEmail TEXT NOT NULL,
                    Reason TEXT NOT NULL,
                    CreatedAtUtc TEXT NOT NULL,
                    IsDuplicate INTEGER NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS Events (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Type TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    DateAndVenue TEXT NOT NULL,
                    Description TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS EventRegistrations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EventId INTEGER NOT NULL,
                    MemberEmail TEXT NOT NULL,
                    RegisteredAtUtc TEXT NOT NULL,
                    UNIQUE(EventId, MemberEmail)
                );

                CREATE TABLE IF NOT EXISTS NewsletterSubscribers (
                    Email TEXT PRIMARY KEY,
                    SubscribedAtUtc TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ContactSubmissions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Topic TEXT NOT NULL,
                    Message TEXT NOT NULL,
                    SubmittedAtUtc TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS EmailAnnouncements (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Subject TEXT NOT NULL,
                    Body TEXT NOT NULL,
                    Audience TEXT NOT NULL,
                    RecipientCount INTEGER NOT NULL,
                    SentAtUtc TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS EmailAnnouncementRecipients (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AnnouncementId INTEGER NOT NULL,
                    RecipientEmail TEXT NOT NULL,
                    FOREIGN KEY (AnnouncementId) REFERENCES EmailAnnouncements(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS ProfileImageOverrides (
                    ProfileKey TEXT PRIMARY KEY,
                    ImageUrl TEXT NOT NULL,
                    UpdatedAtUtc TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ProfileOverrides (
                    ProfileKey TEXT PRIMARY KEY,
                    Name TEXT,
                    Email TEXT,
                    ImageUrl TEXT,
                    UpdatedAtUtc TEXT NOT NULL
                );";
            command.ExecuteNonQuery();

            EnsureColumnExists(connection, "Members", "ImageUrl", "TEXT");
            EnsureColumnExists(connection, "Members", "GitHubProfile", "TEXT");

            SeedIfEmpty(connection);
            _initialized = true;
        }
    }

    private static void SeedIfEmpty(SqliteConnection connection)
    {
        using var projectCountCommand = connection.CreateCommand();
        projectCountCommand.CommandText = "SELECT COUNT(1) FROM Projects;";
        var projectCount = Convert.ToInt32(projectCountCommand.ExecuteScalar());

        if (projectCount == 0)
        {
            InsertSeedMembers(connection);
            InsertSeedProjectsAndComments(connection);
            InsertSeedEvents(connection);
        }
    }

    private static void InsertSeedMembers(SqliteConnection connection)
    {
        var seedMembers = new[]
        {
            (Email: "member1@kuet.ac.bd", FullName: "Member One", StudentId: "2018001", Department: "EEE", Password: "member123"),
            (Email: "member2@kuet.ac.bd", FullName: "Member Two", StudentId: "2018002", Department: "CSE", Password: "member123"),
            (Email: "member3@kuet.ac.bd", FullName: "Member Three", StudentId: "2018003", Department: "ECE", Password: "member123")
        };

        foreach (var member in seedMembers)
        {
            using var memberCommand = connection.CreateCommand();
            memberCommand.CommandText = "INSERT INTO Members (Email, FullName, StudentId, Department, ImageUrl) VALUES ($email, $fullName, $studentId, $department, $imageUrl);";
            memberCommand.Parameters.AddWithValue("$email", member.Email);
            memberCommand.Parameters.AddWithValue("$fullName", member.FullName);
            memberCommand.Parameters.AddWithValue("$studentId", member.StudentId);
            memberCommand.Parameters.AddWithValue("$department", member.Department);
            memberCommand.Parameters.AddWithValue("$imageUrl", "");
            memberCommand.ExecuteNonQuery();

            using var credentialCommand = connection.CreateCommand();
            credentialCommand.CommandText = "INSERT INTO Credentials (Email, Password) VALUES ($email, $password);";
            credentialCommand.Parameters.AddWithValue("$email", member.Email);
            credentialCommand.Parameters.AddWithValue("$password", member.Password);
            credentialCommand.ExecuteNonQuery();
        }
    }

    private static void InsertSeedProjectsAndComments(SqliteConnection connection)
    {
        using var project1 = connection.CreateCommand();
        project1.CommandText = "INSERT INTO Projects (Id, Title, Summary, Lead, Stack) VALUES (1, 'FPGA-Based CNN Accelerator', 'Designed an FPGA pipeline for low-latency image classification.', 'Arafat Hossain', 'Verilog, Xilinx Vivado, Python');";
        project1.ExecuteNonQuery();

        using var project2 = connection.CreateCommand();
        project2.CommandText = "INSERT INTO Projects (Id, Title, Summary, Lead, Stack) VALUES (2, 'RISC-V Vector Processing Unit', 'Custom vector extension prototype focused on matrix operations.', 'Nabila Sultana', 'SystemVerilog, C++, RISC-V Toolchain');";
        project2.ExecuteNonQuery();

        using var comment = connection.CreateCommand();
        comment.CommandText = @"
            INSERT INTO Comments (ProjectId, AuthorEmail, Content, CreatedAtUtc, ReportCount, ModerationStatus)
            VALUES (1, 'member1@kuet.ac.bd', 'Impressive throughput! Can you share LUT usage details?', $createdAtUtc, 0, 'Visible');";
        comment.Parameters.AddWithValue("$createdAtUtc", DateTime.UtcNow.AddDays(-2).ToString("O"));
        comment.ExecuteNonQuery();
    }

    private static void InsertSeedEvents(SqliteConnection connection)
    {
        var seeds = new[]
        {
            (1, "Workshop", "FPGA Fundamentals Bootcamp", "May 10, 2026 · ECE Seminar Room", "Hands-on intro to RTL design, synthesis, and FPGA deployment."),
            (2, "Hackathon", "Accelerator Design Sprint", "June 02, 2026 · KUET CSE Lab", "Build a low-latency computing pipeline in teams and present your benchmark."),
            (3, "Talk", "Industry Session: Edge AI Systems", "June 22, 2026 · Central Auditorium", "Guest engineers discuss real production constraints in edge inference.")
        };

        foreach (var ev in seeds)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Events (Id, Type, Name, DateAndVenue, Description) VALUES ($id, $type, $name, $date, $description);";
            command.Parameters.AddWithValue("$id", ev.Item1);
            command.Parameters.AddWithValue("$type", ev.Item2);
            command.Parameters.AddWithValue("$name", ev.Item3);
            command.Parameters.AddWithValue("$date", ev.Item4);
            command.Parameters.AddWithValue("$description", ev.Item5);
            command.ExecuteNonQuery();
        }
    }

    private static List<AdminAnnouncementItem> GetRecentAnnouncements(SqliteConnection connection)
    {
        var items = new List<AdminAnnouncementItem>();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Subject, Body, Audience, RecipientCount, SentAtUtc
            FROM EmailAnnouncements
            ORDER BY SentAtUtc DESC
            LIMIT 6;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var body = reader.GetString(1);
            items.Add(new AdminAnnouncementItem
            {
                Subject = reader.GetString(0),
                BodyPreview = body.Length > 120 ? body[..120] + "..." : body,
                Audience = reader.GetString(2),
                RecipientCount = reader.GetInt32(3),
                SentAtUtc = DateTime.Parse(reader.GetString(4))
            });
        }

        return items;
    }

    private static Dictionary<string, string> GetProfileImageOverrides(SqliteConnection connection)
    {
        var items = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ProfileKey, ImageUrl FROM ProfileImageOverrides;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items[reader.GetString(0)] = reader.GetString(1);
        }

        return items;
    }

    private static Dictionary<string, (string Name, string Email, string ImageUrl)> GetProfileOverrides(SqliteConnection connection)
    {
        var items = new Dictionary<string, (string, string, string)>(StringComparer.OrdinalIgnoreCase);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ProfileKey, Name, Email, IFNULL(ImageUrl, '') FROM ProfileOverrides;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items[reader.GetString(0)] = (reader.IsDBNull(1) ? string.Empty : reader.GetString(1), reader.IsDBNull(2) ? string.Empty : reader.GetString(2), reader.IsDBNull(3) ? string.Empty : reader.GetString(3));
        }

        return items;
    }

    public static (bool success, string message) SetProfileOverride(string profileKey, string? name, string? email)
    {
        EnsureInitialized();

        var normalizedProfileKey = (profileKey ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedProfileKey))
        {
            return (false, "Invalid profile target.");
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO ProfileOverrides (ProfileKey, Name, Email, ImageUrl, UpdatedAtUtc)
            VALUES ($profileKey, $name, $email, (SELECT ImageUrl FROM ProfileOverrides WHERE ProfileKey = $profileKey), $updatedAtUtc)
            ON CONFLICT(ProfileKey) DO UPDATE SET
                Name = excluded.Name,
                Email = excluded.Email,
                UpdatedAtUtc = excluded.UpdatedAtUtc;";
        command.Parameters.AddWithValue("$profileKey", normalizedProfileKey);
        command.Parameters.AddWithValue("$name", name ?? string.Empty);
        command.Parameters.AddWithValue("$email", email ?? string.Empty);
        command.Parameters.AddWithValue("$updatedAtUtc", DateTime.UtcNow.ToString("O"));

        return command.ExecuteNonQuery() > 0
            ? (true, "Profile details saved.")
            : (false, "Could not save profile details.");
    }
    private static string BuildProfileKey(string roleKey, string email) => $"{roleKey.Trim().ToLowerInvariant()}:{Normalize(email)}";

    private static string NormalizeGitHubProfile(string? profile)
    {
        var normalized = (profile ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
        {
            return string.Empty;
        }

        if (!uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) && !uri.Host.EndsWith(".github.com", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return uri.ToString();
    }

    private static SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        return connection;
    }

    private static bool MemberExists(SqliteConnection connection, string email)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Members WHERE Email = $email;";
        command.Parameters.AddWithValue("$email", email);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static void EnsureColumnExists(SqliteConnection connection, string tableName, string columnName, string columnDefinition)
    {
        using var pragma = connection.CreateCommand();
        pragma.CommandText = $"PRAGMA table_info({tableName});";

        using var reader = pragma.ExecuteReader();
        var columnExists = false;
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
            {
                columnExists = true;
                break;
            }
        }

        if (columnExists)
        {
            return;
        }

        using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};";
        alter.ExecuteNonQuery();
    }

    private static string Normalize(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();
}
