// wwwroot/js/communities.js
// author: jaspreet bath
//
// Communities page: search and filter dropdowns (Type, Campus, Status).
// Also opens and closes the create pop-up, here and on the Details page.
// The tags are handled by community-form.js.

document.addEventListener("DOMContentLoaded", () => {
  // --- Search & filter dropdowns ---
  const filterForm = document.getElementById("community-filters");
  const searchInput = document.getElementById("community-search");
  const filterSelects = [
    ...document.querySelectorAll("#community-filters [data-filter]"),
  ];
  const cards = [...document.querySelectorAll("#browse-grid .community-card")];
  const heading = document.getElementById("browse-heading");
  const emptyMessage = document.getElementById("browse-empty");

  const lower = (value) => (value || "").trim().toLowerCase();

  // A card values for one filter.
  // data-topics values are separated by "|".
  const cardValues = (card, filter) =>
    (card.dataset[filter] || "").split("|").map(lower).filter(Boolean);

  function filterCards() {
    const query = lower(searchInput?.value);
    const chosen = filterSelects
      .map((select) => ({
        filter: select.dataset.filter,
        value: lower(select.value),
      }))
      .filter((choice) => choice.value);
    let visibleCount = 0;

    filterSelects.forEach((select) =>
      select.classList.toggle("is-active", select.value !== ""),
    );

    cards.forEach((card) => {
      const matchesFilters = chosen.every((choice) =>
        cardValues(card, choice.filter).includes(choice.value),
      );
      const searchText = [
        card.dataset.name,
        card.dataset.topics,
        card.dataset.status,
        card.dataset.campus,
      ]
        .map(lower)
        .join(" ");
      const isVisible =
        matchesFilters && (!query || searchText.includes(query));

      card.style.display = isVisible ? "" : "none";
      if (isVisible) visibleCount++;
    });

    if (heading) {
      const baseText = heading.dataset.baseText || "Browse communities";
      heading.textContent = `${baseText} (${visibleCount})`;
    }
    if (emptyMessage) {
      emptyMessage.hidden = visibleCount > 0;
    }
  }

  if (filterForm) {
    filterForm.addEventListener("input", filterCards);
    filterForm.addEventListener("change", filterCards);
    // Filtering happens on the page.
    filterForm.addEventListener("submit", (event) => event.preventDefault());
    // "Clear" is the reset button.
    filterForm.addEventListener("reset", () => setTimeout(filterCards));
    filterCards();
  }

  // Creating the pop-up open/close
  const modal = document.getElementById("create-community-modal");
  if (!modal) return;
  const openButton = document.getElementById("open-create-modal");

  function openModal() {
    modal.classList.add("open");
    modal.setAttribute("aria-hidden", "false");
    modal
      .querySelector("input:not([type='hidden']), textarea, select")
      ?.focus();
  }

  function closeModal() {
    if (!modal.classList.contains("open")) return;
    modal.classList.remove("open");
    modal.setAttribute("aria-hidden", "true");
    openButton?.focus();
  }

  openButton?.addEventListener("click", openModal);
  document
    .getElementById("close-create-modal")
    ?.addEventListener("click", closeModal);
  document
    .getElementById("cancel-create-modal")
    ?.addEventListener("click", closeModal);
  modal.addEventListener("click", (event) => {
    if (event.target === modal) closeModal();
  });
  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") closeModal();
  });
});
