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
})();