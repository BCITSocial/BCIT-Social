// Submit the Events filter form as soon as a dropdown changes.
document.querySelectorAll("[data-events-filters] select").forEach((select) => {
    select.addEventListener("change", () => select.form.submit());
});

// The filter menu is a <details>, so it opens and closes without this script.
// This only adds closing it with Escape (focus goes back to the button) or with a click outside it.
document.querySelectorAll("[data-filter-menu]").forEach((menu) => {
    const button = menu.querySelector("summary");

    menu.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && menu.open) {
            menu.open = false;
            button.focus();
        }
    });

    document.addEventListener("click", (event) => {
        if (menu.open && !menu.contains(event.target)) {
            menu.open = false;
        }
    });
});
