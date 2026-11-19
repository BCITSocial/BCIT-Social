(() => {
    const trigger = document.querySelector("[data-open-members]");
    const dialog = document.getElementById("community-members-dialog");
    if (!trigger || typeof dialog?.showModal !== "function") return;

    const content = dialog.querySelector("[data-members-content]");
    const status = dialog.querySelector("[data-members-status]");
    const fallback = dialog.querySelector("[data-members-fallback]");
    let loadRequest;

    async function openMembers() {
        if (dialog.open) return;

        loadRequest = new AbortController();
        const { signal } = loadRequest;
        const url = new URL(trigger.href);
        url.searchParams.set("popup", "true");
        content.replaceChildren();
        status.textContent = "Loading members…";
        fallback.hidden = true;
        dialog.showModal();

        try {
            const response = await fetch(url, { signal, cache: "no-store" });
            if (!response.ok || response.redirected) throw new Error("Could not load members");
            const roster = await response.text();
            if (signal.aborted || !dialog.open) return;

            content.innerHTML = roster;
            status.textContent = "";
        } catch {
            if (signal.aborted || !dialog.open) return;

            status.textContent = "Could not load members.";
            fallback.hidden = false;
        }
    }

    trigger.addEventListener("click", (event) => {
        if (event.defaultPrevented || event.button !== 0 || event.ctrlKey
            || event.metaKey || event.shiftKey || event.altKey) return;

        event.preventDefault();
        openMembers();
    });

    dialog.addEventListener("click", (event) => {
        if (event.target === dialog) dialog.close();
    });

    dialog.addEventListener("close", () => {
        if (dialog.open) return;
        loadRequest?.abort();
        if (window.location.hash === "#members") {
            history.replaceState(history.state, "", window.location.pathname + window.location.search);
        }
        trigger.focus({ preventScroll: true });
    });

    if (window.location.hash === "#members") openMembers();
})();
