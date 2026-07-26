(function () {
    const root = document.querySelector('[data-ai-chatbox]');
    if (!root) return;

    const token = document.querySelector('meta[name="__RequestVerificationToken"]')?.content || '';
    const panel = root.querySelector('.ai-chatbox__panel');
    const messages = root.querySelector('[data-ai-chat-messages]');
    const form = root.querySelector('[data-ai-chat-form]');
    const input = form?.querySelector('input[name="message"]');
    const suggestions = root.querySelector('[data-ai-chat-suggestions]');
    const ctaElement = root.querySelector('[data-ai-chat-cta]');
    const feedbackForm = root.querySelector('[data-ai-chat-feedback]');
    let sessionKey = window.localStorage.getItem('aiChatSessionKey') || '';
    let lastMessage = '';
    let isSending = false;
    let supportAction = null;
    const typingSpeedMs = 14;

    document.querySelectorAll('[data-ai-chat-toggle]').forEach((button) => {
        button.addEventListener('click', () => {
            root.classList.toggle('is-open');
            const isOpen = root.classList.contains('is-open');
            panel?.setAttribute('aria-hidden', isOpen ? 'false' : 'true');
            document.body.classList.toggle('is-chat-open', isOpen);
            if (isOpen) input?.focus();
        });
    });

    function markHasMessages() {
        root.classList.add('has-messages');
    }

    function scrollMessagesToBottom() {
        if (!messages) return;
        requestAnimationFrame(() => {
            messages.scrollTop = messages.scrollHeight;
        });
    }

    function appendMessage(text, role) {
        const bubble = document.createElement('div');
        bubble.className = 'ai-chatbox__message ai-chatbox__message--' + role;
        bubble.textContent = text;
        messages.appendChild(bubble);
        scrollMessagesToBottom();
        return bubble;
    }

    const sourceTypeIcons = {
        'Công nghệ': 'bi-cpu',
        'Thiết bị': 'bi-box-seam',
        'Tài sản trí tuệ': 'bi-lightbulb',
        'Nhà cung ứng': 'bi-shop',
        'Chuyên gia': 'bi-person-badge',
        'OCOP': 'bi-award',
        'Tin bài': 'bi-newspaper'
    };

    // Renders the API's existing structured `sources` list (title/url/sourceType — see
    // AiKnowledgeItem) as compact clickable rows instead of relying on the raw-text
    // bullet list, which used to embed the URL path directly in the chat message.
    function renderResultList(sources) {
        if (!Array.isArray(sources) || sources.length === 0) return null;

        const list = document.createElement('div');
        list.className = 'ai-chatbox__result-list';

        sources.forEach((source) => {
            if (!source.url) return;

            const icon = sourceTypeIcons[source.sourceType] || 'bi-file-earmark-text';
            const linkLabel = (source.sourceType === 'Chuyên gia' || source.sourceType === 'Nhà cung ứng')
                ? 'Xem hồ sơ →'
                : 'Xem chi tiết →';

            const row = document.createElement('a');
            row.className = 'ai-chatbox__result-row';
            row.href = source.url;

            const iconEl = document.createElement('i');
            iconEl.className = 'bi ' + icon;
            iconEl.setAttribute('aria-hidden', 'true');

            const body = document.createElement('span');
            body.className = 'ai-chatbox__result-body';

            const title = document.createElement('span');
            title.className = 'ai-chatbox__result-title';
            title.textContent = source.title || '';

            const type = document.createElement('span');
            type.className = 'ai-chatbox__result-type';
            type.textContent = source.sourceType || '';

            const link = document.createElement('span');
            link.className = 'ai-chatbox__result-link';
            link.textContent = linkLabel;

            body.append(title, type, link);
            row.append(iconEl, body);
            list.appendChild(row);
        });

        return list.childElementCount > 0 ? list : null;
    }

    function appendTypingIndicator() {
        const bubble = document.createElement('div');
        bubble.className = 'ai-chatbox__message ai-chatbox__message--bot ai-chatbox__message--typing';
        bubble.innerHTML = '<span class="ai-chatbox__typing"><span></span><span></span><span></span></span>AI đang suy nghĩ...';
        messages.appendChild(bubble);
        scrollMessagesToBottom();
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
                scrollMessagesToBottom();

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
        markHasMessages();
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

            const resultList = renderResultList(data.sources);
            if (resultList) {
                messages.appendChild(resultList);
                scrollMessagesToBottom();
            }

            if (data.needsContactInfo) feedbackForm.hidden = false;
        } catch (error) {
            await typeMessage(pending, error.message || 'Hệ thống đang bận. Anh/chị vui lòng thử lại sau.');
        } finally {
            isSending = false;
            scrollMessagesToBottom();
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

    // Registered exactly once, independent of suggestion rendering, so re-fetching
    // suggestions can never attach a second click handler to this element.
    function initializeCta() {
        if (!ctaElement) return;

        ctaElement.addEventListener('click', () => {
            if (!supportAction) return;
            sendMessage(supportAction.title, supportAction.id);
        });
    }

    initializeCta();

    const quickActionIcons = {
        'browse-congnghe': 'bi-cpu',
        'browse-cntb': 'bi-box-seam',
        'browse-chuyengia': 'bi-person-badge',
        'browse-nhacungung': 'bi-shop'
    };

    fetch('/api/ai-chat/suggestions')
        .then((response) => response.json())
        .then((items) => {
            supportAction = null;
            if (ctaElement) {
                ctaElement.hidden = true;
                ctaElement.textContent = '';
            }

            const fragment = document.createDocumentFragment();

            items.forEach((item) => {
                if (item.id === 'action-hotro') {
                    supportAction = item;
                    if (ctaElement) {
                        ctaElement.textContent = item.title + ' →';
                        ctaElement.hidden = false;
                    }
                    return;
                }

                const icon = quickActionIcons[item.id] || 'bi-arrow-right-circle';
                const button = document.createElement('button');
                button.type = 'button';
                button.className = 'ai-chatbox__quick-action';
                button.dataset.actionId = item.id;
                button.innerHTML = '<i class="bi ' + icon + '" aria-hidden="true"></i><span>' + item.title + '</span>';
                button.addEventListener('click', () => sendMessage(item.title, item.id));
                fragment.appendChild(button);
            });

            suggestions.replaceChildren(fragment);
        })
        .catch(() => {});

    if (sessionKey) {
        fetch('/api/ai-chat/history?sessionKey=' + encodeURIComponent(sessionKey))
            .then((response) => response.json())
            .then((items) => {
                if (Array.isArray(items) && items.length > 0) {
                    markHasMessages();
                    items.forEach((item) => {
                        appendMessage(item.content, item.role === 'user' ? 'user' : 'bot');
                    });
                }
            })
            .catch(() => {});
    }
})();
