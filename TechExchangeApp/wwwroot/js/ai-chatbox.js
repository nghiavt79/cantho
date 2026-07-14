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

    document.querySelectorAll('[data-ai-chat-toggle]').forEach((button) => {
        button.addEventListener('click', () => {
            root.classList.toggle('is-open');
            const isOpen = root.classList.contains('is-open');
            const panel = root.querySelector('.ai-chatbox__panel');
            panel?.setAttribute('aria-hidden', isOpen ? 'false' : 'true');
            document.body.classList.toggle('is-chat-open', isOpen);
            if (isOpen) input?.focus();
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

    function appendTypingIndicator() {
        const bubble = document.createElement('div');
        bubble.className = 'ai-chatbox__message ai-chatbox__message--bot ai-chatbox__message--typing';
        bubble.innerHTML = '<span class="ai-chatbox__typing"><span></span><span></span><span></span></span>AI đang suy nghĩ...';
        messages.appendChild(bubble);
        messages.scrollTop = messages.scrollHeight;
        return bubble;
    }

    function typeMessage(bubble, text) {
        const content = text || '';
        let index = 0;
        bubble.classList.remove('ai-chatbox__message--typing');
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
            throw new Error(data.message || data.title || 'Yêu cầu chưa thành công.');
        }

        return data;
    }

    async function sendMessage(text, actionId) {
        if (isSending || !text.trim()) return;
        isSending = true;
        lastMessage = text.trim();
        appendMessage(lastMessage, 'user');
        input.value = '';
        const pending = appendTypingIndicator();

        try {
            const data = await postJson('/api/ai-chat/message', {
                message: lastMessage,
                actionId: actionId || null,
                sessionKey
            });

            sessionKey = data.sessionKey || sessionKey;
            if (sessionKey) window.localStorage.setItem('aiChatSessionKey', sessionKey);
            await typeMessage(pending, data.message || 'Tôi chưa có câu trả lời phù hợp.');
            if (data.needsContactInfo) feedbackForm.hidden = false;
        } catch (error) {
            await typeMessage(pending, error.message || 'Hệ thống đang bận. Anh/chị vui lòng thử lại sau.');
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

            appendMessage(data.message || 'Thông tin đã được ghi nhận.', 'bot');
            feedbackForm.hidden = true;
            feedbackForm.reset();
        } catch (error) {
            appendMessage(error.message || 'Chưa thể gửi thông tin liên hệ.', 'bot');
        } finally {
            submitButton.disabled = false;
        }
    });

    const quickActionIcons = {
        'browse-congnghe': 'bi-cpu',
        'browse-cntb': 'bi-box-seam',
        'action-tuvan': 'bi-chat-square-text',
        'action-hotro': 'bi-send',
        'action-lienhe': 'bi-telephone-fill'
    };

    fetch('/api/ai-chat/suggestions')
        .then((response) => response.json())
        .then((items) => {
            suggestions.innerHTML = '';
            items.forEach((item) => {
                const icon = quickActionIcons[item.id] || 'bi-arrow-right-circle';
                const button = document.createElement('button');
                button.type = 'button';
                button.className = 'ai-chatbox__quick-action';
                button.innerHTML = '<i class="bi ' + icon + '" aria-hidden="true"></i><span>' + item.title + '</span>';
                button.addEventListener('click', () => sendMessage(item.title, item.id));
                suggestions.appendChild(button);
            });
        })
        .catch(() => {});

    if (sessionKey) {
        fetch('/api/ai-chat/history?sessionKey=' + encodeURIComponent(sessionKey))
            .then((response) => response.json())
            .then((items) => {
                (items || []).forEach((item) => {
                    appendMessage(item.content, item.role === 'user' ? 'user' : 'bot');
                });
            })
            .catch(() => {});
    }
})();
