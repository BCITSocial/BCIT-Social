// Opens the create post modal from links marked data-open-create.
// When the modal isn't on the page (signed out), the links go to /Post/Create, which asks the visitor to log in.
(() => {
    const dialog = document.getElementById("create-post-dialog");
    if (!dialog) return;

    document.addEventListener("click", (event) => {
        const link = event.target.closest("a[data-open-create]");
        if (!link || event.ctrlKey || event.metaKey || event.shiftKey || event.button !== 0) return;
        event.preventDefault();
        dialog.showModal();
    });

    dialog.querySelectorAll("[data-close-create]").forEach((button) => {
        button.addEventListener("click", () => dialog.close());
    });

    // A click on the backdrop lands on the dialog element itself.
    dialog.addEventListener("click", (event) => {
        if (event.target === dialog) dialog.close();
    });
})();
