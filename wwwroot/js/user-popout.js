// Discord-style profile popout. Click an avatar or name marked data-user-popout to open a small card with
// "View profile" and a "Message @name" box. Enter sends: the form posts to /Messages/MessageUser, which opens
// (or creates) the direct chat and sends the message. An empty message just opens the chat.
(() => {
    const card = document.querySelector("[data-user-popout-card]");
    if (!card) return;

    const nameEl = card.querySelector("[data-popout-name]");
    const initialsEl = card.querySelector("[data-popout-initials]");
    const profileLink = card.querySelector("[data-popout-profile]");
    const form = card.querySelector("[data-popout-form]");
    const recipient = card.querySelector("[data-popout-recipient]");
    const text = card.querySelector("[data-popout-text]");
    const label = card.querySelector("[data-popout-label]");
    const self = card.querySelector("[data-popout-self]");
    const currentUserId = card.dataset.currentUserId;
    const GAP = 8;
    let trigger = null;

    const initials = (name) => name.split(/\s+/).filter(Boolean).slice(0, 2).map((part) => part[0].toUpperCase()).join("");

    // Beside the trigger (right, else left), kept inside the viewport.
    function place() {
        const box = trigger.getBoundingClientRect();
        const width = card.offsetWidth;
        const height = card.offsetHeight;
        let left = box.right + GAP;
        if (left + width > window.innerWidth - GAP) left = box.left - width - GAP;
        left = Math.min(Math.max(GAP, left), window.innerWidth - width - GAP);
        const top = Math.min(Math.max(GAP, box.top), window.innerHeight - height - GAP);
        card.style.left = `${left}px`;
        card.style.top = `${top}px`;
    }

    function open(target) {
        const id = target.dataset.userId;
        const name = target.dataset.userName || "Alumni member";
        if (trigger === target && !card.hidden) return close();
        trigger = target;

        nameEl.textContent = name;
        initialsEl.textContent = initials(name);
        profileLink.href = card.dataset.profileUrl.replace("__id__", encodeURIComponent(id));

        const isSelf = id === currentUserId;
        if (form) {
            form.hidden = isSelf;
            self.hidden = !isSelf;
            recipient.value = id;
            text.value = "";
            text.placeholder = `Message @${name}`;
            label.textContent = `Message ${name}`;
        }

        // A modal <dialog> (the post modal) sits in the top layer, so the card must live inside it to show on top.
        const host = target.closest("dialog[open]") ?? document.body;
        if (card.parentElement !== host) host.append(card);

        card.hidden = false;
        target.setAttribute("aria-expanded", "true");
        place();
        (form && !isSelf ? text : profileLink).focus();
    }

    function close({ restoreFocus = false } = {}) {
        if (card.hidden) return;
        card.hidden = true;
        trigger?.setAttribute("aria-expanded", "false");
        if (restoreFocus) trigger?.focus();
        trigger = null;
    }

    // Delegated, so it also works on cards loaded later (feed scrolling, the post modal).
    document.addEventListener("click", (event) => {
        const target = event.target.closest("[data-user-popout]");
        if (target) {
            event.preventDefault();
            event.stopPropagation();
            open(target);
            return;
        }
        if (!card.contains(event.target)) close();
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && !card.hidden) close({ restoreFocus: true });
    });
    window.addEventListener("resize", () => { if (!card.hidden) place(); });
    // Follow the trigger while the page scrolls (closing here would also fire when a phone keyboard opens).
    window.addEventListener("scroll", () => { if (!card.hidden && trigger?.isConnected) place(); }, { passive: true, capture: true });

    form?.addEventListener("submit", () => {
        form.querySelector("button[type='submit']").disabled = true;
    });
})();
