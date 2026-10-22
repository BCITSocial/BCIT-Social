// @author Asher
// Switches the Home discovery panel between events and the user's communities.
(() => {
    const tabs = [...document.querySelectorAll(".discovery-tab")];
    if (tabs.length === 0) return;

    function selectTab(selectedTab, moveFocus = false) {
        for (const tab of tabs) {
            const isSelected = tab === selectedTab;
            tab.setAttribute("aria-selected", String(isSelected));
            tab.tabIndex = isSelected ? 0 : -1;
            document.getElementById(tab.getAttribute("aria-controls")).hidden = !isSelected;
        }

        if (moveFocus) selectedTab.focus();
    }

    for (const [index, tab] of tabs.entries()) {
        tab.addEventListener("click", () => selectTab(tab));
        tab.addEventListener("keydown", (event) => {
            if (event.key !== "ArrowLeft" && event.key !== "ArrowRight") return;
            event.preventDefault();
            const direction = event.key === "ArrowRight" ? 1 : -1;
            const nextIndex = (index + direction + tabs.length) % tabs.length;
            selectTab(tabs[nextIndex], true);
        });
    }
})();
