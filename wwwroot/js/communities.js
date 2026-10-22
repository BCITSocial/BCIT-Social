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

    // modal Open / Close
    const modal = document.getElementById("create-community-modal");
    const toggleModal = (show) => modal?.classList.toggle("open", show);

    document.getElementById("open-create-modal")?.addEventListener("click", () => {
        syncModalTags();
        toggleModal(true);
    });
    document.getElementById("close-create-modal")?.addEventListener("click", () => toggleModal(false));
    document.getElementById("cancel-create-modal")?.addEventListener("click", () => toggleModal(false));
    modal?.addEventListener("click", (e) => { if (e.target === modal) toggleModal(false); });
    document.addEventListener("keydown", (e) => { if (e.key === "Escape") toggleModal(false); });

    // modal Dropdowns -> Colored Preview & Type Sync
    const categorySelect = document.getElementById("modal-category-select");
    const tagSelect = document.getElementById("modal-tag-select");
    const statusSelect = document.getElementById("modal-status-select");
    const timeSelect = document.getElementById("modal-time-select");
    const campusSelect = document.getElementById("Campus");
    const hiddenTypeInput = document.getElementById("combined-type-input");
    const previewContainer = document.getElementById("modal-tag-preview");

    function syncModalTags() {
        if (!hiddenTypeInput) return;

        const allTags = [];
        const previewPills = [];

        // Category (Blue)
        const cat = categorySelect?.value?.trim();
        if (cat && cat !== "None") {
            allTags.push(cat);
            previewPills.push(`<span class="category-badge">${cat}</span>`);
        }

        // Tag (Blue)
        const tag = tagSelect?.value?.trim();
        if (tag && tag !== "None" && !allTags.includes(tag)) {
            allTags.push(tag);
            previewPills.push(`<span class="category-badge">${tag}</span>`);
        }

        // Status Tag (Green)
        const status = statusSelect?.value?.trim();
        if (status && status !== "None" && !allTags.includes(status)) {
            allTags.push(status);
            previewPills.push(`<span class="category-badge status-badge">${status}</span>`);
        }

        // Time Tag (Orange)
        const time = timeSelect?.value?.trim();
        if (time && time !== "None" && !allTags.includes(time)) {
            allTags.push(time);
            previewPills.push(`<span class="category-badge time-badge">${time}</span>`);
        }

        // Campus / Location (Bundled for backend persistence, not previewed as a pill)
        const campus = campusSelect?.value?.trim();
        if (campus && campus !== "None" && campus !== "All campuses" && !allTags.includes(campus)) {
            allTags.push(campus);
        }

        hiddenTypeInput.value = allTags.join(", ");

        if (previewContainer) {
            previewContainer.innerHTML = previewPills.join("");
        }
    }

    modal?.querySelectorAll("select").forEach(select => {
        select.addEventListener("change", syncModalTags);
    });
    modal?.querySelector("form")?.addEventListener("submit", syncModalTags);

    syncModalTags();
});