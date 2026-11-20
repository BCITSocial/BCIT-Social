// wwwroot/js/community-actions.js
// author: jaspreet bath
//
// Shared by the Communities page and a community's page:
// - Pop-ups (<dialog>): data-open-modal="<dialog id>" opens one, data-close-modal closes it.
//   A community page opened with #settings opens its Settings pop-up.
// - "Joined" dropdown menus: [data-menu] holding a [data-menu-toggle] button.

document.addEventListener("DOMContentLoaded", () => {
    // --- Pop-ups ---
    function openDialog(dialog, opener) {
        if (!dialog || dialog.open) return;
        dialog.returnFocusTo = opener || document.activeElement;
        dialog.showModal();
        (dialog.querySelector("[data-autofocus]")
            || dialog.querySelector("input:not([type='hidden']):not([type='file']):not([type='radio']), textarea, select"))?.focus();
    }

    document.addEventListener("click", (event) => {
        const opener = event.target.closest("[data-open-modal]");
        if (opener) {
            closeMenus();
            openDialog(document.getElementById(opener.dataset.openModal), opener);
            return;
        }

        const closer = event.target.closest("[data-close-modal]");
        if (closer) {
            closer.closest("dialog")?.close();
        }
    });

    document.querySelectorAll("dialog.community-dialog").forEach((dialog) => {
        // A click on the dark backdrop lands on the dialog element itself.
        dialog.addEventListener("click", (event) => {
            if (event.target === dialog) dialog.close();
        });
        dialog.addEventListener("close", () => {
            const target = dialog.returnFocusTo;
            if (target && document.contains(target)) target.focus();
        });
    });

    // /Community/Details/5#settings (from a card's "Settings") opens the Settings pop-up.
    function openFromHash() {
        if (window.location.hash === "#settings") {
            openDialog(document.getElementById("community-settings-modal"));
        }
    }
    openFromHash();
    window.addEventListener("hashchange", openFromHash);

    // --- "Joined" dropdown menus ---
    const menus = [...document.querySelectorAll("[data-menu]")];

    function menuParts(menu) {
        const toggle = menu.querySelector("[data-menu-toggle]");
        const list = toggle && document.getElementById(toggle.getAttribute("aria-controls"));
        return { toggle, list };
    }

    function setMenu(menu, open) {
        const { toggle, list } = menuParts(menu);
        if (!toggle || !list) return;
        toggle.setAttribute("aria-expanded", String(open));
        list.hidden = !open;
    }

    function closeMenus(except) {
        menus.forEach((menu) => { if (menu !== except) setMenu(menu, false); });
    }

    menus.forEach((menu) => {
        const { toggle, list } = menuParts(menu);
        if (!toggle || !list) return;

        toggle.addEventListener("click", (event) => {
            // The card behind the button is a link, so keep this click to the button.
            event.preventDefault();
            event.stopPropagation();
            const open = toggle.getAttribute("aria-expanded") !== "true";
            closeMenus(menu);
            setMenu(menu, open);
            if (open) list.querySelector("a, button")?.focus();
        });

        menu.addEventListener("keydown", (event) => {
            if (event.key === "Escape" && toggle.getAttribute("aria-expanded") === "true") {
                event.stopPropagation();
                setMenu(menu, false);
                toggle.focus();
            }
        });

        // Tabbing out of the menu closes it. (No relatedTarget means a mouse click; the
        // outside-click handler below deals with those, so Safari's clicks still reach the items.)
        menu.addEventListener("focusout", (event) => {
            if (event.relatedTarget && !menu.contains(event.relatedTarget)) setMenu(menu, false);
        });
    });

    document.addEventListener("click", (event) => {
        if (!event.target.closest("[data-menu]")) closeMenus();
    });

    // --- "Are you sure?" ---
    const confirmDialog = document.getElementById("community-confirm");
    const confirmForm = confirmDialog?.querySelector("[data-confirm-form]");

    document.addEventListener("click", (event) => {
        const trigger = event.target.closest("[data-confirm-action]");
        if (!trigger || !confirmDialog || !confirmForm) return;
        event.preventDefault();

        confirmForm.action = trigger.dataset.confirmAction;
        confirmDialog.querySelector("[data-confirm-title]").textContent = trigger.dataset.confirmTitle || "";
        confirmDialog.querySelector("[data-confirm-message]").textContent = trigger.dataset.confirmMessage || "";
        confirmDialog.querySelector("[data-confirm-submit]").textContent = trigger.dataset.confirmButton || "Confirm";
        confirmDialog.querySelector("[data-confirm-cancel]").textContent = trigger.dataset.confirmCancel || "Cancel";

        // Focus returns to the menu's button, since the menu item closes with the menu.
        const menuToggle = trigger.closest("[data-menu]")?.querySelector("[data-menu-toggle]");
        closeMenus();
        openDialog(confirmDialog, menuToggle || trigger);
        // Safer default: Cancel has focus, not the destructive button.
        confirmDialog.querySelector("[data-confirm-cancel]")?.focus();
    });
});
