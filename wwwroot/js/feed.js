// Infinite scroll for the Home feed.
// When the reader reaches the card 10 from the end, fetch the next 10 cards from /Home/Feed and append them.
(() => {
    const list = document.getElementById("feed-list");
    if (!list || list.dataset.hasMore !== "true") return;

    const status = document.getElementById("feed-status");
    const retry = document.getElementById("feed-retry");
    const loadAhead = 10;
    let loading = false;

    const observer = new IntersectionObserver((entries) => {
        // Also load when the reader scrolled past the target before it was observed.
        if (entries.some((entry) => entry.isIntersecting || entry.boundingClientRect.top < 0)) {
            loadMore();
        }
    });

    function watchTarget() {
        observer.disconnect();
        const cards = list.querySelectorAll(".post-card");
        const target = cards[Math.max(0, cards.length - loadAhead)];
        if (target) observer.observe(target);
    }

    async function loadMore() {
        if (loading) return;
        loading = true;
        observer.disconnect();
        retry.hidden = true;
        status.textContent = "Loading more posts…";

        const cards = list.querySelectorAll(".post-card");
        const last = cards[cards.length - 1];
        const url = new URL(list.dataset.feedUrl, window.location.origin);
        url.searchParams.set("beforeTicks", last.dataset.createdTicks);
        url.searchParams.set("beforeId", last.dataset.postId);

        try {
            const response = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });
            if (!response.ok) throw new Error(`Feed request failed: ${response.status}`);

            list.insertAdjacentHTML("beforeend", await response.text());
            if (response.headers.get("X-Feed-Has-More") === "true") {
                status.textContent = "";
                watchTarget();
            } else {
                status.textContent = "You're all caught up.";
            }
        } catch (error) {
            console.error(error);
            status.textContent = "Couldn't load more posts.";
            retry.hidden = false;
        } finally {
            loading = false;
        }
    }

    retry.addEventListener("click", loadMore);
    watchTarget();
})();
