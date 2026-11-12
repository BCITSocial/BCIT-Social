// Communities page: keeps the category pills to one row and shows a "Show all" button on the right
// when some pills don't fit. The button expands the row to show every pill, and "Show less" folds it back.
(() => {
    const row = document.querySelector("[data-pills-row]");
    if (!row) return;

    const pills = row.querySelector(".filter-pills");
    const toggle = row.querySelector("[data-pills-toggle]");
    const firstPill = pills.querySelector(".filter-pill");
    if (!firstPill) return;

    let expanded = false;

    function update() {
        row.style.setProperty("--pills-row-height", `${firstPill.offsetHeight}px`);
        row.classList.toggle("is-collapsed", !expanded);
        // Some pills are on a second line when the last one sits lower than the first.
        const lastPill = pills.lastElementChild;
        const overflows = lastPill.offsetTop > firstPill.offsetTop;
        toggle.hidden = !overflows;
        toggle.textContent = expanded ? "Show less" : "Show all";
        toggle.setAttribute("aria-expanded", expanded ? "true" : "false");
    }

    toggle.addEventListener("click", () => {
        expanded = !expanded;
        update();
    });

    // Tabbing onto a pill hidden on a later line opens the row so the focused pill is visible.
    pills.addEventListener("focusin", (event) => {
        if (!expanded && event.target.offsetTop > firstPill.offsetTop) {
            expanded = true;
            update();
        }
    });

    window.addEventListener("resize", update);
    update();
})();
