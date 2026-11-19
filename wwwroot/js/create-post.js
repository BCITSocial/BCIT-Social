// Opens the create post modal from links marked data-open-create.
// When the modal isn't on the page (signed out), the links go to /Post/Create, which asks the visitor to log in.
// The form is checked here first, then sent in the background. On success the current page reloads and shows a
// short "published" note, so the new post shows up as a normal card in the feed instead of opening on its own page.
// It also shows a short "deleted" note after a post's Delete button is used.
// All wording comes from data- attributes in Views/Shared/Components/CreatePostDialog/Default.cshtml.
(() => {
    const dialog = document.getElementById("create-post-dialog");
    if (!dialog) return;

    const form = dialog.querySelector("[data-create-post]");
    const message = form.querySelector("[data-form-message]");
    const submit = form.querySelector('button[type="submit"]');
    const submitText = submit.textContent;
    const typeField = form.querySelector("fieldset[data-error]");
    const typeError = typeField.querySelector(".field-error");
    const photoInput = form.querySelector('input[type="file"]');
    const photoList = form.querySelector("[data-photo-previews]");
    const photoError = document.getElementById("create-post-photos-error");
    const publishedKey = "bcit-social:post-published";
    const deletedKey = "bcit-social:post-deleted";

    const format = (text, value) => (text || "").replace("{0}", value);

    // ---- Opening and closing ----

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

    // The draft stays when the modal closes; only the messages are cleared.
    dialog.addEventListener("close", clearMessages);

    // ---- Title counter ----

    form.querySelectorAll("[data-count-for]").forEach((counter) => {
        const input = document.getElementById(counter.dataset.countFor);
        const update = () => { counter.textContent = `${input.value.length}/${input.maxLength}`; };
        input.addEventListener("input", update);
        update();
    });

    // ---- Field checks ----

    function showFieldError(field, errorElement, text) {
        field.setAttribute("aria-invalid", text ? "true" : "false");
        errorElement.textContent = text || "";
        errorElement.hidden = !text;
    }

    function checkText(input) {
        const error = document.getElementById(input.getAttribute("aria-describedby"));
        const ok = input.value.trim().length > 0;
        showFieldError(input, error, ok ? "" : input.dataset.error);
        return ok;
    }

    function checkType() {
        const ok = form.querySelector('input[name="TagId"]:checked') !== null;
        showFieldError(typeField, typeError, ok ? "" : typeField.dataset.error);
        return ok;
    }

    const textInputs = [...form.querySelectorAll("input[data-error], textarea[data-error]")];

    // Once a field has shown an error, check it again as it is fixed.
    textInputs.forEach((input) => {
        input.addEventListener("input", () => {
            if (input.getAttribute("aria-invalid") === "true") checkText(input);
        });
    });
    form.querySelectorAll('input[name="TagId"]').forEach((radio) => {
        radio.addEventListener("change", () => {
            if (typeField.getAttribute("aria-invalid") === "true") checkType();
        });
    });

    // ---- Photos: previews, Remove, and the upload rules ----

    // The photos that will be sent. The file input is rebuilt from this list after each change.
    let photos = [];
    const problems = new Map(); // chosen file -> error text
    let rejected = []; // error text for files left out of the last choice

    const allowedTypes = ["image/png", "image/jpeg", "image/webp", "image/gif"];
    const maxBytes = Number(photoInput.dataset.maxBytes) || Infinity;
    const minSize = Number(photoInput.dataset.minSize) || 0;

    function syncInput() {
        const transfer = new DataTransfer();
        photos.forEach((file) => transfer.items.add(file));
        photoInput.files = transfer.files;
    }

    function showPhotoErrors() {
        const text = [...rejected, ...problems.values()].join(" ");
        showFieldError(photoInput, photoError, text);
    }

    function addPreview(file) {
        const item = document.createElement("li");
        const image = document.createElement("img");
        const url = URL.createObjectURL(file);
        image.src = url;
        image.alt = file.name;

        // The size is only known once the image loads, so the dimension rule is checked here.
        image.addEventListener("load", () => {
            if (minSize && (image.naturalWidth < minSize || image.naturalHeight < minSize) && !problems.has(file)) {
                problems.set(file, format(photoInput.dataset.errorDimensions, file.name));
                item.classList.add("is-invalid");
                showPhotoErrors();
            }
        }, { once: true });

        const remove = document.createElement("button");
        remove.type = "button";
        remove.className = "photo-remove";
        remove.setAttribute("aria-label", format(photoInput.dataset.removeText, file.name));
        remove.innerHTML = '<svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M6 6l12 12M18 6 6 18" /></svg>';
        remove.addEventListener("click", () => {
            const index = [...photoList.children].indexOf(item);
            photos = photos.filter((photo) => photo !== file);
            problems.delete(file);
            URL.revokeObjectURL(url);
            item.remove();
            syncInput();
            showPhotoErrors();
            photoList.hidden = photos.length === 0;
            // Keep focus in the list: the next Remove button, the previous one, or the Add photos box.
            const next = photoList.children[index] || photoList.children[index - 1];
            (next?.querySelector("button") || photoInput).focus();
        });

        item.append(image, remove);
        photoList.append(item);
    }

    // Choosing more photos adds to the ones already chosen. Files that break a rule are not added.
    photoInput.addEventListener("change", () => {
        const chosen = [...photoInput.files].filter((file) => !photos.includes(file));
        rejected = [];
        chosen.forEach((file) => {
            if (!allowedTypes.includes(file.type)) {
                rejected.push(format(photoInput.dataset.errorType, file.name));
            } else if (file.size > maxBytes) {
                rejected.push(format(photoInput.dataset.errorSize, file.name));
            } else {
                photos.push(file);
                addPreview(file);
            }
        });
        syncInput();
        photoList.hidden = photos.length === 0;
        showPhotoErrors();
    });

    function clearPhotos() {
        photoList.querySelectorAll("img").forEach((image) => URL.revokeObjectURL(image.src));
        photoList.replaceChildren();
        photoList.hidden = true;
        photos = [];
        problems.clear();
        rejected = [];
        syncInput();
    }

    // ---- Messages ----

    function showMessage(text) {
        message.textContent = text;
        message.className = "form-message is-error";
        message.hidden = false;
    }

    function clearMessages() {
        message.hidden = true;
        message.textContent = "";
        textInputs.forEach((input) => showFieldError(input, document.getElementById(input.getAttribute("aria-describedby")), ""));
        showFieldError(typeField, typeError, "");
        // Keep a photo's problem visible while that photo is still chosen.
        rejected = [];
        showPhotoErrors();
    }

    function setBusy(busy) {
        submit.disabled = busy;
        submit.textContent = busy ? submit.dataset.submittingText : submitText;
        form.setAttribute("aria-busy", busy ? "true" : "false");
    }

    // ---- Sending ----

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        const results = [checkType(), ...textInputs.map(checkText)];
        const photosOk = problems.size === 0;
        if (!photosOk) showPhotoErrors();

        if (results.includes(false) || !photosOk) {
            showMessage(submit.dataset.errorSummary);
            const firstInvalid = form.querySelector('[aria-invalid="true"]');
            (firstInvalid === typeField ? typeField.querySelector("input") : firstInvalid)?.focus();
            return;
        }

        message.hidden = true;
        setBusy(true);
        try {
            const response = await fetch(form.action, { method: "POST", body: new FormData(form) });
            // PostController.Create redirects to /Home/Post/{id} when the post is saved.
            const saved = response.ok && response.redirected && /\/Home\/Post\/\d+/i.test(new URL(response.url).pathname);
            if (saved) {
                try {
                    sessionStorage.setItem(publishedKey, new URL(response.url).pathname);
                } catch {
                    // Storage can be blocked; the page still reloads, just without the note.
                }
                clearPhotos();
                form.reset();
                window.location.reload();
                return;
            }

            // Not saved: the server sent the form back. Show its error messages when it has any.
            let text = "";
            if (response.ok) {
                const page = new DOMParser().parseFromString(await response.text(), "text/html");
                text = [...page.querySelectorAll(".field-validation-error, .validation-summary-errors li")]
                    .map((element) => element.textContent.trim())
                    .filter(Boolean)
                    .join(" ");
            }
            showMessage(text || submit.dataset.errorFailed);
        } catch (error) {
            console.error(error);
            showMessage(submit.dataset.errorFailed);
        }
        setBusy(false);
    });

    // ---- Notes after the reload: "published" and "deleted" ----

    // Delete forms (marked data-delete-post) ask confirm() first; remember the delete only when it goes ahead.
    // TODO(BE): add a restore action (e.g. POST /Post/Restore/{id}) that clears DateOfDel; the "deleted" note can then get an Undo button.
    document.addEventListener("submit", (event) => {
        if (!event.target.matches("[data-delete-post]") || event.defaultPrevented) return;
        try {
            sessionStorage.setItem(deletedKey, "1");
        } catch {
            // Storage can be blocked; the post is still deleted, just without the note.
        }
    });

    function takeFlag(key) {
        try {
            const value = sessionStorage.getItem(key);
            sessionStorage.removeItem(key);
            return value;
        } catch {
            return null;
        }
    }

    const publishedUrl = takeFlag(publishedKey);
    if (publishedUrl) {
        // data-open-post opens the post in the Home modal where post-details.js is loaded; elsewhere it is a plain link.
        const link = document.createElement("a");
        link.href = publishedUrl;
        link.dataset.openPost = "";
        link.textContent = dialog.dataset.viewPostText;
        showNote(dialog.dataset.publishedText, link);
    } else if (takeFlag(deletedKey)) {
        showNote(dialog.dataset.deletedText);
    }

    function showNote(message, link) {
        const note = document.createElement("div");
        note.className = "post-toast";
        note.setAttribute("role", "status");

        const text = document.createElement("span");
        text.textContent = message;

        const close = document.createElement("button");
        close.type = "button";
        close.className = "icon-button";
        close.setAttribute("aria-label", dialog.dataset.dismissText);
        close.innerHTML = '<svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M6 6l12 12M18 6 6 18" /></svg>';

        const hide = () => note.remove();
        close.addEventListener("click", hide);
        link?.addEventListener("click", hide);
        const timer = setTimeout(hide, 8000);
        note.addEventListener("focusin", () => clearTimeout(timer));

        note.append(...[text, link, close].filter(Boolean));
        document.body.append(note);
    }
})();
