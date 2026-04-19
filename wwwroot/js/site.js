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
})();