// see all/show all
// author: jaspreet bath

document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.getElementById("toggle-discussions-btn");
    const extraCards = document.querySelectorAll(".discussion-card.discussion-extra");

    if (toggleBtn && extraCards.length > 0) {
        let isExpanded = false;

        const seeAllLabel = toggleBtn.getAttribute("data-see-all") || "See all";
        const showLessLabel = toggleBtn.getAttribute("data-show-less") || "Show less";

        // click sets expanded to true
        toggleBtn.addEventListener("click", function () {
            isExpanded = !isExpanded;

            extraCards.forEach(card => {
                card.classList.toggle("is-hidden", !isExpanded);
            });

            toggleBtn.textContent = isExpanded ? showLessLabel : seeAllLabel;
        });
    } else if (toggleBtn) {
        // if no extra items, hide the toggle button
        toggleBtn.style.display = "none";
    }
});