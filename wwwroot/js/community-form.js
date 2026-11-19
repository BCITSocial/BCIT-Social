// author: jaspreet bath
//
// Community form popup.
// Combines dropdown into hidden Type inputs that the server saves.
// Type is saved in the order category, tag, status, time, campus.
// The campus is saved but not previewed.

document.addEventListener("DOMContentLoaded", () => {
    // Order here.
    const parts = ["category", "tag", "status", "time", "campus"];
    // Badge color matching from communities.css (category&tag: blue, status: green, time: orange)
    const badgeClass = { category: "", tag: "", status: "status-badge", time: "time-badge" };

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
                    const badge = document.createElement("span");
                    badge.className = `category-badge ${badgeClass[part]}`.trim();
                    badge.textContent = value;
                    badges.push(badge);
                }
            });

            if (typeInput) typeInput.value = tags.join(", ");
            preview?.replaceChildren(...badges);
        }

        form.querySelectorAll("[data-tag-part]").forEach((select) => select.addEventListener("change", syncTags));
        form.addEventListener("submit", syncTags);
        syncTags();
    });
});