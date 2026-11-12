(() => {
    const tabs = [...document.querySelectorAll("[data-profile-tab]")];
    const panels = [...document.querySelectorAll("[data-profile-panel]")];

    if (tabs.length === 0 || panels.length === 0) {
        return;
    }

    function selectSection(section, { focus = false, updateUrl = true } = {}) {
        const selectedTab = tabs.find((tab) => tab.dataset.profileTab === section);
        if (!selectedTab) {
            return;
        }

        for (const tab of tabs) {
            const selected = tab === selectedTab;
            tab.setAttribute("aria-selected", String(selected));
            tab.tabIndex = selected ? 0 : -1;
        }
        for (const panel of panels) {
            panel.hidden = panel.dataset.profilePanel !== section;
        }

        if (focus) {
            selectedTab.focus();
        }
        if (updateUrl) {
            const url = new URL(window.location.href);
            url.searchParams.set("section", section);
            history.replaceState(history.state, "", url);
        }
    }

    for (const tab of tabs) {
        tab.addEventListener("click", () => selectSection(tab.dataset.profileTab));
        tab.addEventListener("keydown", (event) => {
            const index = tabs.indexOf(tab);
            let nextIndex;
            if (event.key === "ArrowRight") {
                nextIndex = (index + 1) % tabs.length;
            } else if (event.key === "ArrowLeft") {
                nextIndex = (index - 1 + tabs.length) % tabs.length;
            } else if (event.key === "Home") {
                nextIndex = 0;
            } else if (event.key === "End") {
                nextIndex = tabs.length - 1;
            } else {
                return;
            }

            event.preventDefault();
            selectSection(tabs[nextIndex].dataset.profileTab, { focus: true });
        });
    }

    window.addEventListener("popstate", () => {
        selectSection(new URLSearchParams(window.location.search).get("section") ?? "posts", { updateUrl: false });
    });
})();
