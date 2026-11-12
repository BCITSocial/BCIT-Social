// Opens the create popups (post, community, event). Each page has one: a link or button marked
// data-open-create="<dialog id>" opens that dialog. When the dialog isn't on the page (signed out),
// links go to their href instead, e.g. /Post/Create, which asks the visitor to log in.
// data-create-prefill="<selector>" copies that field's text into the dialog's [data-create-body] field.
(() => {
    const open = (trigger) => {
        const dialog = document.getElementById(trigger.dataset.openCreate || "create-post-dialog");
        if (!dialog) return false;

        const source = trigger.dataset.createPrefill && document.querySelector(trigger.dataset.createPrefill);
        const body = dialog.querySelector("[data-create-body]");
        if (source && body && source.value.trim()) {
            body.value = source.value;
        }

        dialog.showModal();
        // Start in the first text field.
        dialog.querySelector("input:not([type=hidden]):not([type=radio]):not(:disabled), textarea")?.focus();
        return true;
    };

    document.addEventListener("click", (event) => {
        const trigger = event.target.closest("[data-open-create]");
        if (!trigger || event.ctrlKey || event.metaKey || event.shiftKey || event.button !== 0) return;
        if (open(trigger)) event.preventDefault();
    });

    // Enter in the home share bar works like pressing Create.
    document.querySelectorAll("[data-create-prefill-source]").forEach((field) => {
        field.addEventListener("keydown", (event) => {
            if (event.key !== "Enter") return;
            const trigger = document.querySelector(`[data-create-prefill="#${field.id}"]`);
            if (trigger && open(trigger)) event.preventDefault();
        });
    });

    document.querySelectorAll("dialog.create-dialog").forEach((dialog) => {
        dialog.querySelectorAll("[data-close-create]").forEach((button) => {
            button.addEventListener("click", () => dialog.close());
        });

        // A click on the backdrop lands on the dialog element itself.
        dialog.addEventListener("click", (event) => {
            if (event.target === dialog) dialog.close();
        });
    });
})();
