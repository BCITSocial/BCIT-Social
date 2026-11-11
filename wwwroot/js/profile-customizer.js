//// Profile customizer
//// Author: Jonathan
//// Date: 2026-11-11
//// Description: This script handles the profile customizer functionality, 
//// allowing users to select and apply different themes, cover styles, avatar shapes, and 
//// card styles to their profile page. 
//// It also manages the saving and loading of user preferences from local storage.

(() => {
    const appearance = window.bcitProfileAppearance;
    const page = document.querySelector(".profile-page[data-profile-user-id]");
    const dialog = document.querySelector("[data-profile-customizer]");
    const form = document.querySelector("[data-profile-customizer-form]");

    if (!appearance || !page || !dialog || !(form instanceof HTMLFormElement)) {
        return;
    }

    const accentInput = form.elements.namedItem("accentColor");
    const coverSelect = form.elements.namedItem("coverStyle");
    const avatarSelect = form.elements.namedItem("avatarShape");
    const cardSelect = form.elements.namedItem("cardStyle");
    const status = dialog.querySelector("[data-profile-customizer-status]");
    const storageKey = appearance.storageKey(page.dataset.profileUserId);
    const { defaults, themes, normalize, apply } = appearance;
    let preferences = appearance.getPreferences();

    function setFormValues(value) {
        accentInput.value = value.accentColor;
        coverSelect.value = value.coverStyle;
        avatarSelect.value = value.avatarShape;
        cardSelect.value = value.cardStyle;
    }

    function getFormValues() {
        return normalize({
            ...preferences,
            accentColor: accentInput.value,
            coverStyle: coverSelect.value,
            avatarShape: avatarSelect.value,
            cardStyle: cardSelect.value
        });
    }

    function save(value, message) {
        apply(value, page);
        try {
            preferences = appearance.savePreferences(storageKey, value);
            return true;
        } catch {
            if (status) {
                status.textContent = message;
            }
            return false;
        }
    }

    apply(preferences, page);

    page.querySelectorAll("[data-profile-theme-choice]").forEach((button) => {
        button.addEventListener("click", () => {
            const theme = button.dataset.profileThemeChoice;
            if (!Object.hasOwn(themes, theme)) {
                return;
            }

            save({ ...preferences, theme, accentColor: themes[theme] },
                "Theme applied, but could not be saved in this browser.");
            // The theme buttons sit in the customizer, so keep its colour field in step with the new theme.
            accentInput.value = themes[theme];
        });
    });

    page.querySelector("[data-open-profile-customizer]")?.addEventListener("click", () => {
        setFormValues(preferences);
        if (status) {
            status.textContent = "";
        }
        dialog.showModal();
    });

    dialog.querySelectorAll("[data-close-profile-customizer]").forEach((button) => {
        button.addEventListener("click", () => dialog.close());
    });

    dialog.addEventListener("close", () => {
        apply(preferences, page);
        setFormValues(preferences);
        if (status) {
            status.textContent = "";
        }
    });

    dialog.addEventListener("click", (event) => {
        if (event.target === dialog) {
            dialog.close();
        }
    });

    form.addEventListener("input", () => apply(getFormValues(), page));
    form.addEventListener("change", () => apply(getFormValues(), page));

    dialog.querySelector("[data-reset-profile-customizer]")?.addEventListener("click", () => {
        const resetPreferences = {
            ...defaults,
            theme: preferences.theme,
            accentColor: themes[preferences.theme]
        };
        setFormValues(resetPreferences);
        apply(resetPreferences, page);
        if (status) {
            status.textContent = "Default choices are ready. Save to apply them.";
        }
    });

    form.addEventListener("submit", (event) => {
        event.preventDefault();
        if (save(getFormValues(), "Your changes are previewed but could not be saved in this browser.")) {
            dialog.close();
        }
    });
})();
