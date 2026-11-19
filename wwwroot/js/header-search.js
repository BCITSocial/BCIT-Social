// Header search: as the user types, a dropdown under the search box shows matches
// grouped as Posts, Events, Communities, then People. Each word typed is matched on its own
// (word starts and small typos count), and title matches are listed first (see "Matching and ranking").
//
// There is no site-wide search endpoint yet, so each group uses what already exists:
// - Posts: the newest posts on the home feed ("/"), loaded once and filtered here.
// - Events: sample events below, because events are still a mockup.
// - Communities: the cards on the Communities page ("/Community"), loaded once and filtered here.
// - People: /Messages/FindUsers (signed-in users only).
// TODO(BE): replace the Posts, Events and Communities sources with real search endpoints.
(() => {
    const root = document.querySelector("[data-header-search]");
    if (!root) return;

    const input = root.querySelector("[data-header-search-input]");
    const panel = root.querySelector("[data-header-search-panel]");
    const signedIn = root.dataset.signedIn === "true";

    const MAX_PER_GROUP = 4;
    const MIN_PEOPLE_QUERY = 2; // FindUsers ignores shorter queries.
    const PEOPLE_DELAY_MS = 200;

    // TODO(BE): sample events until events come from the database.
    const SAMPLE_EVENTS = [
        { title: "Hackathon Kickoff Night", host: "BCIT Computing Society", when: "Fri, 6:00 PM", campus: "Burnaby" },
        { title: "Resume Review Drop-in", host: "Career Services", when: "Tue, 12:30 PM", campus: "Downtown" },
        { title: "Intramural Volleyball", host: "BCIT Rec Club", when: "Wed, 7:00 PM", campus: "Burnaby" },
        { title: "Study Jam: Final Exams", host: "CST Study Group", when: "Mon, 4:00 PM", campus: "Burnaby" },
        { title: "Board Games Social", host: "Tabletop Club", when: "Thu, 5:30 PM", campus: "Downtown" },
        { title: "Industry Night: Tech Careers", host: "BCIT Alumni", when: "Sat, 6:30 PM", campus: "Downtown" },
    ].map(event => ({
        title: event.title,
        meta: `${event.host} · ${event.when} · ${event.campus}`,
        href: `/Events?q=${encodeURIComponent(event.title)}`,
        fields: { title: event.title, tags: `${event.host} ${event.campus}` },
    }));

    const groups = [
        { key: "posts", label: "Posts" },
        { key: "events", label: "Events" },
        { key: "communities", label: "Communities" },
        { key: "people", label: "People" },
    ];

    const clean = value => (value ?? "").replace(/\s+/g, " ").trim();
    const lower = value => clean(value).toLowerCase();

    // ---- Data sources ----

    // Fetches a page once and turns it into a list of results; later calls reuse the same promise.
    const pageLoaders = {};
    function loadFromPage(url, parse) {
        pageLoaders[url] ??= fetch(url, { credentials: "same-origin" })
            .then(response => (response.ok ? response.text() : ""))
            .then(html => parse(new DOMParser().parseFromString(html, "text/html")))
            .catch(() => []);
        return pageLoaders[url];
    }

    const loadPosts = () => loadFromPage("/", doc =>
        [...doc.querySelectorAll(".post-card")].map(card => {
            const title = clean(card.querySelector(".post-title")?.textContent);
            const content = clean(card.querySelector(".post-content")?.textContent);
            const meta = clean(card.querySelector(".post-meta")?.textContent);
            const tags = [...card.querySelectorAll(".post-tags li")].map(tag => clean(tag.textContent)).join(" ");
            const tagLabels = tags ? tags.split(" ").map(tag => `#${tag}`).join(" ") : "";
            return {
                title,
                meta: tagLabels ? `${tagLabels} · ${meta}` : meta,
                href: `/Home/Post/${card.dataset.postId}`,
                fields: { title, tags, body: `${content} ${meta}` },
            };
        }));

    const loadCommunities = () => loadFromPage("/Community", doc => {
        const seen = new Set();
        const results = [];
        // A joined community can appear in more than one section, so keep one card per link.
        for (const card of doc.querySelectorAll(".community-card")) {
            const link = card.querySelector(".community-card-title a");
            const href = link?.getAttribute("href");
            if (!href || seen.has(href)) continue;
            seen.add(href);
            const name = clean(card.dataset.name || link.textContent);
            const members = clean(card.querySelector(".member-count")?.textContent);
            results.push({
                title: name,
                meta: members,
                href,
                fields: {
                    title: name,
                    tags: `${card.dataset.topics ?? ""} ${card.dataset.campus ?? ""}`,
                    body: clean(card.querySelector(".community-desc")?.textContent),
                },
            });
        }
        return results;
    });

    // FindUsers only matches the whole text inside a name, so ask it for the longest word
    // and let rank() check the other words and order the results.
    function peopleQuery(words) {
        return words.reduce((longest, word) => (word.length > longest.length ? word : longest), "");
    }

    async function searchPeople(words, signal) {
        const query = peopleQuery(words);
        if (!signedIn || query.length < MIN_PEOPLE_QUERY) return [];
        const url = new URL("/Messages/FindUsers", window.location.origin);
        url.searchParams.set("q", query);
        const response = await fetch(url, { credentials: "same-origin", signal, headers: { Accept: "application/json" } });
        if (!response.ok || !response.headers.get("content-type")?.includes("json")) return [];
        const people = await response.json();
        return people.map(person => ({
            title: person.name,
            meta: person.program ?? "",
            href: `/Profile/Details?userid=${encodeURIComponent(person.id)}`,
            initials: initialsOf(person.name),
            fields: { title: person.name, tags: person.program ?? "" },
        }));
    }

    // ---- Matching and ranking ----
    //
    // The query is split into words, and every word has to be found somewhere in the item,
    // in any order. A word can match a whole word ("coding"), the start of a word ("cod"),
    // the middle of a word ("ode"), or, for words of 4+ letters, a word with one typo ("codng").
    // Matches in the title count most, then tags/topics, then the rest of the text.
    // Ties keep the original order, which is newest first for posts.

    const FIELD_WEIGHTS = { title: 3, tags: 2, body: 1 };
    const MATCH_SCORES = { exact: 4, prefix: 3, inside: 2, typo: 1 };

    const wordsOf = value => lower(value).split(/[^\p{L}\p{N}]+/u).filter(Boolean);

    // True when a and b differ by at most one inserted, deleted, changed, or swapped letter.
    function withinOneEdit(a, b) {
        if (Math.abs(a.length - b.length) > 1) return false;
        let i = 0;
        while (i < a.length && i < b.length && a[i] === b[i]) i++;
        if (i === a.length && i === b.length) return true;
        const swapped = a[i] === b[i + 1] && a[i + 1] === b[i] && a.slice(i + 2) === b.slice(i + 2);
        return swapped
            || a.slice(i + 1) === b.slice(i + 1)   // one letter changed
            || a.slice(i) === b.slice(i + 1)       // one letter missing from a
            || a.slice(i + 1) === b.slice(i);      // one extra letter in a
    }

    // How well one query word matches one word of the item (0 = no match).
    function wordScore(queryWord, word) {
        if (word === queryWord) return MATCH_SCORES.exact;
        if (word.startsWith(queryWord)) return MATCH_SCORES.prefix;
        if (word.includes(queryWord)) return MATCH_SCORES.inside;
        if (queryWord.length >= 4) {
            // Compare with the whole word, and with its start so "progam" still finds "programming".
            if (withinOneEdit(queryWord, word)) return MATCH_SCORES.typo;
            for (const length of [queryWord.length - 1, queryWord.length, queryWord.length + 1]) {
                if (length < word.length && withinOneEdit(queryWord, word.slice(0, length))) return MATCH_SCORES.typo;
            }
        }
        return 0;
    }

    // The item's score for the whole query, or 0 when any query word is missing.
    function itemScore(item, queryWords) {
        let total = 0;
        for (const queryWord of queryWords) {
            let best = 0;
            for (const [field, weight] of Object.entries(FIELD_WEIGHTS)) {
                for (const word of wordsOf(item.fields?.[field])) {
                    best = Math.max(best, wordScore(queryWord, word) * weight);
                }
            }
            if (best === 0) return 0;
            total += best;
        }
        // Small bonus when the title contains the query as typed, for example "soccer club".
        if (lower(item.fields?.title).includes(queryWords.join(" "))) total += 5;
        return total;
    }

    function rank(items, queryWords) {
        return items
            .map((item, index) => ({ item, index, score: itemScore(item, queryWords) }))
            .filter(result => result.score > 0)
            .sort((a, b) => b.score - a.score || a.index - b.index)
            .slice(0, MAX_PER_GROUP)
            .map(result => result.item);
    }

    function initialsOf(name) {
        return clean(name).split(" ").filter(Boolean).slice(0, 2).map(part => part[0].toUpperCase()).join("");
    }

    // ---- Rendering ----

    // Wraps every place a query word appears in <mark>, built with text nodes so nothing is parsed as HTML.
    function highlighted(text, queryWords) {
        const fragment = document.createDocumentFragment();
        const lowerText = text.toLowerCase();
        const marked = new Array(text.length).fill(false);
        for (const word of queryWords) {
            for (let index = lowerText.indexOf(word); index >= 0; index = lowerText.indexOf(word, index + 1)) {
                marked.fill(true, index, index + word.length);
            }
        }
        let start = 0;
        while (start < text.length) {
            let end = start;
            while (end < text.length && marked[end] === marked[start]) end++;
            if (marked[start]) {
                const mark = document.createElement("mark");
                mark.textContent = text.slice(start, end);
                fragment.append(mark);
            } else {
                fragment.append(text.slice(start, end));
            }
            start = end;
        }
        return fragment;
    }

    function resultItem(group, item, queryWords, id) {
        const link = document.createElement("a");
        link.className = "search-result";
        link.href = item.href;
        link.id = id;
        link.setAttribute("role", "option");
        link.tabIndex = -1;

        const icon = document.createElement("span");
        icon.className = `search-result-icon search-result-icon-${group}`;
        icon.setAttribute("aria-hidden", "true");
        icon.textContent = group === "people" ? item.initials : group === "communities" ? initialsOf(item.title) : "";
        link.append(icon);

        const body = document.createElement("span");
        body.className = "search-result-body";
        const title = document.createElement("span");
        title.className = "search-result-title";
        title.append(highlighted(item.title, queryWords));
        body.append(title);
        if (item.meta) {
            const meta = document.createElement("span");
            meta.className = "search-result-meta";
            meta.append(highlighted(item.meta, queryWords));
            body.append(meta);
        }
        link.append(body);
        return link;
    }

    function render(queryWords, results, { loading = false } = {}) {
        panel.replaceChildren();
        let count = 0;
        for (const group of groups) {
            const items = results[group.key] ?? [];
            if (items.length === 0) continue;
            const section = document.createElement("section");
            section.className = "search-group";
            section.setAttribute("role", "group");
            const heading = document.createElement("h2");
            heading.className = "search-group-title";
            heading.id = `header-search-group-${group.key}`;
            heading.textContent = group.label;
            section.setAttribute("aria-labelledby", heading.id);
            section.append(heading);
            items.forEach((item, index) => {
                section.append(resultItem(group.key, item, queryWords, `header-search-${group.key}-${index}`));
                count++;
            });
            panel.append(section);
        }

        if (count === 0) {
            const empty = document.createElement("p");
            empty.className = "search-empty";
            empty.textContent = loading ? "Searching..." : `No results for "${clean(input.value)}"`;
            panel.append(empty);
        }
        setActive(-1);
        open();
    }

    // ---- Open, close, and keyboard ----

    let activeIndex = -1;
    const options = () => [...panel.querySelectorAll(".search-result")];

    function setActive(index) {
        const items = options();
        activeIndex = items.length === 0 ? -1 : Math.max(-1, Math.min(index, items.length - 1));
        items.forEach((item, i) => item.classList.toggle("is-active", i === activeIndex));
        const active = items[activeIndex];
        if (active) {
            input.setAttribute("aria-activedescendant", active.id);
            active.scrollIntoView({ block: "nearest" });
        } else {
            input.removeAttribute("aria-activedescendant");
        }
    }

    function open() {
        panel.hidden = false;
        input.setAttribute("aria-expanded", "true");
    }

    function close() {
        panel.hidden = true;
        input.setAttribute("aria-expanded", "false");
        setActive(-1);
    }

    // ---- Searching ----

    let version = 0;
    let peopleTimer;
    let peopleRequest;

    async function search() {
        const queryWords = wordsOf(input.value);
        const current = ++version;
        clearTimeout(peopleTimer);
        peopleRequest?.abort();

        if (queryWords.length === 0) {
            close();
            return;
        }

        const [posts, communities] = await Promise.all([loadPosts(), loadCommunities()]);
        if (current !== version) return;

        const results = {
            posts: rank(posts, queryWords),
            events: rank(SAMPLE_EVENTS, queryWords),
            communities: rank(communities, queryWords),
            people: [],
        };
        const waitingForPeople = signedIn && peopleQuery(queryWords).length >= MIN_PEOPLE_QUERY;
        render(queryWords, results, { loading: waitingForPeople });
        if (!waitingForPeople) return;

        // People come from the server, so wait until typing pauses.
        peopleTimer = setTimeout(async () => {
            peopleRequest = new AbortController();
            try {
                results.people = rank(await searchPeople(queryWords, peopleRequest.signal), queryWords);
            } catch {
                return; // Aborted by newer typing, or the request failed: keep the other groups.
            }
            if (current === version) render(queryWords, results);
        }, PEOPLE_DELAY_MS);
    }

    input.addEventListener("input", search);
    input.addEventListener("focus", () => {
        // Start loading the page data early so the first results appear quickly.
        loadPosts();
        loadCommunities();
        if (clean(input.value)) search();
    });

    input.addEventListener("keydown", event => {
        const items = options();
        if (event.key === "ArrowDown") {
            event.preventDefault();
            if (panel.hidden && clean(input.value)) search();
            else setActive(activeIndex + 1 >= items.length ? 0 : activeIndex + 1);
        } else if (event.key === "ArrowUp") {
            event.preventDefault();
            setActive(activeIndex <= 0 ? items.length - 1 : activeIndex - 1);
        } else if (event.key === "Enter") {
            const target = items[activeIndex] ?? items[0];
            if (target && !panel.hidden) {
                event.preventDefault();
                window.location.href = target.href;
            }
        } else if (event.key === "Escape") {
            if (!panel.hidden) {
                event.preventDefault();
                close();
            } else {
                input.value = "";
            }
        }
    });

    // Keep the dropdown open while clicking inside it; close it when focus or clicks leave the search.
    document.addEventListener("pointerdown", event => {
        if (!root.contains(event.target)) close();
    });
    root.addEventListener("focusout", event => {
        if (!root.contains(event.relatedTarget)) close();
    });
    panel.addEventListener("pointermove", event => {
        const item = event.target.closest(".search-result");
        if (item) setActive(options().indexOf(item));
    });
})();
