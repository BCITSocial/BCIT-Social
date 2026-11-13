//// Profile theme customizer
//// Author: Jonathan
//// Date: 2026-11-11
//// Description: This script handles the profile theme customizer functionality,
//// allowing users to select themes, accent colors, and avatar shapes.
//// It also manages the saving and loading of user preferences from local storage.

(() => {
    const themes = Object.freeze({
        campus: "#173a63",
        forest: "#16835d",
        lavender: "#805ad5",
        dark: "#60a5fa"
    });
    const defaults = Object.freeze({
        theme: "campus",
        accentColor: themes.campus,
        avatarShape: "circle"
    });
    const avatarShapes = ["circle", "rounded"];
    let currentPreferences = { ...defaults };

    function normalize(value) {
        if (!value || typeof value !== "object") {
            return { ...defaults };
        }

        const preferences = { ...defaults };
        const hasValidTheme = Object.hasOwn(themes, value.theme);
        if (hasValidTheme) {
            preferences.theme = value.theme;
        }
        if (avatarShapes.includes(value.avatarShape)) {
            preferences.avatarShape = value.avatarShape;
        }
        if (hasValidTheme
            && typeof value.accentColor === "string"
            && /^#[\da-f]{6}$/i.test(value.accentColor)) {
            preferences.accentColor = value.accentColor;
        }
        if (preferences.theme === "campus" && preferences.accentColor.toLowerCase() === "#f4510b") {
            preferences.accentColor = themes.campus;
        }
        return preferences;
    }

    function apply(preferences, profilePage) {
        const body = document.body;
        body.dataset.profileTheme = preferences.theme;
        body.dataset.profileAvatar = preferences.avatarShape;
        body.style.setProperty("--color-accent", preferences.accentColor);
        body.style.setProperty("--color-focus", preferences.accentColor);
        body.style.setProperty("--profile-accent-foreground", getAccentForeground(preferences.accentColor));

        const accentRgb = preferences.accentColor.slice(1).match(/.{2}/g)
            .map((channel) => parseInt(channel, 16))
            .join(", ");
        body.style.setProperty("--bs-link-color-rgb", accentRgb);
        body.style.setProperty("--bs-link-hover-color-rgb", accentRgb);

        profilePage?.querySelectorAll("[data-profile-theme-choice]").forEach((button) => {
            button.setAttribute("aria-pressed", String(button.dataset.profileThemeChoice === preferences.theme));
        });
    }

    function getAccentForeground(color) {
        const channels = color.slice(1).match(/.{2}/g).map((channel) => parseInt(channel, 16) / 255);
        const luminance = channels
            .map((channel) => channel <= 0.04045 ? channel / 12.92 : ((channel + 0.055) / 1.055) ** 2.4)
            .reduce((total, channel, index) => total + channel * [0.2126, 0.7152, 0.0722][index], 0);
        return luminance > 0.42 ? "#172033" : "#ffffff";
    }

    const appearance = {
        themes,
        defaults,
        normalize,
        apply,
        storageKey: (userId) => `bcit-social:profile-style:${userId}`,
        getPreferences: () => ({ ...currentPreferences }),
        savePreferences(storageKey, value) {
            const preferences = normalize(value);
            localStorage.setItem(storageKey, JSON.stringify(preferences));
            currentPreferences = preferences;
            return preferences;
        }
    };
    window.bcitProfileAppearance = appearance;

    const userId = document.body.dataset.profileUserId;
    if (!userId) {
        return;
    }

    const storageKey = appearance.storageKey(userId);
    try {
        const saved = localStorage.getItem(storageKey);
        if (saved) {
            currentPreferences = normalize(JSON.parse(saved));
        }
        apply(currentPreferences);
    } catch (error) {
        console.warn("Could not load saved profile appearance preferences.", error);
    }
})();
