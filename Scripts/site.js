(function () {
    var STORAGE_KEYS = {
        members: "hac_members",
        users: "hac_users",
        currentUser: "hac_current_user",
        projects: "hac_projects",
        reports: "hac_comment_reports",
        newsletter: "hac_newsletter_subscribers",
        inquiries: "hac_contact_inquiries",
        eventRegistrations: "hac_event_registrations"
    };

    var DUPLICATE_REPORT_WINDOW_HOURS = 24;

    var defaultMembers = [
        "member1@kuet.ac.bd",
        "member2@kuet.ac.bd",
        "member3@kuet.ac.bd"
    ];

    var defaultUsers = [
        { email: "member1@kuet.ac.bd", password: "member123" },
        { email: "member2@kuet.ac.bd", password: "member123" },
        { email: "member3@kuet.ac.bd", password: "member123" }
    ];

    var defaultProjects = [
        {
            id: 1,
            title: "FPGA-Based CNN Accelerator",
            summary: "Designed an FPGA pipeline for low-latency image classification.",
            lead: "Arafat Hossain",
            stack: "Verilog, Xilinx Vivado, Python",
            comments: [
                {
                    id: 1,
                    authorEmail: "member1@kuet.ac.bd",
                    content: "Impressive throughput! Can you share LUT usage details?",
                    createdAtUtc: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
                    reportCount: 0,
                    moderationStatus: "Visible"
                }
            ]
        },
        {
            id: 2,
            title: "RISC-V Vector Processing Unit",
            summary: "Custom vector extension prototype focused on matrix operations.",
            lead: "Nabila Sultana",
            stack: "SystemVerilog, C++, RISC-V Toolchain",
            comments: []
        }
    ];

    function setFooterYear() {
        var year = document.getElementById("footerYear");
        if (year) {
            year.textContent = new Date().getFullYear();
        }
    }

    function setActiveNavLink() {
        var currentPath = window.location.pathname.split("/").pop() || "index.html";
        var links = document.querySelectorAll("#topMenu a[href]");

        links.forEach(function (link) {
            var href = link.getAttribute("href");
            if (href === currentPath) {
                link.classList.add("active");
            }
        });
    }

    function setupMenu() {
        var toggle = document.getElementById("menuToggle");
        var menu = document.getElementById("topMenu");

        if (toggle && menu) {
            toggle.addEventListener("click", function () {
                menu.classList.toggle("open");
            });
        }
    }

    function normalizeEmail(email) {
        return (email || "").trim().toLowerCase();
    }

    function load(key, fallback) {
        try {
            var raw = localStorage.getItem(key);
            if (!raw) {
                return fallback;
            }
            return JSON.parse(raw);
        } catch (e) {
            return fallback;
        }
    }

    function save(key, value) {
        localStorage.setItem(key, JSON.stringify(value));
    }

    function ensureSeeds() {
        if (!localStorage.getItem(STORAGE_KEYS.members)) {
            save(STORAGE_KEYS.members, defaultMembers);
        }

        if (!localStorage.getItem(STORAGE_KEYS.users)) {
            save(STORAGE_KEYS.users, defaultUsers);
        }

        if (!localStorage.getItem(STORAGE_KEYS.projects)) {
            save(STORAGE_KEYS.projects, defaultProjects);
        }

        if (!localStorage.getItem(STORAGE_KEYS.reports)) {
            save(STORAGE_KEYS.reports, []);
        }

        if (!localStorage.getItem(STORAGE_KEYS.newsletter)) {
            save(STORAGE_KEYS.newsletter, []);
        }

        if (!localStorage.getItem(STORAGE_KEYS.inquiries)) {
            save(STORAGE_KEYS.inquiries, []);
        }

        if (!localStorage.getItem(STORAGE_KEYS.eventRegistrations)) {
            save(STORAGE_KEYS.eventRegistrations, []);
        }
    }

    function getCurrentUser() {
        return normalizeEmail(localStorage.getItem(STORAGE_KEYS.currentUser) || "");
    }

    function setCurrentUser(email) {
        localStorage.setItem(STORAGE_KEYS.currentUser, normalizeEmail(email));
    }

    function clearCurrentUser() {
        localStorage.removeItem(STORAGE_KEYS.currentUser);
    }

    function updateNavAuthState() {
        var authLink = document.getElementById("authNavLink");
        var logoutBtn = document.getElementById("logoutBtn");
        var currentUser = getCurrentUser();

        if (authLink) {
            if (currentUser) {
                authLink.textContent = "Logged in: " + currentUser;
                authLink.href = "auth.html";
            } else {
                authLink.textContent = "Register / Login";
                authLink.href = "auth.html";
            }
        }

        if (logoutBtn) {
            logoutBtn.classList.toggle("hidden", !currentUser);
            logoutBtn.onclick = function () {
                clearCurrentUser();
                window.location.href = "index.html";
            };
        }
    }

    function showMessage(elementId, message, isError) {
        var box = document.getElementById(elementId);
        if (!box) {
            return;
        }

        box.textContent = message;
        box.classList.remove("hidden");
        box.style.borderColor = isError ? "#8c2f2f" : "#2c5ea5";
        box.style.background = isError ? "#3b1b24" : "#17315a";
    }

    function setupAuthPage() {
        if (document.body.getAttribute("data-page") !== "auth") {
            return;
        }

        var registerForm = document.getElementById("registerForm");
        var loginForm = document.getElementById("loginForm");

        if (registerForm) {
            registerForm.addEventListener("submit", function (event) {
                event.preventDefault();

                var formData = new FormData(registerForm);
                var email = normalizeEmail(formData.get("Email"));
                var password = String(formData.get("Password") || "");

                if (!email || !password) {
                    showMessage("authMessage", "Please fill in all required fields.", true);
                    return;
                }

                var members = load(STORAGE_KEYS.members, []);
                var users = load(STORAGE_KEYS.users, []);

                var exists = users.some(function (u) {
                    return normalizeEmail(u.email) === email;
                });

                if (exists) {
                    showMessage("authMessage", "This email is already registered. Please login.", true);
                    return;
                }

                users.push({
                    email: email,
                    password: password
                });

                if (members.indexOf(email) === -1) {
                    members.push(email);
                }

                save(STORAGE_KEYS.users, users);
                save(STORAGE_KEYS.members, members);
                setCurrentUser(email);
                updateNavAuthState();
                registerForm.reset();
                showMessage("authMessage", "Registration successful. You are now logged in.", false);
            });
        }

        if (loginForm) {
            loginForm.addEventListener("submit", function (event) {
                event.preventDefault();

                var formData = new FormData(loginForm);
                var email = normalizeEmail(formData.get("Email"));
                var password = String(formData.get("Password") || "");
                var users = load(STORAGE_KEYS.users, []);

                var matched = users.some(function (u) {
                    return normalizeEmail(u.email) === email && String(u.password) === password;
                });

                if (!matched) {
                    showMessage("authMessage", "Invalid email or password.", true);
                    return;
                }

                setCurrentUser(email);
                updateNavAuthState();
                loginForm.reset();
                showMessage("authMessage", "Login successful.", false);
            });
        }
    }

    function isMember(email) {
        var members = load(STORAGE_KEYS.members, []);
        return members.indexOf(normalizeEmail(email)) !== -1;
    }

    function formatDate(dateIso) {
        var d = new Date(dateIso);
        return d.toLocaleString(undefined, {
            year: "numeric",
            month: "short",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit"
        });
    }

    function getNextCommentId(projects) {
        var maxId = 0;
        projects.forEach(function (p) {
            (p.comments || []).forEach(function (c) {
                if (c.id > maxId) {
                    maxId = c.id;
                }
            });
        });
        return maxId + 1;
    }

    function setupProjectsPage() {
        if (document.body.getAttribute("data-page") !== "projects") {
            return;
        }

        var container = document.getElementById("projectsContainer");
        if (!container) {
            return;
        }

        function renderProjects() {
            var currentUser = getCurrentUser();
            var projects = load(STORAGE_KEYS.projects, []);
            container.innerHTML = "";

            projects.forEach(function (project) {
                var section = document.createElement("section");
                section.className = "card project-card";

                var commentsHtml = "";
                var comments = (project.comments || []).slice().sort(function (a, b) {
                    return new Date(b.createdAtUtc) - new Date(a.createdAtUtc);
                });

                if (!comments.length) {
                    commentsHtml = "<p class=\"muted\">No comments yet.</p>";
                } else {
                    commentsHtml = comments.map(function (comment) {
                        var reportForm = "";

                        if (currentUser) {
                            reportForm = [
                                "<form class=\"report-form\" data-comment-id=\"" + comment.id + "\">",
                                "<select name=\"reason\" required>",
                                "<option value=\"Spam\">Spam</option>",
                                "<option value=\"Harassment\">Harassment</option>",
                                "<option value=\"OffTopic\">Off Topic</option>",
                                "<option value=\"Other\">Other</option>",
                                "</select>",
                                "<button type=\"submit\" class=\"btn-secondary\">Report</button>",
                                "</form>"
                            ].join("");
                        }

                        return [
                            "<article class=\"comment-item\">",
                            "<p>" + escapeHtml(comment.content) + "</p>",
                            "<small>",
                            "By " + escapeHtml(comment.authorEmail),
                            " · " + formatDate(comment.createdAtUtc),
                            " · Status: " + escapeHtml(comment.moderationStatus || "Visible"),
                            " · Reports: " + (comment.reportCount || 0),
                            "</small>",
                            reportForm,
                            "</article>"
                        ].join("");
                    }).join("");
                }

                var commentFormHtml = "";
                if (currentUser) {
                    commentFormHtml = [
                        "<form class=\"project-actions\" data-project-id=\"" + project.id + "\">",
                        "<label>Add Comment</label>",
                        "<textarea name=\"content\" rows=\"3\" required></textarea>",
                        "<button type=\"submit\" class=\"btn-primary\">Post Comment</button>",
                        "</form>"
                    ].join("");
                } else {
                    commentFormHtml = "<p class=\"project-login-hint\"><a href=\"auth.html\">Login as member</a> to comment and report.</p>";
                }

                section.innerHTML = [
                    "<h2>" + escapeHtml(project.title) + "</h2>",
                    "<p>" + escapeHtml(project.summary) + "</p>",
                    "<p><strong>Lead:</strong> " + escapeHtml(project.lead) + "</p>",
                    "<p><strong>Tech Stack:</strong> " + escapeHtml(project.stack) + "</p>",
                    "<div class=\"comments\">",
                    "<h3>Comments</h3>",
                    commentsHtml,
                    "</div>",
                    commentFormHtml
                ].join("");

                container.appendChild(section);
            });

            wireProjectHandlers();
        }

        function wireProjectHandlers() {
            var commentForms = container.querySelectorAll("form.project-actions");
            var reportForms = container.querySelectorAll("form.report-form");

            commentForms.forEach(function (form) {
                form.addEventListener("submit", function (event) {
                    event.preventDefault();

                    var projectId = Number(form.getAttribute("data-project-id"));
                    var currentUser = getCurrentUser();
                    var contentField = form.querySelector("textarea[name='content']");
                    var content = (contentField ? contentField.value : "").trim();

                    if (!currentUser || !isMember(currentUser)) {
                        showMessage("projectMessage", "Only registered members can comment.", true);
                        return;
                    }

                    if (!content) {
                        showMessage("projectMessage", "Comment cannot be empty.", true);
                        return;
                    }

                    var projects = load(STORAGE_KEYS.projects, []);
                    var project = projects.find(function (p) { return Number(p.id) === projectId; });

                    if (!project) {
                        showMessage("projectMessage", "Project not found.", true);
                        return;
                    }

                    project.comments = project.comments || [];
                    project.comments.push({
                        id: getNextCommentId(projects),
                        authorEmail: currentUser,
                        content: content,
                        createdAtUtc: new Date().toISOString(),
                        reportCount: 0,
                        moderationStatus: "Visible"
                    });

                    save(STORAGE_KEYS.projects, projects);
                    showMessage("projectMessage", "Comment posted successfully.", false);
                    renderProjects();
                });
            });

            reportForms.forEach(function (form) {
                form.addEventListener("submit", function (event) {
                    event.preventDefault();

                    var currentUser = getCurrentUser();
                    if (!currentUser || !isMember(currentUser)) {
                        showMessage("projectMessage", "Only registered members can report comments.", true);
                        return;
                    }

                    var commentId = Number(form.getAttribute("data-comment-id"));
                    var reasonField = form.querySelector("select[name='reason']");
                    var reason = (reasonField ? reasonField.value : "Other").trim();
                    var normalizedReason = reason.toLowerCase();
                    var now = new Date();
                    var reports = load(STORAGE_KEYS.reports, []);

                    var duplicate = reports.some(function (r) {
                        var sameUser = normalizeEmail(r.memberEmail) === currentUser;
                        var sameComment = Number(r.commentId) === commentId;
                        var sameReason = String(r.reasonCode || "").toLowerCase() === normalizedReason;
                        var hours = (now.getTime() - new Date(r.createdAtUtc).getTime()) / (1000 * 60 * 60);
                        return sameUser && sameComment && sameReason && hours <= DUPLICATE_REPORT_WINDOW_HOURS;
                    });

                    reports.push({
                        commentId: commentId,
                        memberEmail: currentUser,
                        reasonCode: reason,
                        createdAtUtc: now.toISOString(),
                        isSuppressedDuplicate: duplicate
                    });
                    save(STORAGE_KEYS.reports, reports);

                    if (duplicate) {
                        showMessage("projectMessage", "Duplicate report blocked. You already reported this recently.", true);
                        return;
                    }

                    var projects = load(STORAGE_KEYS.projects, []);
                    var updated = false;

                    projects.forEach(function (project) {
                        (project.comments || []).forEach(function (comment) {
                            if (Number(comment.id) === commentId) {
                                comment.reportCount = Number(comment.reportCount || 0) + 1;
                                if (comment.reportCount >= 3) {
                                    comment.moderationStatus = "UnderReview";
                                }
                                updated = true;
                            }
                        });
                    });

                    if (!updated) {
                        showMessage("projectMessage", "Comment not found.", true);
                        return;
                    }

                    save(STORAGE_KEYS.projects, projects);
                    showMessage("projectMessage", "Report submitted successfully.", false);
                    renderProjects();
                });
            });
        }

        renderProjects();
    }

    function setupNewsletterForm() {
        var form = document.getElementById("newsletterForm");
        var message = document.getElementById("newsletterMessage");

        if (!form || !message) {
            return;
        }

        form.addEventListener("submit", function (event) {
            event.preventDefault();

            var formData = new FormData(form);
            var email = normalizeEmail(formData.get("newsletterEmail"));

            if (!email) {
                message.textContent = "Please provide a valid email.";
                return;
            }

            var subscribers = load(STORAGE_KEYS.newsletter, []);
            if (subscribers.indexOf(email) !== -1) {
                message.textContent = "This email is already subscribed.";
                return;
            }

            subscribers.push(email);
            save(STORAGE_KEYS.newsletter, subscribers);
            form.reset();
            message.textContent = "Subscribed successfully. You will receive upcoming updates.";
        });
    }

    function setupEventsPage() {
        if (document.body.getAttribute("data-page") !== "events") {
            return;
        }

        var buttons = document.querySelectorAll(".register-event-btn");
        if (!buttons.length) {
            return;
        }

        buttons.forEach(function (button) {
            button.addEventListener("click", function () {
                var currentUser = getCurrentUser();
                if (!currentUser || !isMember(currentUser)) {
                    showMessage("eventMessage", "Please login as a member to register for events.", true);
                    return;
                }

                var eventName = button.getAttribute("data-event-name") || "Event";
                var registrations = load(STORAGE_KEYS.eventRegistrations, []);

                var alreadyRegistered = registrations.some(function (r) {
                    return normalizeEmail(r.memberEmail) === currentUser && r.eventName === eventName;
                });

                if (alreadyRegistered) {
                    showMessage("eventMessage", "You are already registered for this event.", true);
                    return;
                }

                registrations.push({
                    eventName: eventName,
                    memberEmail: currentUser,
                    registeredAtUtc: new Date().toISOString()
                });

                save(STORAGE_KEYS.eventRegistrations, registrations);
                showMessage("eventMessage", "Registration successful for " + eventName + ".", false);
            });
        });
    }

    function setupContactPage() {
        if (document.body.getAttribute("data-page") !== "contact") {
            return;
        }

        var form = document.getElementById("contactForm");
        if (!form) {
            return;
        }

        form.addEventListener("submit", function (event) {
            event.preventDefault();

            var formData = new FormData(form);
            var inquiry = {
                name: String(formData.get("name") || "").trim(),
                email: normalizeEmail(formData.get("email")),
                topic: String(formData.get("topic") || "").trim(),
                message: String(formData.get("message") || "").trim(),
                createdAtUtc: new Date().toISOString()
            };

            if (!inquiry.name || !inquiry.email || !inquiry.topic || !inquiry.message) {
                showMessage("contactMessage", "Please fill in all contact form fields.", true);
                return;
            }

            var inquiries = load(STORAGE_KEYS.inquiries, []);
            inquiries.push(inquiry);
            save(STORAGE_KEYS.inquiries, inquiries);

            form.reset();
            showMessage("contactMessage", "Message sent successfully. We will get back to you soon.", false);
        });
    }

    function escapeHtml(value) {
        return String(value || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/\"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    ensureSeeds();
    setFooterYear();
    setupMenu();
    setActiveNavLink();
    updateNavAuthState();
    setupAuthPage();
    setupProjectsPage();
    setupNewsletterForm();
    setupEventsPage();
    setupContactPage();
})();
