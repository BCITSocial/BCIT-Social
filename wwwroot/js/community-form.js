// author: jaspreet bath
//
// Community form (create pop-up, Settings pop-up, Create and Edit pages).
// - Combines the dropdowns into the hidden Type input that the server saves,
//   in the order category, tag, campus. The campus is saved but not previewed.
// - Cover picker: previews the picked image in the box (it becomes the community banner).

document.addEventListener("DOMContentLoaded", () => {
    // Order here.
    const parts = ["category", "tag", "campus"];

    document.querySelectorAll("form[data-community-form]").forEach((form) => {
        const typeInput = form.querySelector("[data-combined-type]");
        const preview = form.querySelector("[data-tag-preview]");

        function syncTags() {
            const tags = [];
            const badges = [];

            parts.forEach((part) => {
                const value = form.querySelector(`[data-tag-part="${part}"]`)?.value.trim();
                // Empty: None/All campuses. Skip repeats.
                if (!value || tags.some((tag) => tag.toLowerCase() === value.toLowerCase())) return;

                tags.push(value);
                if (part !== "campus") {
                    // Blue badge, matching communities.css.
                    const badge = document.createElement("span");
                    badge.className = "category-badge";
                    badge.textContent = value;
                    badges.push(badge);
                }
            });

            if (typeInput) typeInput.value = tags.join(", ");
            preview?.replaceChildren(...badges);
        }

        form.querySelectorAll("[data-tag-part]").forEach((select) => select.addEventListener("change", syncTags));
        form.addEventListener("submit", syncTags);
        // Cancel buttons are reset buttons; refresh the preview once the dropdowns are back.
        form.addEventListener("reset", () => setTimeout(syncTags));
        syncTags();

        // --- Cover image picker ---
        const picker = form.querySelector("[data-cover-picker]");
        const fileInput = picker?.querySelector("input[type='file']");
        const error = form.querySelector("[data-cover-error]");
        if (!picker || !fileInput) return;

        // What the box showed when the form loaded (the saved banner, or nothing).
        const startImage = picker.style.backgroundImage;
        const startHasImage = picker.classList.contains("has-image");
        let previewUrl = null;

        function showError(message) {
            if (!error) return;
            error.textContent = message || "";
            error.hidden = !message;
        }

        function resetPreview() {
            if (previewUrl) URL.revokeObjectURL(previewUrl);
            previewUrl = null;
            picker.style.backgroundImage = startImage;
            picker.classList.toggle("has-image", startHasImage);
        }

        fileInput.addEventListener("change", () => {
            const file = fileInput.files?.[0];
            showError("");
            if (!file) {
                resetPreview();
                return;
            }

            const maxBytes = Number(fileInput.dataset.maxBytes) || 0;
            if (maxBytes && file.size > maxBytes) {
                fileInput.value = "";
                resetPreview();
                showError(fileInput.dataset.tooLarge);
                return;
            }

            if (previewUrl) URL.revokeObjectURL(previewUrl);
            previewUrl = URL.createObjectURL(file);
            picker.style.backgroundImage = `url("${previewUrl}")`;
            picker.classList.add("has-image");
        });

        // Cancel / reset puts the box back the way it was.
        form.addEventListener("reset", () => {
            showError("");
            resetPreview();
        });
    });
});
