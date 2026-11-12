// wwwroot/js/community-edit.js
// author: jaspreet bath

document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("edit-community-form");
    const categorySelect = document.getElementById("edit-category-select");
    const campusSelect = document.getElementById("edit-campus-select");
    const tagSelect = document.getElementById("edit-tag-select");
    const statusSelect = document.getElementById("edit-status-select");
    const timeSelect = document.getElementById("edit-time-select");
    const hiddenTypeInput = document.getElementById("edit-combined-type-input");
    const previewContainer = document.getElementById("edit-tag-preview");

    function syncEditTags() {
        const tagList = [];
        const previewElements = [];

        // primary Category (Blue)
        if (categorySelect && categorySelect.value) {
            // trim value
            const catVal = categorySelect.value.trim();
            // if not "none", adds text into tagList 
            // and badge (color)
            if (catVal && catVal !== "None") {
                tagList.push(catVal);
                previewElements.push(`<span class="category-badge">${catVal}</span>`);
            }
        }

        // tag (Blue)
        if (tagSelect && tagSelect.value) {
            const tagVal = tagSelect.value.trim();
            if (tagVal && tagVal !== "None" && !tagList.includes(tagVal)) {
                tagList.push(tagVal);
                previewElements.push(`<span class="category-badge">${tagVal}</span>`);
            }
        }

        // status Tag (Green)
        if (statusSelect && statusSelect.value) {
            const statusVal = statusSelect.value.trim();
            if (statusVal && statusVal !== "None" && !tagList.includes(statusVal)) {
                tagList.push(statusVal);
                previewElements.push(`<span class="category-badge status-badge">${statusVal}</span>`);
            }
        }

        // time Tag (Orange)
        if (timeSelect && timeSelect.value) {
            const timeVal = timeSelect.value.trim();
            if (timeVal && timeVal !== "None" && !tagList.includes(timeVal)) {
                tagList.push(timeVal);
                previewElements.push(`<span class="category-badge time-badge">${timeVal}</span>`);
            }
        }

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

        if (previewContainer) {
            previewContainer.innerHTML = previewElements.join("");
        }
    }

    [categorySelect, campusSelect, tagSelect, statusSelect, timeSelect].forEach(select => {
        if (select) {
            select.addEventListener("change", syncEditTags);
        }
    });

    if (form) {
        form.addEventListener("submit", syncEditTags);
    }

    syncEditTags();
});