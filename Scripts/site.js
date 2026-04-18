(function () {
    var toggle = document.getElementById("menuToggle");
    var menu = document.getElementById("topMenu");

    if (toggle && menu) {
        toggle.addEventListener("click", function () {
            menu.classList.toggle("open");
        });
    }
})();
