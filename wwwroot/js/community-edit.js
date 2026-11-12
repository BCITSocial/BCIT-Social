// wwwroot/js/community-edit.js
// author: jaspreet bath
// Saves the ticked tags (_TagPicker) plus the campus into the hidden Type field, comma separated.

document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("edit-community-form");
    const tagPicker = document.getElementById("edit-community-tags");
    const campusSelect = document.getElementById("edit-campus-select");
    const hiddenTypeInput = document.getElementById("edit-combined-type-input");

    function syncEditTags() {
        const tagList = [...(tagPicker?.querySelectorAll("input:checked") ?? [])].map(box => box.value);

        // campus / Location (Bundled into Type)
        if (campusSelect && campusSelect.value) {
            const campusVal = campusSelect.value.trim();
            if (campusVal && campusVal !== "None" && !tagList.includes(campusVal)) {
                tagList.push(campusVal);
            }
        }

        if (hiddenTypeInput) {
            hiddenTypeInput.value = tagList.join(", ");
        }
    }

    tagPicker?.addEventListener("change", syncEditTags);
    campusSelect?.addEventListener("change", syncEditTags);

    if (form) {
        form.addEventListener("submit", syncEditTags);
    }

    syncEditTags();
});
