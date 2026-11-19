(function () {
    'use strict';

    var API_URL = '/api/chat';
    var STORAGE_KEY = 'basau_chatbot_history_v1';
    var MAX_MESSAGE_LENGTH = 500;
    var MAX_HISTORY = 6;
    var REQUEST_TIMEOUT_MS = 30000;

    var WELCOME_MESSAGE =
        'Xin chào! Mình là trợ lý tư vấn của Basau Bags. Bạn đang tìm mẫu túi nào (đeo chéo, balo, đi làm hay tầm giá bao nhiêu)? Chọn nhanh gợi ý bên dưới hoặc nhắn cho mình nhé!';
    var FALLBACK_NOTICE = 'Trợ lý AI đang bận, đây là gợi ý nhanh từ cửa hàng';
    var RATE_LIMIT_MESSAGE = 'Bạn gửi hơi nhanh, vui lòng thử lại sau ít giây.';
    var QUICK_SUGGESTIONS = [
        'Túi dưới 500k',
        'Túi đang giảm giá',
        'Balo đi học'
    ];

    // Kiểm tra URL tương đối an toàn (bắt đầu bằng '/' và không phải '//' hoặc '/\')
    function isSafeRelativeUrl(url) {
        if (typeof url !== 'string') return false;
        var trimmed = url.trim();
        if (!trimmed || !trimmed.startsWith('/') || trimmed.startsWith('//') || trimmed.startsWith('/\\')) {
            return false;
        }
        if (/[\u0000-\u001f\u007f]/.test(trimmed)) {
            return false;
        }
        return true;
    }

    // Định dạng giá tiền kiểu Việt Nam
    function formatCurrency(value) {
        var num = Number(value);
        if (!Number.isFinite(num) || num < 0) return '0 đ';
        return Math.round(num).toLocaleString('vi-VN') + ' đ';
    }

    // Chuẩn hóa một đối tượng sản phẩm nhận từ API hoặc sessionStorage
    function sanitizeProduct(raw) {
        if (!raw || typeof raw !== 'object') return null;

        var tenTui = typeof raw.tenTui === 'string' && raw.tenTui.trim()
            ? raw.tenTui.trim()
            : 'Sản phẩm túi xách';
        var donGia = Number(raw.donGia);
        var giaGoc = Number(raw.giaGoc);
        var phanTramGiam = Number(raw.phanTramGiam);

        return {
            maTui: Number.isFinite(Number(raw.maTui)) ? Number(raw.maTui) : 0,
            tenTui: tenTui,
            donGia: Number.isFinite(donGia) && donGia >= 0 ? donGia : 0,
            giaGoc: Number.isFinite(giaGoc) && giaGoc >= 0 ? giaGoc : 0,
            dangGiam: Boolean(raw.dangGiam),
            phanTramGiam: Number.isFinite(phanTramGiam) && phanTramGiam > 0 ? Math.round(phanTramGiam) : 0,
            anhUrl: isSafeRelativeUrl(raw.anhUrl) ? raw.anhUrl.trim() : '',
            linkChiTiet: isSafeRelativeUrl(raw.linkChiTiet) ? raw.linkChiTiet.trim() : '',
            conHang: raw.conHang !== false
        };
    }

    function sanitizeProducts(list) {
        if (!Array.isArray(list)) return [];
        var result = [];
        for (var i = 0; i < list.length; i++) {
            var item = sanitizeProduct(list[i]);
            if (item) result.push(item);
        }
        return result;
    }

    // Đọc lịch sử hội thoại gần nhất từ sessionStorage (tối đa 6 tin)
    function loadHistory() {
        try {
            var raw = sessionStorage.getItem(STORAGE_KEY);
            if (!raw) return [];
            var parsed = JSON.parse(raw);
            if (!Array.isArray(parsed)) return [];

            var valid = [];
            for (var i = 0; i < parsed.length; i++) {
                var item = parsed[i];
                if (!item || typeof item !== 'object') continue;
                if (item.role !== 'user' && item.role !== 'assistant') continue;
                if (typeof item.content !== 'string' || !item.content.trim()) continue;

                var entry = {
                    role: item.role,
                    content: item.content
                };

                if (item.role === 'assistant') {
                    entry.source = item.source === 'fallback' ? 'fallback' : 'llm';
                    entry.products = sanitizeProducts(item.products);
                }

                valid.push(entry);
            }

            return valid.slice(-MAX_HISTORY);
        } catch (_) {
            return [];
        }
    }

    // Lưu tối đa 6 tin gần nhất vào sessionStorage
    function saveHistory(items) {
        try {
            var trimmed = Array.isArray(items) ? items.slice(-MAX_HISTORY) : [];
            sessionStorage.setItem(STORAGE_KEY, JSON.stringify(trimmed));
        } catch (_) {
            // Bỏ qua nếu trình duyệt chặn sessionStorage hoặc bộ nhớ đầy
        }
    }

    function clearHistoryStorage() {
        try {
            sessionStorage.removeItem(STORAGE_KEY);
        } catch (_) {
            // Bỏ qua lỗi quyền truy cập storage
        }
    }

    // Chuẩn bị mảng history gửi lên POST /api/chat theo đúng hợp đồng API
    function buildHistoryPayload(items) {
        var recent = Array.isArray(items) ? items.slice(-MAX_HISTORY) : [];
        var payload = [];
        for (var i = 0; i < recent.length; i++) {
            var item = recent[i];
            if (!item || (item.role !== 'user' && item.role !== 'assistant')) continue;
            var content = typeof item.content === 'string'
                ? item.content.trim().slice(0, MAX_MESSAGE_LENGTH)
                : '';
            if (!content) continue;
            payload.push({
                role: item.role,
                content: content
            });
        }
        return payload;
    }

    // Xóa toàn bộ phần tử con của một node bằng DOM API
    function clearChildren(node) {
        while (node.firstChild) {
            node.removeChild(node.firstChild);
        }
    }

    // Tạo phần tử icon placeholder khi không có ảnh hợp lệ hoặc ảnh lỗi
    function createThumbPlaceholder() {
        var icon = document.createElement('i');
        icon.className = 'bi bi-bag cb-product-placeholder';
        icon.setAttribute('aria-hidden', 'true');
        return icon;
    }

    // Tạo thẻ sản phẩm bằng createElement/textContent (an toàn XSS)
    function createProductCard(product) {
        var card = document.createElement('div');
        card.className = 'cb-product-card';

        // Khung ảnh sản phẩm
        var thumb = document.createElement('div');
        thumb.className = 'cb-product-thumb';

        if (isSafeRelativeUrl(product.anhUrl)) {
            var img = document.createElement('img');
            img.className = 'cb-product-img';
            img.src = product.anhUrl;
            img.alt = product.tenTui;
            img.loading = 'lazy';
            img.addEventListener('error', function () {
                clearChildren(thumb);
                thumb.appendChild(createThumbPlaceholder());
            });
            thumb.appendChild(img);
        } else {
            thumb.appendChild(createThumbPlaceholder());
        }

        // Khung thông tin sản phẩm
        var info = document.createElement('div');
        info.className = 'cb-product-info';

        var nameEl = document.createElement('div');
        nameEl.className = 'cb-product-name';
        nameEl.textContent = product.tenTui;
        info.appendChild(nameEl);

        // Giá hiện tại và giá gốc khi giảm giá
        var pricesEl = document.createElement('div');
        pricesEl.className = 'cb-product-prices';

        var currentPriceEl = document.createElement('span');
        currentPriceEl.className = product.dangGiam
            ? 'cb-price-current is-sale'
            : 'cb-price-current';
        currentPriceEl.textContent = formatCurrency(product.donGia);
        pricesEl.appendChild(currentPriceEl);

        if (product.dangGiam && product.giaGoc > product.donGia) {
            var originalPriceEl = document.createElement('span');
            originalPriceEl.className = 'cb-price-original';
            originalPriceEl.textContent = formatCurrency(product.giaGoc);
            pricesEl.appendChild(originalPriceEl);
        }
        info.appendChild(pricesEl);

        // Nhãn giảm giá / hết hàng và nút Xem chi tiết
        var footerEl = document.createElement('div');
        footerEl.className = 'cb-product-footer';

        var badgesEl = document.createElement('div');
        badgesEl.className = 'cb-product-badges';

        if (product.dangGiam && product.phanTramGiam > 0) {
            var saleBadge = document.createElement('span');
            saleBadge.className = 'cb-badge cb-badge-sale';
            saleBadge.textContent = '-' + product.phanTramGiam + '%';
            badgesEl.appendChild(saleBadge);
        }

        if (product.conHang === false) {
            var outBadge = document.createElement('span');
            outBadge.className = 'cb-badge cb-badge-out';
            outBadge.textContent = 'Hết hàng';
            badgesEl.appendChild(outBadge);
        }

        footerEl.appendChild(badgesEl);

        if (isSafeRelativeUrl(product.linkChiTiet)) {
            var linkEl = document.createElement('a');
            linkEl.className = 'cb-product-link';
            linkEl.href = product.linkChiTiet;
            linkEl.textContent = 'Xem chi tiết';
            footerEl.appendChild(linkEl);
        }

        if (badgesEl.childNodes.length > 0 || footerEl.childNodes.length > 1) {
            info.appendChild(footerEl);
        }

        card.appendChild(thumb);
        card.appendChild(info);
        return card;
    }

    function initChatbot() {
        if (!document.body || document.getElementById('cbChatbotRoot')) return;

        var history = loadHistory();
        var isOpen = false;
        var isLoading = false;
        var typingNode = null;
        var chipButtons = [];

        // Khởi tạo toàn bộ cây DOM bằng createElement
        var root = document.createElement('div');
        root.id = 'cbChatbotRoot';
        root.className = 'cb-root';

        // Cửa sổ chat
        var chatWindow = document.createElement('section');
        chatWindow.id = 'cbChatWindow';
        chatWindow.className = 'cb-window';
        chatWindow.setAttribute('role', 'dialog');
        chatWindow.setAttribute('aria-label', 'Trợ lý tư vấn túi');
        chatWindow.setAttribute('aria-hidden', 'true');

        // Header cửa sổ chat
        var header = document.createElement('div');
        header.className = 'cb-header';

        var headerInfo = document.createElement('div');
        headerInfo.className = 'cb-header-info';

        var avatar = document.createElement('div');
        avatar.className = 'cb-header-avatar';
        var avatarIcon = document.createElement('i');
        avatarIcon.className = 'bi bi-bag-heart-fill';
        avatarIcon.setAttribute('aria-hidden', 'true');
        avatar.appendChild(avatarIcon);

        var headerText = document.createElement('div');
        headerText.className = 'cb-header-text';

        var title = document.createElement('h2');
        title.className = 'cb-title';
        title.textContent = 'Trợ lý tư vấn túi';

        var subtitle = document.createElement('p');
        subtitle.className = 'cb-subtitle';
        subtitle.textContent = 'Basau Bags • Gợi ý mẫu phù hợp';

        headerText.appendChild(title);
        headerText.appendChild(subtitle);
        headerInfo.appendChild(avatar);
        headerInfo.appendChild(headerText);

        var headerActions = document.createElement('div');
        headerActions.className = 'cb-header-actions';

        var clearBtn = document.createElement('button');
        clearBtn.type = 'button';
        clearBtn.className = 'cb-clear-btn';
        clearBtn.setAttribute('aria-label', 'Xóa hội thoại');
        clearBtn.title = 'Xóa hội thoại';
        var clearIcon = document.createElement('i');
        clearIcon.className = 'bi bi-trash3';
        clearIcon.setAttribute('aria-hidden', 'true');
        var clearLabel = document.createElement('span');
        clearLabel.textContent = 'Xóa hội thoại';
        clearBtn.appendChild(clearIcon);
        clearBtn.appendChild(clearLabel);

        var closeBtn = document.createElement('button');
        closeBtn.type = 'button';
        closeBtn.className = 'cb-close-btn';
        closeBtn.setAttribute('aria-label', 'Đóng khung chat');
        closeBtn.title = 'Đóng';
        var closeIcon = document.createElement('i');
        closeIcon.className = 'bi bi-x-lg';
        closeIcon.setAttribute('aria-hidden', 'true');
        closeBtn.appendChild(closeIcon);

        headerActions.appendChild(clearBtn);
        headerActions.appendChild(closeBtn);
        header.appendChild(headerInfo);
        header.appendChild(headerActions);

        // Vùng tin nhắn cuộn
        var messagesEl = document.createElement('div');
        messagesEl.className = 'cb-messages';
        messagesEl.setAttribute('role', 'log');
        messagesEl.setAttribute('aria-live', 'polite');
        messagesEl.setAttribute('aria-relevant', 'additions');

        // Khu vực nhập tin nhắn
        var footer = document.createElement('div');
        footer.className = 'cb-footer';

        var form = document.createElement('form');
        form.className = 'cb-form';
        form.setAttribute('novalidate', 'novalidate');

        var input = document.createElement('textarea');
        input.className = 'cb-input';
        input.rows = 1;
        input.maxLength = MAX_MESSAGE_LENGTH;
        input.placeholder = 'Nhập nhu cầu tìm túi của bạn...';
        input.setAttribute('aria-label', 'Nhập tin nhắn tư vấn túi');

        var sendBtn = document.createElement('button');
        sendBtn.type = 'submit';
        sendBtn.className = 'cb-send-btn';
        sendBtn.setAttribute('aria-label', 'Gửi tin nhắn');
        var sendIcon = document.createElement('i');
        sendIcon.className = 'bi bi-send-fill';
        sendIcon.setAttribute('aria-hidden', 'true');
        var sendLabel = document.createElement('span');
        sendLabel.textContent = 'Gửi';
        sendBtn.appendChild(sendIcon);
        sendBtn.appendChild(sendLabel);

        form.appendChild(input);
        form.appendChild(sendBtn);

        var metaBar = document.createElement('div');
        metaBar.className = 'cb-meta-bar';

        var hintEl = document.createElement('span');
        hintEl.className = 'cb-hint';
        hintEl.textContent = 'Enter để gửi • Shift+Enter xuống dòng';

        var counterEl = document.createElement('span');
        counterEl.className = 'cb-counter';
        counterEl.textContent = '0/' + MAX_MESSAGE_LENGTH;

        metaBar.appendChild(hintEl);
        metaBar.appendChild(counterEl);

        footer.appendChild(form);
        footer.appendChild(metaBar);

        chatWindow.appendChild(header);
        chatWindow.appendChild(messagesEl);
        chatWindow.appendChild(footer);

        // Nút tròn nổi mở/đóng ở góc dưới bên phải
        var fabBtn = document.createElement('button');
        fabBtn.type = 'button';
        fabBtn.className = 'cb-fab';
        fabBtn.setAttribute('aria-label', 'Mở trợ lý tư vấn túi');
        fabBtn.setAttribute('aria-expanded', 'false');
        fabBtn.setAttribute('aria-controls', 'cbChatWindow');

        var fabIcon = document.createElement('i');
        fabIcon.className = 'bi bi-chat-dots-fill';
        fabIcon.setAttribute('aria-hidden', 'true');
        fabBtn.appendChild(fabIcon);

        root.appendChild(chatWindow);
        root.appendChild(fabBtn);
        document.body.appendChild(root);

        function scrollToBottom() {
            messagesEl.scrollTop = messagesEl.scrollHeight;
        }

        function updateCounterAndResize() {
            var len = input.value.length;
            counterEl.textContent = len + '/' + MAX_MESSAGE_LENGTH;
            if (len >= MAX_MESSAGE_LENGTH - 50) {
                counterEl.classList.add('is-near-limit');
            } else {
                counterEl.classList.remove('is-near-limit');
            }

            input.style.height = 'auto';
            var nextHeight = Math.min(input.scrollHeight, 96);
            input.style.height = (nextHeight > 38 ? nextHeight : 38) + 'px';
        }

        function setLoadingState(loading) {
            isLoading = loading;
            input.disabled = loading;
            sendBtn.disabled = loading;
            clearBtn.disabled = loading;
            for (var i = 0; i < chipButtons.length; i++) {
                chipButtons[i].disabled = loading;
            }
        }

        function showTypingIndicator() {
            removeTypingIndicator();

            var wrapper = document.createElement('div');
            wrapper.className = 'cb-msg cb-msg-assistant';

            var indicator = document.createElement('div');
            indicator.className = 'cb-typing';

            var dots = document.createElement('span');
            dots.className = 'cb-typing-dots';
            dots.setAttribute('aria-hidden', 'true');
            for (var i = 0; i < 3; i++) {
                dots.appendChild(document.createElement('span'));
            }

            var label = document.createElement('span');
            label.textContent = 'Đang trả lời…';

            indicator.appendChild(dots);
            indicator.appendChild(label);
            wrapper.appendChild(indicator);

            typingNode = wrapper;
            messagesEl.appendChild(wrapper);
            scrollToBottom();
        }

        function removeTypingIndicator() {
            if (typingNode && typingNode.parentNode) {
                typingNode.parentNode.removeChild(typingNode);
            }
            typingNode = null;
        }

        function appendUserMessage(text) {
            var wrapper = document.createElement('div');
            wrapper.className = 'cb-msg cb-msg-user';

            var bubble = document.createElement('div');
            bubble.className = 'cb-bubble';
            bubble.textContent = text;

            wrapper.appendChild(bubble);
            messagesEl.appendChild(wrapper);
            scrollToBottom();
        }

        function appendAssistantMessage(text, source, products) {
            var wrapper = document.createElement('div');
            wrapper.className = 'cb-msg cb-msg-assistant';

            var bubble = document.createElement('div');
            bubble.className = 'cb-bubble';
            bubble.textContent = text;
            wrapper.appendChild(bubble);

            // Hiện thông báo nhỏ khi backend dùng chế độ gợi ý nhanh dự phòng
            if (source === 'fallback') {
                var fallbackEl = document.createElement('div');
                fallbackEl.className = 'cb-fallback-note';
                var infoIcon = document.createElement('i');
                infoIcon.className = 'bi bi-info-circle';
                infoIcon.setAttribute('aria-hidden', 'true');
                var fallbackText = document.createElement('span');
                fallbackText.textContent = FALLBACK_NOTICE;
                fallbackEl.appendChild(infoIcon);
                fallbackEl.appendChild(fallbackText);
                wrapper.appendChild(fallbackEl);
            }

            // Hiển thị danh sách thẻ sản phẩm gợi ý nếu có
            var safeProducts = sanitizeProducts(products);
            if (safeProducts.length > 0) {
                var listEl = document.createElement('div');
                listEl.className = 'cb-products';
                for (var i = 0; i < safeProducts.length; i++) {
                    listEl.appendChild(createProductCard(safeProducts[i]));
                }
                wrapper.appendChild(listEl);
            }

            messagesEl.appendChild(wrapper);
            scrollToBottom();
        }

        function appendErrorMessage(text) {
            var wrapper = document.createElement('div');
            wrapper.className = 'cb-msg cb-msg-assistant cb-msg-error';

            var bubble = document.createElement('div');
            bubble.className = 'cb-bubble';
            bubble.textContent = text;

            wrapper.appendChild(bubble);
            messagesEl.appendChild(wrapper);
            scrollToBottom();
        }

        // Hiển thị lời chào ban đầu và 3 chip gợi ý bấm nhanh
        function appendWelcomeBlock() {
            chipButtons = [];

            var wrapper = document.createElement('div');
            wrapper.className = 'cb-msg cb-msg-assistant';

            var bubble = document.createElement('div');
            bubble.className = 'cb-bubble';
            bubble.textContent = WELCOME_MESSAGE;
            wrapper.appendChild(bubble);

            var chipsWrap = document.createElement('div');
            chipsWrap.className = 'cb-chips';

            for (var i = 0; i < QUICK_SUGGESTIONS.length; i++) {
                (function (suggestionText) {
                    var chip = document.createElement('button');
                    chip.type = 'button';
                    chip.className = 'cb-chip';
                    chip.textContent = suggestionText;
                    chip.disabled = isLoading;
                    chip.addEventListener('click', function () {
                        if (!isLoading) {
                            sendMessage(suggestionText);
                        }
                    });
                    chipButtons.push(chip);
                    chipsWrap.appendChild(chip);
                })(QUICK_SUGGESTIONS[i]);
            }

            wrapper.appendChild(chipsWrap);
            messagesEl.appendChild(wrapper);
        }

        // Vẽ lại toàn bộ vùng tin nhắn (lời chào + lịch sử đã lưu)
        function renderConversation() {
            clearChildren(messagesEl);
            appendWelcomeBlock();

            for (var i = 0; i < history.length; i++) {
                var entry = history[i];
                if (entry.role === 'user') {
                    appendUserMessage(entry.content);
                } else if (entry.role === 'assistant') {
                    appendAssistantMessage(entry.content, entry.source, entry.products);
                }
            }

            scrollToBottom();
        }

        function setOpen(nextOpen) {
            isOpen = Boolean(nextOpen);
            if (isOpen) {
                root.classList.add('is-open');
                chatWindow.setAttribute('aria-hidden', 'false');
                fabBtn.setAttribute('aria-expanded', 'true');
                fabBtn.setAttribute('aria-label', 'Đóng trợ lý tư vấn túi');
                fabIcon.className = 'bi bi-x-lg';
                scrollToBottom();
                if (!isLoading) {
                    input.focus();
                }
            } else {
                root.classList.remove('is-open');
                chatWindow.setAttribute('aria-hidden', 'true');
                fabBtn.setAttribute('aria-expanded', 'false');
                fabBtn.setAttribute('aria-label', 'Mở trợ lý tư vấn túi');
                fabIcon.className = 'bi bi-chat-dots-fill';
            }
        }

        // Gửi tin nhắn đến POST /api/chat kèm timeout 30 giây
        async function sendMessage(rawText) {
            if (isLoading) return;

            var text = typeof rawText === 'string'
                ? rawText.trim().slice(0, MAX_MESSAGE_LENGTH)
                : '';
            if (!text) return;

            // Lấy tối đa 6 tin gần nhất trước lượt hiện tại để gửi làm history
            var historyPayload = buildHistoryPayload(history);

            appendUserMessage(text);
            input.value = '';
            updateCounterAndResize();

            setLoadingState(true);
            showTypingIndicator();

            var controller = typeof AbortController !== 'undefined' ? new AbortController() : null;
            var timeoutId = null;
            if (controller) {
                timeoutId = setTimeout(function () {
                    controller.abort();
                }, REQUEST_TIMEOUT_MS);
            }

            try {
                var fetchOptions = {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    credentials: 'same-origin',
                    body: JSON.stringify({
                        message: text,
                        history: historyPayload
                    })
                };

                if (controller) {
                    fetchOptions.signal = controller.signal;
                }

                var response = await fetch(API_URL, fetchOptions);

                // Đọc và parse JSON an toàn phòng trường hợp server trả HTML/rỗng
                var data = null;
                try {
                    var bodyText = await response.text();
                    if (bodyText) {
                        data = JSON.parse(bodyText);
                    }
                } catch (_) {
                    data = null;
                }

                removeTypingIndicator();

                if (response.status === 429) {
                    appendErrorMessage(RATE_LIMIT_MESSAGE);
                    return;
                }

                if (!response.ok) {
                    var serverErr = data && typeof data.error === 'string' && data.error.trim()
                        ? data.error.trim()
                        : '';
                    if (serverErr) {
                        appendErrorMessage(serverErr);
                    } else if (response.status >= 500) {
                        appendErrorMessage('Hệ thống tư vấn đang tạm bận. Bạn vui lòng thử lại sau ít phút nhé.');
                    } else {
                        appendErrorMessage('Yêu cầu chưa thể xử lý lúc này. Bạn vui lòng kiểm tra lại và thử lại nhé.');
                    }
                    return;
                }

                var products = data && Array.isArray(data.products) ? sanitizeProducts(data.products) : [];
                var source = data && data.source === 'fallback' ? 'fallback' : 'llm';
                var replyText = data && typeof data.reply === 'string' ? data.reply.trim() : '';

                if (!replyText) {
                    replyText = products.length > 0
                        ? 'Mình gợi ý một số mẫu túi phù hợp cho bạn:'
                        : 'Xin lỗi, mình chưa nhận được nội dung trả lời phù hợp. Bạn vui lòng thử lại nhé.';
                }

                appendAssistantMessage(replyText, source, products);

                // Cập nhật và lưu tối đa 6 tin gần nhất vào sessionStorage
                history.push({ role: 'user', content: text });
                history.push({
                    role: 'assistant',
                    content: replyText,
                    source: source,
                    products: products
                });
                history = history.slice(-MAX_HISTORY);
                saveHistory(history);
            } catch (err) {
                removeTypingIndicator();
                if (err && err.name === 'AbortError') {
                    appendErrorMessage('Yêu cầu mất quá nhiều thời gian (quá 30 giây). Bạn vui lòng thử lại nhé.');
                } else {
                    appendErrorMessage('Không thể kết nối tới máy chủ. Vui lòng kiểm tra kết nối mạng và thử lại.');
                }
            } finally {
                if (timeoutId !== null) {
                    clearTimeout(timeoutId);
                }
                setLoadingState(false);
                if (isOpen) {
                    input.focus();
                }
            }
        }

        // Gắn sự kiện tương tác
        fabBtn.addEventListener('click', function () {
            setOpen(!isOpen);
        });

        closeBtn.addEventListener('click', function () {
            setOpen(false);
            fabBtn.focus();
        });

        clearBtn.addEventListener('click', function () {
            if (isLoading) return;
            history = [];
            clearHistoryStorage();
            renderConversation();
            if (isOpen) {
                input.focus();
            }
        });

        input.addEventListener('input', updateCounterAndResize);

        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey && !e.isComposing) {
                e.preventDefault();
                if (!isLoading) {
                    sendMessage(input.value);
                }
            }
        });

        form.addEventListener('submit', function (e) {
            e.preventDefault();
            if (!isLoading) {
                sendMessage(input.value);
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && isOpen) {
                setOpen(false);
                fabBtn.focus();
            }
        });

        renderConversation();
        updateCounterAndResize();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initChatbot);
    } else {
        initChatbot();
    }
})();
