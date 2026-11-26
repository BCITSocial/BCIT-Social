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

    // Members pop-up: top-layer exit animation fallback, declarative invokers fallback,
    // light-dismiss fallback, and #members hash sync.
    let supportsDisplayTransition;
    function canTransitionDisplay() {
        if (supportsDisplayTransition !== undefined) return supportsDisplayTransition;
        if (!window.CSS?.supports?.("transition-behavior", "allow-discrete") || !document.body) {
            return false;
        }
        const probe = document.createElement("div");
        probe.style.cssText = "transition: display 1s allow-discrete !important; display: block;";
        document.body.appendChild(probe);
        getComputedStyle(probe).display;
        probe.style.display = "none";
        supportsDisplayTransition = getComputedStyle(probe).display === "block";
        probe.remove();
        return supportsDisplayTransition;
    }

    function supportsTopLayerExit() {
        return Boolean(window.CSS?.supports?.("overlay", "auto") && canTransitionDisplay());
    }

    async function closeTopLayer(dialog) {
        if (!dialog.open || dialog.hasAttribute("data-closing")) return;
        if (!supportsTopLayerExit()) {
            dialog.setAttribute("data-closing", "");
            const animations = dialog.getAnimations({ subtree: true });
            if (animations.length > 0) {
                await Promise.race([
                    Promise.allSettled(animations.map((a) => a.finished)),
                    new Promise((r) => setTimeout(r, 2000)),
                ]);
            }
            if (!dialog.hasAttribute("data-closing")) return;
            dialog.removeAttribute("data-closing");
        }
        if (dialog.open) dialog.close();
    }

    if (!("commandForElement" in HTMLButtonElement.prototype)) {
        let polyfillLoaded = false;
        import("https://esm.run/invokers-polyfill")
            .then(() => {
                polyfillLoaded = true;
            })
            .catch(() => {});

        document.addEventListener("click", (event) => {
            if (polyfillLoaded || "commandForElement" in HTMLButtonElement.prototype) return;
            const button = event.target.closest("button[command][commandfor]");
            if (!button) return;
            const target = document.getElementById(button.getAttribute("commandfor"));
            if (!(target instanceof HTMLDialogElement)) return;
            const command = button.getAttribute("command");
            if (command === "show-modal" && !target.open) target.showModal();
            else if (command === "close" && target.open) closeTopLayer(target);
        });
    }

    const membersDialog = document.getElementById("community-members-modal");
    if (membersDialog) {
        membersDialog.addEventListener("cancel", (event) => {
            if (!supportsTopLayerExit()) {
                event.preventDefault();
                closeTopLayer(membersDialog);
            }
        });

        membersDialog.addEventListener("command", (event) => {
            if (event.command === "close" && !supportsTopLayerExit()) {
                event.preventDefault();
                closeTopLayer(membersDialog);
            }
        });

        if (!("closedBy" in HTMLDialogElement.prototype)) {
            const outside = (event) => {
                const rect = membersDialog.getBoundingClientRect();
                return event.clientX < rect.left
                    || event.clientX > rect.right
                    || event.clientY < rect.top
                    || event.clientY > rect.bottom;
            };
            let pressedOutside = false;
            membersDialog.addEventListener("pointerdown", (event) => {
                pressedOutside = outside(event);
            });
            membersDialog.addEventListener("pointerup", (event) => {
                if (pressedOutside && outside(event)) {
                    closeTopLayer(membersDialog);
                }
                pressedOutside = false;
            });
        }

        membersDialog.addEventListener("close", () => {
            membersDialog.removeAttribute("data-closing");
            membersDialog.returnValue = "";
            if (window.location.hash === "#members") {
                history.replaceState(null, "", window.location.pathname + window.location.search);
            }
        });

        function openMembersFromHash() {
            if (window.location.hash === "#members" && !membersDialog.open) {
                membersDialog.showModal();
            }
        }
        openMembersFromHash();
        window.addEventListener("hashchange", openMembersFromHash);

        const confirmDialog = document.getElementById("community-confirm");
        const confirmForm = confirmDialog?.querySelector("[data-confirm-form]");
        let pendingMemberForm = null;

        async function submitMemberActionForm(form) {
            const submitButton = form.querySelector("button[type='submit']");
            if (submitButton) submitButton.disabled = true;
            try {
                const response = await fetch(form.action, {
                    method: "POST",
                    body: new FormData(form),
                    credentials: "same-origin",
                });
                if (response.redirected && response.url.includes("/Account/Login")) {
                    window.location.href = response.url;
                    return;
                }
                if (response.ok) {
                    history.replaceState(null, "", window.location.pathname + window.location.search + "#members");
                    window.location.reload();
                    return;
                }
            } catch (error) {
                console.error("Failed to update community member.", error);
            }
            if (submitButton) submitButton.disabled = false;
        }

        membersDialog.addEventListener("submit", (event) => {
            const form = event.target.closest("form[data-member-action-form]");
            if (!form) return;
            event.preventDefault();

            if (form.dataset.confirmTitle && confirmDialog && confirmForm) {
                pendingMemberForm = form;
                confirmDialog.querySelector("[data-confirm-title]").textContent = form.dataset.confirmTitle;
                confirmDialog.querySelector("[data-confirm-message]").textContent = form.dataset.confirmMessage || "";
                confirmDialog.querySelector("[data-confirm-submit]").textContent = form.dataset.confirmButton || "Confirm";
                confirmDialog.querySelector("[data-confirm-cancel]").textContent = form.dataset.confirmCancel || "Cancel";
                confirmDialog.returnFocusTo = event.submitter || form.querySelector("button[type='submit']");
                confirmDialog.showModal();
                confirmDialog.querySelector("[data-confirm-cancel]")?.focus();
                return;
            }

            submitMemberActionForm(form);
        });

        if (confirmDialog && confirmForm) {
            confirmForm.addEventListener("submit", (event) => {
                if (!pendingMemberForm) return;
                event.preventDefault();
                const formToSubmit = pendingMemberForm;
                pendingMemberForm = null;
                confirmDialog.close();
                submitMemberActionForm(formToSubmit);
            });

            confirmDialog.querySelector("[data-confirm-cancel]")?.addEventListener("click", () => {
                if (pendingMemberForm) confirmDialog.close();
            });

            confirmDialog.addEventListener("close", () => {
                pendingMemberForm = null;
            });
        }
    }
});
