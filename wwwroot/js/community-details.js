// see all/show all for the Posts list on a community's page
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
            toggleBtn.setAttribute("aria-expanded", String(isExpanded));
        });
    } else if (toggleBtn) {
        // if no extra items, hide the toggle button
        toggleBtn.style.display = "none";
    }


    // Post type tags in the pop-up post modal.
    // one row, then "Show all" for rest.
    document.querySelectorAll("[data-collapsible-choices]").forEach(function (row) {
        const list = row.querySelector(".post-type-choices");
        const toggle = row.querySelector(".post-type-toggle");
        if (!list || !toggle) return;

        // Shows button if only some don't fit on the first row.
        function checkOverflow() {
            if (row.classList.contains("is-expanded")) return;

            toggle.hidden = list.scrollHeight <= list.clientHeight + 1;
        }

        function setExpanded(expanded) {
            row.classList.toggle("is-expanded", expanded);
            // param(name, value)
            toggle.setAttribute("aria-expanded", String(expanded));
            // if expanded, show less, else show all
            toggle.textContent = expanded ? toggle.dataset.showLess : toggle.dataset.showAll;
            checkOverflow();
        }

        toggle.addEventListener("click", function () {
            setExpanded(!row.classList.contains("is-expanded"));
        });

        // Tabbing onto a pill that's hidden opens the list so it can be seen.
        list.addEventListener("focusin", function (event) {
            const pill = event.target.closest(".post-type-choice");
            if (pill && pill.offsetTop + pill.offsetHeight > list.clientHeight) {
                setExpanded(true);
            }
        });

        // Closing modal resets the "list" to false.
        row.closest("form")?.addEventListener("reset", function () {
            setExpanded(false);
        });

        // Runs when the pop-up opens (the list gets a size) and when the window is resized.
        new ResizeObserver(checkOverflow).observe(list);
    });
});
