// Post details: opens a post in the Home modal and handles the Share button.
// Links marked data-open-post load /Home/Post/{id} into #post-dialog and update the URL,
// so Back closes the modal and the URL can be shared. Without the modal, links open the full page.
(() => {
    const dialog = document.getElementById("post-dialog");
    const body = document.getElementById("post-dialog-body");
    let returnFocus = null;

    // Collapse long comments to four lines, including comments inside opened reply threads.
    function refreshComments() {
        document.querySelectorAll("[data-comment-toggle][aria-expanded='false']").forEach(button => {
            const content = document.getElementById(button.getAttribute("aria-controls"));
            if (!content.clientHeight) return;
            content.classList.add("is-collapsed");
            button.hidden = content.scrollHeight <= content.clientHeight + 1;
            if (button.hidden) content.classList.remove("is-collapsed");
        });
    }

    document.addEventListener("click", event => {
        const button = event.target.closest("[data-comment-toggle]");
        if (!button) return;
        const expanded = button.getAttribute("aria-expanded") === "true";
        document.getElementById(button.getAttribute("aria-controls")).classList.toggle("is-collapsed", expanded);
        button.setAttribute("aria-expanded", String(!expanded));
        button.textContent = expanded ? "Show more" : "Show less";
        if (expanded) refreshComments();
    });
    document.addEventListener("toggle", refreshComments, true);
    // Keep the reply form and nested comments below their shared action row.
    document.addEventListener("click", event => {
        const button = event.target.closest("[data-comment-panel]");
        if (!button) return;
        const panel = document.getElementById(button.getAttribute("aria-controls"));
        panel.hidden = !panel.hidden;
        button.setAttribute("aria-expanded", String(!panel.hidden));
        if (!panel.hidden) refreshComments();
    });
    window.addEventListener("resize", refreshComments);
    refreshComments();

    // Show a modal for empty comments and replies, including whitespace-only text.
    function showBlankComment(event, input) {
        if (!input?.matches(".comment-form .comment-input") || input.value.trim()) return;
        event.preventDefault();
        const message = document.getElementById("blank-comment-dialog");
        if (message && !message.open) message.showModal();
    }

    document.addEventListener("invalid", (event) => showBlankComment(event, event.target), true);
    document.addEventListener("submit", (event) => {
        if (event.target.matches(".comment-form")) {
            showBlankComment(event, event.target.querySelector(".comment-input"));
        }
    });

    async function openPost(url, addToHistory) {
        try {
            const response = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });
            if (!response.ok) throw new Error(`Post request failed: ${response.status}`);
            body.innerHTML = await response.text();
        } catch (error) {
            console.error(error);
            window.location.href = url;
            return;
        }

        if (addToHistory) history.pushState({ postUrl: url }, "", url);
        if (!dialog.open) {
            returnFocus = document.activeElement;
            dialog.showModal();
        }
        body.scrollTop = 0;
        refreshComments();
    }

    // Closing goes back in history when the modal added an entry, so the URL returns to the feed.
    function closePost() {
        if (history.state?.postUrl) {
            history.back();
        } else {
            dialog.close();
        }
    }

    if (dialog) {
        document.addEventListener("click", (event) => {
            const link = event.target.closest("a[data-open-post]");
            if (!link || event.ctrlKey || event.metaKey || event.shiftKey || event.button !== 0) return;
            event.preventDefault();
            openPost(link.href, true);
        });

        dialog.querySelector("[data-close-dialog]").addEventListener("click", closePost);
        // A click on the backdrop lands on the dialog element itself.
        dialog.addEventListener("click", (event) => {
            if (event.target === dialog) closePost();
        });
        dialog.addEventListener("cancel", (event) => {
            event.preventDefault();
            closePost();
        });
        dialog.addEventListener("close", () => {
            body.innerHTML = "";
            returnFocus?.focus();
        });

        window.addEventListener("popstate", (event) => {
            if (event.state?.postUrl) {
                openPost(event.state.postUrl, false);
            } else if (dialog.open) {
                dialog.close();
            }
        });
    }

    // A click anywhere on a post card opens the post, unless it lands on something with its own click
    // (Save, Edit, Delete...) or the reader is selecting text. It goes through the title link, so the
    // modal opens where there is one and the full post page opens everywhere else.
    document.addEventListener("click", (event) => {
        const card = event.target.closest("[data-card-link]");
        if (!card || event.target.closest("a, button, form, input, textarea, select, summary, label")) return;
        if (window.getSelection()?.toString()) return;
        card.querySelector("[data-card-target]")?.click();
    });

    // Share uses the device share sheet when available, otherwise copies the link.
    document.addEventListener("click", async (event) => {
        const button = event.target.closest("[data-share-url]");
        if (!button) return;

        const url = new URL(button.dataset.shareUrl, window.location.origin).href;
        const status = button.parentElement.querySelector(".share-status");
        try {
            if (navigator.share) {
                await navigator.share({ title: button.dataset.shareTitle, url });
            } else {
                await navigator.clipboard.writeText(url);
                if (status) status.textContent = "Link copied";
            }
        } catch (error) {
            if (error.name !== "AbortError" && status) status.textContent = "Couldn't share this post.";
        }
    });
})();
