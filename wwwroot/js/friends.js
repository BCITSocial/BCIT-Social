(() => {
    // A cancelled confirmation must not submit the removal form.
    document.querySelectorAll('[data-remove-friend]').forEach((form) => {
        form.addEventListener('submit', (event) => {
            if (!window.confirm(`Remove ${form.dataset.friendName} from your friends?`)) event.preventDefault();
        });
    });

    const dialog = document.querySelector('[data-cancel-dialog]');
    const recipient = dialog?.querySelector('[data-cancel-recipient]');
    let pendingForm = null;
    let confirmedForm = null;

    document.querySelectorAll('[data-cancel-request]').forEach((form) => {
        form.addEventListener('submit', (event) => {
            if (confirmedForm === form) {
                confirmedForm = null;
                return;
            }

            if (!dialog || !recipient || typeof dialog.showModal !== 'function') {
                if (!window.confirm(`Cancel your friend request to ${form.dataset.recipientName}?`)) event.preventDefault();
                return;
            }

            event.preventDefault();
            pendingForm = form;
            recipient.textContent = form.dataset.recipientName;
            dialog.showModal();
        });
    });

    dialog?.querySelector('[data-keep-request]')?.addEventListener('click', () => dialog.close());
    dialog?.querySelector('[data-confirm-cancel]')?.addEventListener('click', () => {
        if (!pendingForm) return;
        confirmedForm = pendingForm;
        dialog.close();
        confirmedForm.requestSubmit();
    });
    dialog?.addEventListener('close', () => { pendingForm = null; });
})();
