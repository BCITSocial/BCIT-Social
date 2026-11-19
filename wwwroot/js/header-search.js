// Header search: as the user types, a dropdown under the search box shows matches
// grouped as Posts, Events, Communities, then People.
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
        text: `${event.title} ${event.host} ${event.campus}`,
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
            return {
                title,
                meta,
                href: `/Home/Post/${card.dataset.postId}`,
                text: `${title} ${content} ${tags} ${meta}`,
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
                text: `${name} ${card.dataset.topics ?? ""} ${card.dataset.campus ?? ""}`,
            });
        }
        return results;
    });

    async function searchPeople(query, signal) {
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
        }));
    }

    const matches = (items, query) => items.filter(item => lower(item.text).includes(query)).slice(0, MAX_PER_GROUP);

    function initialsOf(name) {
        return clean(name).split(" ").filter(Boolean).slice(0, 2).map(part => part[0].toUpperCase()).join("");
    }

    // ---- Rendering ----

    // Wraps the first match of the query in <mark>, built with text nodes so nothing is parsed as HTML.
    function highlighted(text, query) {
        const fragment = document.createDocumentFragment();
        const index = text.toLowerCase().indexOf(query);
        if (!query || index < 0) {
            fragment.append(text);
            return fragment;
        }
        const mark = document.createElement("mark");
        mark.textContent = text.slice(index, index + query.length);
        fragment.append(text.slice(0, index), mark, text.slice(index + query.length));
        return fragment;
    }

    function resultItem(group, item, query, id) {
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
        title.append(highlighted(item.title, query));
        body.append(title);
        if (item.meta) {
            const meta = document.createElement("span");
            meta.className = "search-result-meta";
            meta.textContent = item.meta;
            body.append(meta);
        }
        link.append(body);
        return link;
    }

    function render(query, results, { loading = false } = {}) {
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
                section.append(resultItem(group.key, item, query, `header-search-${group.key}-${index}`));
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
        const query = lower(input.value);
        const current = ++version;
        clearTimeout(peopleTimer);
        peopleRequest?.abort();

        if (!query) {
            close();
            return;
        }

        const [posts, communities] = await Promise.all([loadPosts(), loadCommunities()]);
        if (current !== version) return;

        const results = {
            posts: matches(posts, query),
            events: matches(SAMPLE_EVENTS, query),
            communities: matches(communities, query),
            people: [],
        };
        const waitingForPeople = signedIn && query.length >= MIN_PEOPLE_QUERY;
        render(query, results, { loading: waitingForPeople });
        if (!waitingForPeople) return;

        // People come from the server, so wait until typing pauses.
        peopleTimer = setTimeout(async () => {
            peopleRequest = new AbortController();
            try {
                results.people = (await searchPeople(query, peopleRequest.signal)).slice(0, MAX_PER_GROUP);
            } catch {
                return; // Aborted by newer typing, or the request failed: keep the other groups.
            }
            if (current === version) render(query, results);
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
