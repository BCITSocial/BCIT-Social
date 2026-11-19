// Infinite scroll for the Home feed.
// When the reader reaches the card 10 from the end, fetch the next 10 cards from /Home/Feed and append them.
// also now handles expandable post content 
(() => {
    const list = document.getElementById("feed-list");

    if (!list) {
        return;
    }

    // Applies the collapsed state only when a post is actually taller
    // than the feed's maximum content height.
    function setupExpandablePosts() {
        const posts = list.querySelectorAll("[data-post-content]");

        posts.forEach((content) => {
            if (content.dataset.expandableReady === "true") {
                return;
            }

            const button = content.parentElement.querySelector("[data-post-toggle]");

            if (!button) {
                return;
            }

            content.classList.add("is-collapsed");

            requestAnimationFrame(() => {
                const isOverflowing =
                    content.scrollHeight > content.clientHeight + 1;

                if (isOverflowing) {
                    button.hidden = false;
                }
                else {
                    content.classList.remove("is-collapsed");
                    button.hidden = true;
                }

                content.dataset.expandableReady = "true";
            });
        });
    }

    // Event delegation means this also works for posts added later
    // through infinite scrolling.
    list.addEventListener("click", (event) => {
        const button = event.target.closest("[data-post-toggle]");

        if (!button) {
            return;
        }

        const content =
            button.parentElement.querySelector("[data-post-content]");

        if (!content) {
            return;
        }

        const expanded =
            button.getAttribute("aria-expanded") === "true";

        if (expanded) {
            content.classList.add("is-collapsed");
            button.textContent = "Show more";
            button.setAttribute("aria-expanded", "false");
        }
        else {
            content.classList.remove("is-collapsed");
            button.textContent = "Show less";
            button.setAttribute("aria-expanded", "true");
        }
    });

    setupExpandablePosts();

    // Nothing below this point is needed when there are no more
    // pages to load, but Show more / Show less still works above.
    if (list.dataset.hasMore !== "true") {
        return;
    }

    const status = document.getElementById("feed-status");
    const retry = document.getElementById("feed-retry");
    const loadAhead = 10;
    let loading = false;

    const observer = new IntersectionObserver((entries) => {
        if (entries.some((entry) =>
            entry.isIntersecting ||
            entry.boundingClientRect.top < 0)) {
            loadMore();
        }
    });

    function watchTarget() {
        observer.disconnect();

        const cards = list.querySelectorAll(".post-card");
        const target =
            cards[Math.max(0, cards.length - loadAhead)];

        if (target) {
            observer.observe(target);
        }
    }

    async function loadMore() {
        if (loading) {
            return;
        }

        loading = true;
        observer.disconnect();
        retry.hidden = true;
        status.textContent = "Loading more posts…";

        const cards = list.querySelectorAll(".post-card");
        const last = cards[cards.length - 1];

        const url =
            new URL(list.dataset.feedUrl, window.location.origin);

        url.searchParams.set(
            "beforeTicks",
            last.dataset.createdTicks);

        url.searchParams.set(
            "beforeId",
            last.dataset.postId);

        url.searchParams.set(
            "filter",
            list.dataset.filter);

        const tagId = list.dataset.tagId;

        if (tagId) {
            url.searchParams.set("tagId", tagId);
        }

        try {
            const response = await fetch(url, {
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!response.ok) {
                throw new Error(
                    `Feed request failed: ${response.status}`);
            }

            list.insertAdjacentHTML(
                "beforeend",
                await response.text());

            // Newly loaded posts also need their content checked.
            setupExpandablePosts();

            if (response.headers.get("X-Feed-Has-More") === "true") {
                status.textContent = "";
                watchTarget();
            }
            else {
                status.textContent = "You're all caught up.";
            }
        }
        catch (error) {
            console.error(error);
            status.textContent = "Couldn't load more posts.";
            retry.hidden = false;
        }
        finally {
            loading = false;
        }
    }

    retry.addEventListener("click", loadMore);

    watchTarget();
})();