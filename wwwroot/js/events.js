// Submit the Events filter form as soon as a dropdown changes.
document.querySelectorAll("[data-events-filters] select").forEach((select) => {
    select.addEventListener("change", () => select.form.submit());
});
