(function () {
    var toggle = document.getElementById("menuToggle");
    var menu = document.getElementById("topMenu");

    if (toggle && menu) {
        toggle.addEventListener("click", function () {
            menu.classList.toggle("open");
        });
    }

    if (menu) {
        var links = menu.querySelectorAll("a[href]");
        var path = window.location.pathname.toLowerCase();
        links.forEach(function (link) {
            var href = (link.getAttribute("href") || "").toLowerCase();
            if (!href) {
                return;
            }

            if (path === href || (href !== "/" && path.indexOf(href) === 0)) {
                link.style.color = "#5cf2cf";
                link.style.fontWeight = "600";
            }
        });
    }

    var backToTop = document.getElementById("backToTop");
    if (backToTop) {
        var toggleBackToTop = function () {
            var isScrollable = document.documentElement.scrollHeight > window.innerHeight + 120;
            var shouldShow = isScrollable && window.scrollY > 260;
            backToTop.classList.toggle("visible", shouldShow);
        };

        backToTop.addEventListener("click", function () {
            window.scrollTo({ top: 0, behavior: "smooth" });
        });

        window.addEventListener("scroll", toggleBackToTop, { passive: true });
        window.addEventListener("resize", toggleBackToTop);
        toggleBackToTop();
    }

    var dashboard = document.getElementById("adminDashboard");
    if (dashboard) {
        var canvas = document.getElementById("roleChart");
        var chartContext = canvas ? canvas.getContext("2d") : null;
        var membersCount = document.getElementById("membersCount");
        var adminsCount = document.getElementById("adminsCount");
        var totalPeopleCount = document.getElementById("totalPeopleCount");
        var liveUpdatedAt = document.getElementById("liveUpdatedAt");
        var tableBody = document.getElementById("peopleTableBody");
        var customRecipients = document.getElementById("customRecipients");
        var audienceRadios = document.querySelectorAll('input[name="Audience"]');
        var latestSnapshot = null;

        var drawChart = function (memberTotal, adminTotal) {
            if (!canvas || !chartContext) {
                return;
            }

            var width = canvas.clientWidth || 320;
            var height = 240;
            canvas.width = width;
            canvas.height = height;

            chartContext.clearRect(0, 0, width, height);

            var leftPadding = 70;
            var topPadding = 34;
            var barHeight = 42;
            var barGap = 34;
            var maxCount = Math.max(memberTotal, adminTotal, 1);
            var usableWidth = width - leftPadding - 32;
            var memberBarWidth = Math.max(18, Math.round((memberTotal / maxCount) * usableWidth));
            var adminBarWidth = Math.max(18, Math.round((adminTotal / maxCount) * usableWidth));

            chartContext.font = "600 14px Segoe UI, sans-serif";
            chartContext.fillStyle = "#18212c";
            chartContext.fillText("Members", 16, topPadding + 26);
            chartContext.fillText("Admins", 16, topPadding + barGap + barHeight + 26);

            chartContext.fillStyle = "rgba(168, 117, 56, 0.22)";
            chartContext.fillRect(leftPadding, topPadding, usableWidth, barHeight);
            chartContext.fillRect(leftPadding, topPadding + barGap + barHeight, usableWidth, barHeight);

            chartContext.fillStyle = "#a87538";
            chartContext.fillRect(leftPadding, topPadding, memberBarWidth, barHeight);
            chartContext.fillStyle = "#5bc0b5";
            chartContext.fillRect(leftPadding, topPadding + barGap + barHeight, adminBarWidth, barHeight);

            chartContext.fillStyle = "#18212c";
            chartContext.fillText(String(memberTotal), leftPadding + memberBarWidth + 10, topPadding + 26);
            chartContext.fillText(String(adminTotal), leftPadding + adminBarWidth + 10, topPadding + barGap + barHeight + 26);
        };

        var renderRows = function (people) {
            if (!tableBody) {
                return;
            }

            tableBody.innerHTML = "";
            people.forEach(function (person) {
                var row = document.createElement("tr");
                row.innerHTML = "<td><span class='chip'>" + person.role + "</span></td>" +
                    "<td>" + person.name + "</td>" +
                    "<td>" + person.email + "</td>" +
                    "<td>" + person.department + "</td>" +
                    "<td>" + person.studentId + "</td>";
                tableBody.appendChild(row);
            });
        };

        var renderSnapshot = function (snapshot) {
            latestSnapshot = snapshot;
            var people = snapshot.people || [];
            var members = people.filter(function (person) { return person.role === "Member"; });
            var admins = people.filter(function (person) { return person.role === "Admin"; });

            if (membersCount) { membersCount.textContent = String(members.length); }
            if (adminsCount) { adminsCount.textContent = String(admins.length); }
            if (totalPeopleCount) { totalPeopleCount.textContent = String(people.length); }
            if (liveUpdatedAt) { liveUpdatedAt.textContent = "Updated " + new Date(snapshot.updatedAtUtc).toLocaleString(); }

            drawChart(members.length, admins.length);
            renderRows(people);
        };

        var refreshSnapshot = function () {
            fetch("/Admin/DashboardSnapshot", { headers: { "Accept": "application/json" } })
                .then(function (response) { return response.json(); })
                .then(renderSnapshot)
                .catch(function () { });
        };

        var toggleCustomRecipients = function () {
            if (!customRecipients) {
                return;
            }

            var selectedAudience = document.querySelector('input[name="Audience"]:checked');
            var isCustom = selectedAudience && selectedAudience.value === "Custom";
            customRecipients.classList.toggle("is-hidden", !isCustom);
        };

        audienceRadios.forEach(function (radio) {
            radio.addEventListener("change", toggleCustomRecipients);
        });

        window.addEventListener("resize", function () {
            if (latestSnapshot) {
                renderSnapshot(latestSnapshot);
            }
        });

        toggleCustomRecipients();
        refreshSnapshot();
        window.setInterval(refreshSnapshot, 10000);
    }
})();