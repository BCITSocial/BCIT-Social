// wwwroot/js/communities.js
// author: jaspreet bath

document.addEventListener("DOMContentLoaded", () => {
    // Search & Pill Filtering
    const searchInput = document.getElementById("community-search");
    const pills = document.querySelectorAll("#category-pills .filter-pill");
    const cards = document.querySelectorAll("#browse-grid .community-card");
    const heading = document.getElementById("browse-heading");
    let activeCategory = "all";

    function filterCards() {
        const query = (searchInput?.value || "").trim().toLowerCase();
        let visibleCount = 0;

        cards.forEach(card => {
            const name = (card.dataset.name || "").toLowerCase();
            const rawType = (card.dataset.type || "").toLowerCase();
            const tags = rawType.split(",").map(t => t.trim()).filter(Boolean);

            const matchesCategory = activeCategory === "all" || tags.includes(activeCategory);
            const matchesSearch = !query || name.includes(query) || rawType.includes(query);

            const isVisible = matchesCategory && matchesSearch;
            card.style.display = isVisible ? "" : "none";
            if (isVisible) visibleCount++;
        });

        if (heading) {
            const baseText = heading.dataset.baseText || "Browse communities";
            heading.textContent = `${baseText} (${visibleCount})`;
        }
    }

    pills.forEach(pill => {
        pill.addEventListener("click", () => {
            pills.forEach(p => p.classList.remove("active"));
            pill.classList.add("active");
            activeCategory = (pill.dataset.category || "all").trim().toLowerCase();
            filterCards();
        });
    });

    searchInput?.addEventListener("input", filterCards);

    // The create community popup is opened and closed by create-post.js.
    const modal = document.getElementById("create-community-dialog");

    // Ticked tags (_TagPicker) plus the campus are saved together in Community.Type, comma separated.
    const tagPicker = document.getElementById("community-tags");
    const campusSelect = document.getElementById("Campus");
    const hiddenTypeInput = document.getElementById("combined-type-input");

    function syncModalTags() {
        if (!hiddenTypeInput) return;

        const allTags = [...(tagPicker?.querySelectorAll("input:checked") ?? [])].map(box => box.value);

        // Campus / Location (Bundled for backend persistence, not shown as a tag)
        const campus = campusSelect?.value?.trim();
        if (campus && campus !== "None" && campus !== "All campuses" && !allTags.includes(campus)) {
            allTags.push(campus);
        }

        hiddenTypeInput.value = allTags.join(", ");
    }

    modal?.querySelectorAll("select, input[type=checkbox]").forEach(field => {
        field.addEventListener("change", syncModalTags);
    });
    modal?.querySelector("form")?.addEventListener("submit", syncModalTags);

    syncModalTags();
});