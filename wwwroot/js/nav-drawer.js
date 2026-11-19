// author: Andre Di Lascio
// Phone and tablet layout: the menu button in the header slides the left nav in from the side,
// and the search icon unfolds the header search. Both do nothing on wide screens, where the
// nav and search are always shown (see the "Phone layout" section of home.css).
(() => {
    const toggle = document.querySelector("[data-nav-toggle]");
    const nav = document.getElementById("app-nav");
    const backdrop = document.querySelector("[data-nav-backdrop]");
    const narrow = window.matchMedia("(max-width: 900px)");

    function setNavOpen(open, returnFocus) {
        document.body.classList.toggle("nav-open", open);
        toggle?.setAttribute("aria-expanded", String(open));
        toggle?.setAttribute("aria-label", open ? "Close menu" : "Open menu");
        if (backdrop) backdrop.hidden = !open;
        if (open) {
            nav?.querySelector("a")?.focus();
        } else if (returnFocus) {
            toggle?.focus();
        }
    }

    if (toggle && nav) {
        toggle.addEventListener("click", () => setNavOpen(!document.body.classList.contains("nav-open"), false));
        backdrop?.addEventListener("click", () => setNavOpen(false, true));
        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape" && document.body.classList.contains("nav-open")) setNavOpen(false, true);
        });
        // Going back to a wide window closes the menu so it doesn't stay open behind the page.
        narrow.addEventListener("change", (event) => {
            if (!event.matches) setNavOpen(false, false);
        });
    }

    const searchToggle = document.querySelector("[data-search-toggle]");
    searchToggle?.addEventListener("click", () => {
        const open = document.body.classList.toggle("search-open");
        searchToggle.setAttribute("aria-expanded", String(open));
        if (open) document.querySelector(".header-search input:not(:disabled)")?.focus();
    });
})();
