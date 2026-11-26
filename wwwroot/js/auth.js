// Account and create-profile forms: show/hide password, bio character count, live profile preview,
// and a busy state on submit so the button can't be pressed twice.
(() => {
    document.querySelectorAll("[data-reveal-password]").forEach((button) => {
        const input = document.getElementById(button.getAttribute("aria-controls"));
        if (!input) return;
        button.hidden = false;
        button.addEventListener("click", () => {
            const show = input.type === "password";
            input.type = show ? "text" : "password";
            button.textContent = show ? "Hide" : "Show";
            button.setAttribute("aria-pressed", String(show));
        });
    });

    document.querySelectorAll("[data-char-count]").forEach((output) => {
        const field = document.getElementById(output.dataset.charCount);
        if (!field) return;
        const max = field.maxLength > 0 ? field.maxLength : null;
        const update = () => { output.textContent = max ? `${field.value.length} / ${max}` : `${field.value.length}`; };
        field.addEventListener("input", update);
        update();
    });

    const preview = document.querySelector("[data-profile-preview]");
    if (preview) {
        const nameInput = document.getElementById(preview.dataset.nameField);
        const programInput = document.getElementById(preview.dataset.programField);
        const campusInput = document.getElementById(preview.dataset.campusField);
        const initials = preview.querySelector("[data-preview-initials]");
        const name = preview.querySelector("[data-preview-name]");
        const detail = preview.querySelector("[data-preview-detail]");
        const update = () => {
            const value = nameInput.value.trim();
            name.textContent = value || "Your name";
            initials.textContent = value
                ? value.split(/\s+/).slice(0, 2).map((part) => part[0].toUpperCase()).join("")
                : "?";
            detail.textContent = [programInput?.value.trim(), campusInput?.value.trim()].filter(Boolean).join(" · ") || "Program · Campus";
        };
        [nameInput, programInput, campusInput].forEach((field) => field?.addEventListener("input", update));
        update();
    }

    document.querySelectorAll("form[data-auth-form]").forEach((form) => {
        form.addEventListener("submit", () => {
            const button = form.querySelector(".auth-submit");
            if (!button || (typeof form.checkValidity === "function" && !form.checkValidity())) return;
            button.disabled = true;
            button.textContent = button.dataset.busyText || "Please wait...";
        });
    });
})();
