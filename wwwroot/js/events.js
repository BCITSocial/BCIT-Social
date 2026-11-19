// Keep the optional end date after the start date on both event forms.
document.querySelectorAll("[data-event-date-range]").forEach((form) => {
    const start = form.querySelector('input[name="EventDate" i]');
    const end = form.querySelector('input[name="EndsAt" i]');
    if (!start || !end) return;

    const validate = () => {
        const minimum = start.value || start.min;
        if (minimum) end.min = minimum;
        else end.removeAttribute("min");
        end.setCustomValidity(start.value && end.value && end.valueAsNumber <= start.valueAsNumber
            ? "End time must be after the start time." : "");
    };
    start.addEventListener("input", validate);
    end.addEventListener("input", validate);
    start.addEventListener("change", validate);
    end.addEventListener("change", validate);
    validate();
});

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

// "Create event" pop-up: open / close, check the fields when the form is sent, and show the result.
// Errors appear under each field and in a summary at the top; the first field with a problem gets focus.
(() => {
    const dialog = document.getElementById("create-event-dialog");
    const form = dialog?.querySelector("[data-create-event]");
    if (!dialog || !form) return;

    const message = form.querySelector("[data-form-message]");
    const submit = form.querySelector("[type=submit]");
    const submitText = submit.textContent;
    const initiallyDisabled = submit.disabled;
    const start = form.querySelector("#event-start");
    const end = form.querySelector("#event-end");

    document.querySelectorAll("[data-open-event]").forEach((button) => {
        button.addEventListener("click", () => dialog.showModal());
    });
    dialog.querySelectorAll("[data-close-event]").forEach((button) => {
        button.addEventListener("click", () => dialog.close());
    });
    // A click on the backdrop lands on the dialog element itself.
    dialog.addEventListener("click", (event) => {
        if (event.target === dialog) dialog.close();
    });

    const showMessage = (text, kind) => {
        message.textContent = text;
        message.className = "form-message is-" + kind;
        message.hidden = false;
    };

    const setError = (field, text) => {
        const error = document.getElementById(field.id + "-error");
        field.setAttribute("aria-invalid", text ? "true" : "false");
        if (error) {
            error.textContent = text;
            error.hidden = !text;
        }
    };

    // Returns the error text for one field, or "" when it is fine. Disabled fields are skipped.
    const check = (field) => {
        if (field.disabled) return "";
        if (field.required && !field.value.trim()) return field.dataset.error;
        if (field === start && start.value && new Date(start.value) < new Date()) return start.dataset.errorPast;
        if (field === end && end.value && start.value && new Date(end.value) <= new Date(start.value)) return end.dataset.error;
        return "";
    };

    const fields = [...form.querySelectorAll("[data-error]")];

    // After a field has shown an error, re-check it as the user fixes it.
    fields.forEach((field) => {
        field.addEventListener("input", () => {
            if (field.getAttribute("aria-invalid") === "true") setError(field, check(field));
        });
        field.addEventListener("change", () => {
            if (field.getAttribute("aria-invalid") === "true") setError(field, check(field));
        });
    });

    form.addEventListener("submit", (event) => {
        let firstBad = null;
        fields.forEach((field) => {
            const error = check(field);
            setError(field, error);
            if (error && !firstBad) firstBad = field;
        });

        if (firstBad) {
            event.preventDefault();
            showMessage(submit.dataset.errorSummary, "error");
            firstBad.focus();
            return;
        }

        // Sending: block a second click and show the "Creating..." text until the page reloads.
        submit.disabled = true;
        submit.textContent = submit.dataset.submittingText;
    });

    // Closing clears the messages and errors so the form opens clean next time. Typed values are kept.
    dialog.addEventListener("close", () => {
        message.hidden = true;
        fields.forEach((field) => setError(field, ""));
        submit.disabled = initiallyDisabled;
        submit.textContent = submitText;
    });
})();
