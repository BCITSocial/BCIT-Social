// Overview / Members tabs on a community's page. The "N members" link in the header also opens
// the Members tab. The open tab is kept in the URL (#members) so it survives a reload or a shared link.
(() => {
    const tabs = [...document.querySelectorAll(".details-tabs [data-tab]")];
    if (tabs.length === 0) return;

    const show = (name, focus) => {
        const tab = tabs.find((t) => t.dataset.tab === name) ?? tabs[0];
        tabs.forEach((t) => {
            const selected = t === tab;
            t.setAttribute("aria-selected", String(selected));
            t.tabIndex = selected ? 0 : -1;
            document.getElementById(t.getAttribute("aria-controls")).hidden = !selected;
        });
        if (focus) tab.focus();
        history.replaceState(null, "", tab.dataset.tab === tabs[0].dataset.tab ? location.pathname + location.search : `#${tab.dataset.tab}`);
        return tab;
    };

    tabs.forEach((tab) => tab.addEventListener("click", () => show(tab.dataset.tab)));

    // Left and right arrows move between tabs.
    document.querySelector(".details-tabs").addEventListener("keydown", (event) => {
        if (event.key !== "ArrowLeft" && event.key !== "ArrowRight") return;
        const current = tabs.indexOf(document.activeElement);
        if (current < 0) return;
        const next = (current + (event.key === "ArrowRight" ? 1 : tabs.length - 1)) % tabs.length;
        show(tabs[next].dataset.tab, true);
    });

    document.querySelectorAll("[data-show-tab]").forEach((link) => {
        link.addEventListener("click", (event) => {
            event.preventDefault();
            show(link.dataset.showTab).scrollIntoView({ behavior: "smooth", block: "nearest" });
        });
    });

    if (tabs.some((t) => `#${t.dataset.tab}` === location.hash)) show(location.hash.slice(1));
})();
