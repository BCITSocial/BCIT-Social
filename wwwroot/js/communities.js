// wwwroot/js/communities.js
// author: jaspreet bath
//
// Communities page:
// - Search and filter dropdowns (Topic, Campus) hide cards that don't match, in both sections.
// - "Your communities" shows every match.
// - "Browse communities" shows 3 cards. "See all" shows 8 per page, with a pager
//   (Prev, page numbers, Next, End) under them. "Show less" goes back to 3.
// Pop-ups and the "Joined" menus are handled by community-actions.js; tags by community-form.js.

document.addEventListener("DOMContentLoaded", () => {
    const filterForm = document.getElementById("community-filters");
    const searchInput = document.getElementById("community-search");
    const filterSelects = [...document.querySelectorAll("#community-filters [data-filter]")];

    const yourCards = [...document.querySelectorAll("#your-grid .community-card")];
    const yourCount = document.getElementById("your-count");
    const yourEmpty = document.getElementById("your-empty");

    const browseCards = [...document.querySelectorAll("#browse-grid .community-card")];
    const browseCount = document.getElementById("browse-count");
    const browseEmpty = document.getElementById("browse-empty");
    const browseHeading = document.getElementById("browse-heading");
    const browsePage = document.getElementById("browse-page");
    const browseToggle = document.getElementById("browse-toggle");
    const pager = document.getElementById("browse-pager");

    // Browse, prevCount = 3, page size = 8.
    const toCount = (value, fallback) => {
        const number = Number.parseInt(value, 10);
        return Number.isFinite(number) && number > 0 ? number : fallback;
    };
    const previewCount = toCount(browsePage?.dataset.previewCount, 3);
    const pageSize = toCount(browsePage?.dataset.pageSize, 8);

    // Start on page 1 of the pages, with the pager under them (no "See all" click needed).
    let expanded = true;
    let page = 1;
    let matchingBrowse = browseCards;

    // --- Search & filter dropdowns ---
    const lower = (value) => (value || "").trim().toLowerCase();

    // A card values for one filter.
    // data-topics values are separated by "|".
    const cardValues = (card, filter) =>
        (card.dataset[filter] || "").split("|").map(lower).filter(Boolean);

    // A card matching the search query and chosen filters.
    function matches(card, query, chosen) {
        const matchesFilters = chosen.every((choice) => cardValues(card, choice.filter).includes(choice.value));
        const searchText = [card.dataset.name, card.dataset.topics, card.dataset.status, card.dataset.campus]
            .map(lower)
            .join(" ");
        return matchesFilters && (!query || searchText.includes(query));
    }

    // Empty message: "none joined/none to browse" vs "nothing matches your search".
    function showEmpty(element, count, filtering) {
        if (!element) return;
        element.hidden = count > 0;
        const message = filtering ? element.dataset.emptyFiltered : element.dataset.emptyNone;
        if (message) element.textContent = message;
    }

    function applyFilters() {
        const query = lower(searchInput?.value);
        const chosen = filterSelects
            .map((select) => ({ filter: select.dataset.filter, value: lower(select.value) }))
            .filter((choice) => choice.value);
        const filtering = Boolean(query) || chosen.length > 0;

        filterSelects.forEach((select) =>
            select.classList.toggle("is-active", select.value !== ""),
        );

        let yourVisible = 0;
        yourCards.forEach((card) => {
            const isVisible = matches(card, query, chosen);
            card.hidden = !isVisible;
            if (isVisible) yourVisible++;
        });
        if (yourCount) yourCount.textContent = String(yourVisible);
        showEmpty(yourEmpty, yourVisible, filtering);

        matchingBrowse = browseCards.filter((card) => matches(card, query, chosen));
        if (browseCount) browseCount.textContent = String(matchingBrowse.length);
        showEmpty(browseEmpty, matchingBrowse.length, filtering);

        page = 1;
        renderBrowse();
    }

    // --- Browse: preview (3) or pages (8) ---
    const totalPages = () => Math.max(1, Math.ceil(matchingBrowse.length / pageSize));

    function renderBrowse() {
        const shown = expanded
            ? matchingBrowse.slice((page - 1) * pageSize, page * pageSize)
            : matchingBrowse.slice(0, previewCount);
        const visible = new Set(shown);
        browseCards.forEach((card) => { card.hidden = !visible.has(card); });

        if (browseToggle) {
            // Nothing more to see: no "See all" (but keep "Show less" while expanded).
            browseToggle.hidden = !expanded && matchingBrowse.length <= previewCount;
            browseToggle.textContent = expanded ? browseToggle.dataset.showLess : browseToggle.dataset.seeAll;
            browseToggle.setAttribute("aria-expanded", String(expanded));
        }
        browsePage?.classList.toggle("is-paged", expanded && matchingBrowse.length > 0);
        renderPager();
    }

    // Page numbers to show: all of them when there are few, otherwise
    // the first, the last, and the ones around the current page ("gap" = …).
    function pageNumbers(current, total) {
        if (total <= 7) {
            return Array.from({ length: total }, (_, index) => index + 1);
        }
        const pages = new Set([1, total, current - 1, current, current + 1]);
        if (current <= 3) [2, 3, 4].forEach((number) => pages.add(number));
        if (current >= total - 2) [total - 3, total - 2, total - 1].forEach((number) => pages.add(number));

        const sorted = [...pages].filter((number) => number >= 1 && number <= total).sort((a, b) => a - b);
        const result = [];
        sorted.forEach((number, index) => {
            if (index > 0 && number - sorted[index - 1] > 1) result.push("gap");
            result.push(number);
        });
        return result;
    }

    function pagerButton(label, targetPage, role, { current = false, disabled = false, ariaLabel = "" } = {}) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "pager-btn";
        button.textContent = label;
        button.dataset.page = String(targetPage);
        button.dataset.role = role;
        button.disabled = disabled;
        if (current) button.setAttribute("aria-current", "page");
        if (ariaLabel) button.setAttribute("aria-label", ariaLabel);
        return button;
    }

    function pagerSpan(className, text) {
        const span = document.createElement("span");
        span.className = className;
        span.textContent = text;
        span.setAttribute("aria-hidden", "true");
        return span;
    }

    function renderPager() {
        if (!pager) return;
        const total = totalPages();
        const show = expanded && total > 1;
        pager.hidden = !show;
        if (!show) {
            pager.replaceChildren();
            return;
        }

        const items = [
            pagerButton(`‹ ${pager.dataset.prev || "Prev"}`, page - 1, "prev", { disabled: page === 1 }),
        ];
        pageNumbers(page, total).forEach((number) => {
            items.push(number === "gap"
                ? pagerSpan("pager-gap", "…")
                : pagerButton(String(number), number, "page", {
                    current: number === page,
                    ariaLabel: `Page ${number} of ${total}`,
                }));
        });
        items.push(
            pagerButton(`${pager.dataset.next || "Next"} ›`, page + 1, "next", { disabled: page === total }),
            pagerSpan("pager-divider", ""),
            pagerButton(`${pager.dataset.end || "End"} »`, total, "end", { disabled: page === total }),
        );
        pager.replaceChildren(...items);
    }

    pager?.addEventListener("click", (event) => {
        const button = event.target.closest("button[data-page]");
        if (!button || button.disabled) return;
        const target = Number.parseInt(button.dataset.page, 10);
        if (!Number.isInteger(target) || target === page) return;

        const role = button.dataset.role;
        page = Math.min(Math.max(target, 1), totalPages());
        renderBrowse();

        // The pager was rebuilt, so put focus back on the matching button.
        const sameButton = role === "page" ? null : pager.querySelector(`[data-role="${role}"]:not(:disabled)`);
        (sameButton || pager.querySelector("[aria-current='page']"))?.focus();

        // Bring the top of the list back into view on long pages.
        if (browseHeading && browseHeading.getBoundingClientRect().top < 0) {
            const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
            browseHeading.scrollIntoView({ behavior: reduceMotion ? "auto" : "smooth", block: "start" });
        }
    });

    browseToggle?.addEventListener("click", () => {
        expanded = !expanded;
        page = 1;
        renderBrowse();
    });

    // Search toggle, closing also clears the search.
    const searchBox = searchInput?.closest(".filter-search");
    const searchIcon = searchBox?.querySelector(".filter-search-icon");
    const searchIsOpen = () => searchBox.matches(":focus-within") || searchInput.value !== "";

    function closeSearch() {
        if (searchInput.value !== "") {
            searchInput.value = "";
            applyFilters();
        }
        searchInput.blur();
    }

    // While it's open, keep the click from moving focus (otherwise the box flickers closed and back open).
    searchIcon?.addEventListener("mousedown", (event) => {
        if (searchIsOpen()) event.preventDefault();
    });
    searchIcon?.addEventListener("click", (event) => {
        if (!searchIsOpen()) return; // closed: the label opens it and puts the cursor in the box
        event.preventDefault();
        closeSearch();
    });
    // Esc closes it too.
    searchInput?.addEventListener("keydown", (event) => {
        if (event.key === "Escape") closeSearch();
    });

    if (filterForm) {
        filterForm.addEventListener("input", applyFilters);
        filterForm.addEventListener("change", applyFilters);
        // Filtering happens on the page.
        filterForm.addEventListener("submit", (event) => event.preventDefault());
        // "Clear" is the reset button.
        filterForm.addEventListener("reset", () => setTimeout(applyFilters));
    }
    applyFilters();
});