// Post details: opens a post in the Home modal and handles the Share button.
// Links marked data-open-post load /Home/Post/{id} into #post-dialog and update the URL,
// so Back closes the modal and the URL can be shared. Without the modal, links open the full page.
(() => {
    const dialog = document.getElementById("post-dialog");
    const body = document.getElementById("post-dialog-body");
    let returnFocus = null;

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
