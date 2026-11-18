(() => {
    // A cancelled confirmation must not submit the removal form.
    document.querySelectorAll('[data-remove-friend]').forEach((form) => {
        form.addEventListener('submit', (event) => {
            if (!window.confirm(`Remove ${form.dataset.friendName} from your friends?`)) event.preventDefault();
        });
    });
})();
