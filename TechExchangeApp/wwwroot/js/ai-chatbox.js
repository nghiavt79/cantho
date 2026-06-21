(function () {
    const root = document.querySelector('[data-ai-chatbox]');
    if (!root) return;

    const token = document.querySelector('meta[name="__RequestVerificationToken"]')?.content || '';
    const messages = root.querySelector('[data-ai-chat-messages]');
    const form = root.querySelector('[data-ai-chat-form]');
    const input = form?.querySelector('input[name="message"]');
    const suggestions = root.querySelector('[data-ai-chat-suggestions]');
    const feedbackForm = root.querySelector('[data-ai-chat-feedback]');
    let sessionKey = window.localStorage.getItem('aiChatSessionKey') || '';
    let lastMessage = '';
    let isSending = false;
    const typingSpeedMs = 14;

    root.querySelectorAll('[data-ai-chat-toggle]').forEach((button) => {
        button.addEventListener('click', () => {
            root.classList.toggle('is-open');
            const panel = root.querySelector('.ai-chatbox__panel');
            panel?.setAttribute('aria-hidden', root.classList.contains('is-open') ? 'false' : 'true');
            if (root.classList.contains('is-open')) input?.focus();
        });
    });

    function appendMessage(text, role) {
        const bubble = document.createElement('div');
        bubble.className = 'ai-chatbox__message ai-chatbox__message--' + role;
        bubble.textContent = text;
        messages.appendChild(bubble);
        messages.scrollTop = messages.scrollHeight;
        return bubble;
    }

    function typeMessage(bubble, text) {
        const content = text || '';
        let index = 0;
        bubble.textContent = '';

        return new Promise((resolve) => {
            function step() {
                const nextChunkSize = content.charCodeAt(index) > 127 ? 1 : 2;
                bubble.textContent += content.slice(index, index + nextChunkSize);
                index += nextChunkSize;
                messages.scrollTop = messages.scrollHeight;

                if (index < content.length) {
                    window.setTimeout(step, typingSpeedMs);
                    return;
                }

                resolve();
            }

            step();
        });
    }

    async function postJson(url, payload) {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(payload)
        });

        const data = await response.json().catch(() => ({}));
        if (!response.ok) {
            throw new Error(data.message || data.title || 'Yeu cau chua thanh cong.');
        }

        return data;
    }

    async function sendMessage(text) {
        if (isSending || !text.trim()) return;
        isSending = true;
        lastMessage = text.trim();
        appendMessage(lastMessage, 'user');
        input.value = '';
        const pending = appendMessage('Dang tim thong tin...', 'bot');

        try {
            const data = await postJson('/api/ai-chat/message', {
                message: lastMessage,
                sessionKey
            });

            sessionKey = data.sessionKey || sessionKey;
            if (sessionKey) window.localStorage.setItem('aiChatSessionKey', sessionKey);
            await typeMessage(pending, data.message || 'Toi chua co cau tra loi phu hop.');
            if (data.needsContactInfo) feedbackForm.hidden = false;
        } catch (error) {
            await typeMessage(pending, error.message || 'He thong dang ban. Anh/chi vui long thu lai sau.');
        } finally {
            isSending = false;
            messages.scrollTop = messages.scrollHeight;
        }
    }

    form?.addEventListener('submit', (event) => {
        event.preventDefault();
        sendMessage(input.value);
    });

    feedbackForm?.addEventListener('submit', async (event) => {
        event.preventDefault();
        const submitButton = feedbackForm.querySelector('button');
        submitButton.disabled = true;

        try {
            const formData = new FormData(feedbackForm);
            const data = await postJson('/api/ai-chat/submit-feedback', {
                fullName: formData.get('fullName'),
                phone: formData.get('phone'),
                email: formData.get('email'),
                message: lastMessage,
                sessionKey
            });

            appendMessage(data.message || 'Thong tin da duoc ghi nhan.', 'bot');
            feedbackForm.hidden = true;
            feedbackForm.reset();
        } catch (error) {
            appendMessage(error.message || 'Chua the gui thong tin lien he.', 'bot');
        } finally {
            submitButton.disabled = false;
        }
    });

    fetch('/api/ai-chat/suggestions')
        .then((response) => response.json())
        .then((items) => {
            suggestions.innerHTML = '';
            items.forEach((item) => {
                const button = document.createElement('button');
                button.type = 'button';
                button.textContent = item;
                button.addEventListener('click', () => sendMessage(item));
                suggestions.appendChild(button);
            });
        })
        .catch(() => {});
})();
