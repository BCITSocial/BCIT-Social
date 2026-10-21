// Messages page: sending, conversation list search, the details panel, mute, and search in conversation.
// Without JavaScript the page still works through links; this script updates it in place.
// Everything here stays in the browser. Nothing is sent to the server, so sent messages and mute
// last until the page reloads. Saving them is up to the backend.
(() => {
    const page = document.querySelector("[data-messages]");
    if (!page) return;

    const chat = page.querySelector(".msg-chat[data-conversation-id]");
    const activeId = chat?.dataset.conversationId;
    const log = chat?.querySelector(".msg-log");
    const scroller = chat?.querySelector(".msg-scroll");
    const typingRow = chat?.querySelector("[data-typing]");
    const form = chat?.querySelector("[data-message-form]");
    const timeFormat = new Intl.DateTimeFormat("en-US", { hour: "numeric", minute: "2-digit" });

    const dayKey = (date) => `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
    // Local time without a time zone ("2026-10-21T10:24:00"), the same format the Razor view writes.
    const isoLocal = (date) => `${dayKey(date)}T${date.toTimeString().slice(0, 8)}`;

    // ---- Message list ----

    function dayLabel(date) {
        const today = new Date();
        const yesterday = new Date(today.getFullYear(), today.getMonth(), today.getDate() - 1);
        if (dayKey(date) === dayKey(today)) return "Today";
        if (dayKey(date) === dayKey(yesterday)) return "Yesterday";
        return date.toLocaleDateString("en-US", { weekday: "long", month: "short", day: "numeric" });
    }

    // The newest message is at scrollTop 0 because the scroller uses column-reverse.
    const scrollToBottom = () => scroller.scrollTo({ top: 0, behavior: "smooth" });

    function fromTemplate(name) {
        return chat.querySelector(`template[data-template="${name}"]`).content.firstElementChild.cloneNode(true);
    }

    // Adds a day divider first when the message is on a later day than the last one shown.
    function ensureDay(date) {
        const days = log.querySelectorAll(".msg-day");
        const key = dayKey(date);
        if (days[days.length - 1]?.dataset.day === key) return;
        const divider = fromTemplate("day");
        divider.dataset.day = key;
        divider.querySelector("span").textContent = dayLabel(date);
        log.insertBefore(divider, typingRow);
    }

    // message: { text, sentAt (Date), isMine }
    function addMessage(message) {
        ensureDay(message.sentAt);
        const row = fromTemplate(message.isMine ? "mine" : "theirs");
        row.querySelector("[data-text]").textContent = message.text;
        const time = row.querySelector("time");
        time.dateTime = isoLocal(message.sentAt);
        time.textContent = timeFormat.format(message.sentAt);
        log.insertBefore(row, typingRow);
        scrollToBottom();
    }

    // ---- Conversation list ----

    // Shows the latest message in the open conversation's row and moves the row to the top.
    function updateListRow(message) {
        const link = page.querySelector(`.msg-conversation[data-conversation-id="${activeId}"]`);
        if (!link) return;
        link.querySelector(".msg-preview").textContent = (message.isMine ? "You: " : "") + message.text;
        let time = link.querySelector(".msg-conversation-top time");
        if (!time) {
            time = document.createElement("time");
            link.querySelector(".msg-conversation-top").append(time);
        }
        time.dateTime = isoLocal(message.sentAt);
        time.textContent = timeFormat.format(message.sentAt);
        const item = link.closest("li");
        item.parentElement.prepend(item);
    }

    // ---- Sending ----

    if (form) {
        const input = form.querySelector("textarea[name='text']");
        const sendButton = form.querySelector(".msg-send");

        // Grow with the text up to the CSS max-height, then scroll inside the box.
        const resize = () => {
            input.style.height = "auto";
            input.style.height = `${input.scrollHeight + 2}px`;
        };
        const updateButton = () => { sendButton.disabled = input.value.trim() === ""; };

        form.addEventListener("submit", (event) => {
            event.preventDefault();
            const text = input.value.trim();
            if (!text) return;
            const message = { text, sentAt: new Date(), isMine: true };
            addMessage(message);
            updateListRow(message);
            input.value = "";
            resize();
            updateButton();
            input.focus();
        });

        // Enter sends, Shift+Enter adds a new line. isComposing skips Enter that confirms an IME word.
        input.addEventListener("keydown", (event) => {
            if (event.key === "Enter" && !event.shiftKey && !event.isComposing) {
                event.preventDefault();
                form.requestSubmit();
            }
        });

        input.addEventListener("input", () => {
            resize();
            updateButton();
        });

        updateButton();
    }

    // ---- Conversation list search ----

    const listSearch = page.querySelector("[data-message-search]");
    const listEmpty = page.querySelector("[data-message-search-empty]");
    const listStatus = page.querySelector("[data-message-search-status]");
    listSearch?.addEventListener("input", () => {
        const query = listSearch.value.trim().toLowerCase();
        let shown = 0;
        page.querySelectorAll(".msg-conversations > li").forEach((item) => {
            const match = !query || item.textContent.toLowerCase().includes(query);
            item.hidden = !match;
            if (match) shown += 1;
        });
        if (listEmpty) listEmpty.hidden = shown > 0;
        listStatus.textContent = query ? `${shown} conversation${shown === 1 ? "" : "s"} found` : "";
    });

    // ---- Details panel ----

    const details = page.querySelector("#msg-details");
    const detailsOpen = page.querySelector("[data-details-open]");
    const detailsClose = page.querySelector("[data-details-close]");
    const isPlainClick = (event) => !(event.ctrlKey || event.metaKey || event.shiftKey || event.button !== 0);

    // Keeps ?details= in the URL so a refresh or shared link opens the same view.
    function setDetails(open, url) {
        details.hidden = !open;
        page.classList.toggle("is-details-open", open);
        history.replaceState(history.state, "", url);
        if (open) {
            details.focus();
        } else {
            detailsOpen.focus();
        }
    }

    if (details && detailsOpen && detailsClose) {
        detailsOpen.addEventListener("click", (event) => {
            if (!isPlainClick(event)) return;
            event.preventDefault();
            setDetails(true, detailsOpen.href);
        });
        detailsClose.addEventListener("click", (event) => {
            if (!isPlainClick(event)) return;
            event.preventDefault();
            setDetails(false, detailsClose.href);
        });
        details.addEventListener("keydown", (event) => {
            if (event.key === "Escape") setDetails(false, detailsClose.href);
        });
    }

    // ---- Mute (on screen only) ----

    page.querySelector("[data-conversation-mute]")?.addEventListener("click", (event) => {
        const button = event.currentTarget;
        const muted = button.getAttribute("aria-pressed") !== "true";
        button.setAttribute("aria-pressed", String(muted));
        button.querySelector("[data-mute-label]").textContent = muted ? "Unmute notifications" : "Mute notifications";
    });

    // ---- Search in conversation ----

    const chatSearch = chat?.querySelector("[data-chat-search]");
    if (chatSearch) {
        const input = chatSearch.querySelector("[data-chat-search-input]");
        const count = chatSearch.querySelector("[data-chat-search-count]");
        const headerButton = chat.querySelector(".msg-chat-head [data-conversation-search]");
        let matches = [];
        let current = -1;

        const clear = () => log.querySelectorAll(".msg-bubble.is-match").forEach((bubble) => bubble.classList.remove("is-match", "is-current"));

        function show(index) {
            matches[current]?.classList.remove("is-current");
            current = (index + matches.length) % matches.length;
            matches[current].classList.add("is-current");
            matches[current].scrollIntoView({ block: "center", behavior: "smooth" });
            count.textContent = `${current + 1} of ${matches.length}`;
        }

        // Starts at the newest match.
        function run() {
            clear();
            const query = input.value.trim().toLowerCase();
            matches = query ? [...log.querySelectorAll(".msg-bubble")].filter((bubble) => bubble.textContent.toLowerCase().includes(query)) : [];
            current = -1;
            matches.forEach((bubble) => bubble.classList.add("is-match"));
            if (matches.length) {
                show(matches.length - 1);
            } else {
                count.textContent = query ? "No matches" : "";
            }
        }

        function open() {
            // On phones the details panel covers the chat, so close it first.
            if (details && !details.hidden && matchMedia("(max-width: 860px)").matches) setDetails(false, detailsClose.href);
            chatSearch.hidden = false;
            headerButton.setAttribute("aria-expanded", "true");
            input.focus();
            input.select();
        }

        function close() {
            clear();
            input.value = "";
            count.textContent = "";
            chatSearch.hidden = true;
            headerButton.setAttribute("aria-expanded", "false");
            headerButton.focus();
        }

        page.querySelectorAll("[data-conversation-search]").forEach((button) => button.addEventListener("click", open));
        chatSearch.querySelector("[data-chat-search-close]").addEventListener("click", close);
        chatSearch.querySelectorAll("[data-chat-search-step]").forEach((button) => {
            button.addEventListener("click", () => matches.length && show(current + Number(button.dataset.chatSearchStep)));
        });
        input.addEventListener("input", run);
        // Enter goes to the next older match, Shift+Enter back toward the newest.
        input.addEventListener("keydown", (event) => {
            if (event.key === "Enter" && matches.length) {
                event.preventDefault();
                show(current + (event.shiftKey ? 1 : -1));
            } else if (event.key === "Escape") {
                close();
            }
        });
    }

    // ---- Phones: bring the open chat into view so the message box is on screen ----

    if (chat && page.classList.contains("is-chat-open") && !page.classList.contains("is-details-open") && matchMedia("(max-width: 860px)").matches) {
        chat.scrollIntoView({ block: "end" });
    }
})();
