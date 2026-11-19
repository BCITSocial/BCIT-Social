// Limits each tag picker (_TagPicker.cshtml) to data-max ticked chips, and blocks submitting
// with fewer than data-min. Once the limit is reached the other chips are disabled until one is unticked.
(() => {
    const update = (picker) => {
        const boxes = [...picker.querySelectorAll("input[type=checkbox]")];
        const checked = boxes.filter((box) => box.checked).length;
        const max = Number(picker.dataset.max) || boxes.length;
        const min = Number(picker.dataset.min) || 0;
        boxes.forEach((box) => { box.disabled = !box.checked && checked >= max; });
        if (boxes[0]) {
            boxes[0].setCustomValidity(checked < min ? (min === 1 ? "Choose at least one tag." : `Choose at least ${min} tags.`) : "");
        }
    };

    document.querySelectorAll("[data-tag-picker]").forEach((picker) => {
        picker.addEventListener("change", () => update(picker));
        update(picker);
        // Reset buttons and dialogs reopening keep the limit in step.
        picker.closest("form")?.addEventListener("reset", () => setTimeout(() => update(picker)));
    });
})();
