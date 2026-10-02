/*
 * Tìm kiếm bằng giọng nói (HSMT C.9).
 * Gắn nút micro cạnh mọi ô <input data-voice-search>; dùng Web Speech API của trình duyệt
 * (Chrome, Edge, Safari). Trình duyệt không hỗ trợ thì không hiện nút.
 * Nói xong → điền từ khóa vào ô và gửi form tìm kiếm.
 */
(function () {
    'use strict';
    var Recognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!Recognition) return;

    var isEn = (document.documentElement.lang || '').toLowerCase().indexOf('en') === 0
        || location.pathname === '/en' || location.pathname.indexOf('/en/') === 0;
    var text = isEn
        ? { start: 'Search by voice', listening: 'Listening… speak now', denied: 'Microphone access was blocked.' }
        : { start: 'Tìm kiếm bằng giọng nói', listening: 'Đang nghe… mời nói', denied: 'Trình duyệt chưa cho phép dùng micro.' };

    var style = document.createElement('style');
    // Nút micro nằm TRONG ô nhập, sát lề phải: bọc input bằng span position:relative,
    // span nhận lại kích thước flex/grid của input để không vỡ bố cục form.
    style.textContent =
        '.voice-search-wrap{position:relative;display:block;min-width:0}' +
        '.voice-search-wrap>input{width:100%;padding-right:40px !important;box-sizing:border-box}' +
        // !important: CSS của từng form (vd .se-searchbar button) không được đè lên nút micro
        '.voice-search-wrap>.voice-search-btn{position:absolute !important;top:50% !important;right:6px !important;' +
        'transform:translateY(-50%) !important;display:inline-flex !important;align-items:center;justify-content:center;' +
        'width:30px !important;height:30px !important;min-width:0 !important;margin:0 !important;border:0 !important;' +
        'border-radius:50% !important;background:transparent !important;box-shadow:none !important;color:#6c757d !important;' +
        'padding:0 !important;cursor:pointer;font-size:1.05em !important;line-height:1;z-index:2}' +
        '.voice-search-wrap>.voice-search-btn:hover{background:rgba(0,0,0,.06) !important;color:#0d6efd !important}' +
        '.voice-search-wrap>.voice-search-btn.is-listening{color:#dc3545 !important;animation:voicePulse 1s infinite}' +
        '@keyframes voicePulse{0%,100%{opacity:1}50%{opacity:.35}}';
    document.head.appendChild(style);

    function attach(input) {
        if (input.dataset.voiceAttached) return;
        input.dataset.voiceAttached = '1';

        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'voice-search-btn';
        btn.title = text.start;
        btn.setAttribute('aria-label', text.start);
        btn.innerHTML = '<i class="bi bi-mic"></i>';

        var cs = getComputedStyle(input);
        var wrap = document.createElement('span');
        wrap.className = 'voice-search-wrap';
        wrap.style.flex = cs.flex;
        wrap.style.alignSelf = cs.alignSelf;
        wrap.style.gridColumn = cs.gridColumn;
        wrap.style.margin = cs.margin;
        if (input.style.width) wrap.style.width = input.style.width;
        input.style.margin = '0';
        input.parentNode.insertBefore(wrap, input);
        wrap.appendChild(input);
        wrap.appendChild(btn);

        var rec = null;
        var originalPh = input.placeholder;

        btn.addEventListener('click', function () {
            if (rec) { rec.stop(); return; }
            rec = new Recognition();
            rec.lang = isEn ? 'en-US' : 'vi-VN';
            rec.interimResults = true;
            rec.maxAlternatives = 1;

            rec.onstart = function () {
                btn.classList.add('is-listening');
                input.placeholder = text.listening;
            };
            rec.onresult = function (e) {
                var transcript = '';
                for (var i = 0; i < e.results.length; i++) transcript += e.results[i][0].transcript;
                input.value = transcript.trim();
                if (e.results[e.results.length - 1].isFinal && input.value && input.form) {
                    input.form.requestSubmit ? input.form.requestSubmit() : input.form.submit();
                }
            };
            rec.onerror = function (e) {
                if (e.error === 'not-allowed' || e.error === 'service-not-allowed') alert(text.denied);
            };
            rec.onend = function () {
                btn.classList.remove('is-listening');
                input.placeholder = originalPh;
                rec = null;
            };
            rec.start();
        });
    }

    function init() { document.querySelectorAll('input[data-voice-search]').forEach(attach); }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init); else init();
})();
