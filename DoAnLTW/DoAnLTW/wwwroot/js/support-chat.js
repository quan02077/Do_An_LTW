/*
 * Tab "Nhân viên" trong khung chat của khách (cạnh tab "Trợ lý AI"): chat thời gian thực với nhân viên cửa hàng qua SignalR.
 * Gắn vào khung do chatbot.js dựng (sự kiện cb:ready / window.BasauChatbot). Chỉ chạy khi đã đăng nhập và có thư viện signalR.
 * Mọi nội dung tin nhắn hiển thị bằng textContent (không innerHTML) nên không thể chèn mã HTML/JS.
 * Mã khách không bao giờ gửi từ trình duyệt: máy chủ tự xác định từ cookie đăng nhập.
 */
(function () {
    'use strict';

    var authMeta = document.querySelector('meta[name="chat-auth"]');
    if (!authMeta || authMeta.getAttribute('content') !== '1') return;
    if (typeof signalR === 'undefined') return;
    var staffMeta = document.querySelector('meta[name="chat-staff"]');
    if (staffMeta && staffMeta.getAttribute('content') === '1') return;   // nhân viên trả lời ở trang Admin, không dùng tab này

    var HUB_URL = '/hubs/support-chat';
    var MAX_LENGTH = 1000;
    var WELCOME = 'Chào bạn! Hãy nhắn câu hỏi của bạn tại đây, tin nhắn được lưu lại nên bạn có thể quay lại xem sau. Vui lòng không gửi mật khẩu hoặc số thẻ.';
    // dòng nhắc luôn hiện ở đầu tab: chưa có chỉ báo nhân viên trực tuyến nên nói rõ khách có thể phải chờ
    var ONLINE_NOTE = 'Nhân viên sẽ trả lời bạn khi trực tuyến.';
    var NOTE_ONLINE = 'Nhân viên đang trực tuyến.';
    var NOTE_OFFLINE = 'Hiện chưa có nhân viên trực tuyến. Bạn cứ để lại tin, nhân viên sẽ trả lời sớm nhất.';

    var ctx = null;            // giao diện khung chat từ chatbot.js
    var ui = null;             // các phần tử của tab nhân viên
    var tab = 'ai';            // 'ai' | 'staff'
    var unread = 0;
    var loaded = false;        // đã nạp lịch sử từ máy chủ ít nhất một lần
    var sending = false;
    var staffOnline = null;    // null = chưa biết; true/false do máy chủ báo (staffPresence)
    var seen = new Set();      // mã tin đã hiển thị (tránh trùng khi tin đến cả từ lịch sử lẫn sự kiện)

    var connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL)
        .withAutomaticReconnect([0, 2000, 5000, 10000, 20000])
        .build();

    // ---------- Tiện ích ----------
    function el(tag, className, text) {
        var node = document.createElement(tag);
        if (className) node.className = className;
        if (text != null) node.textContent = text;
        return node;
    }

    function pad(n) { return String(n).padStart(2, '0'); }

    function formatTime(iso) {
        var d = iso ? new Date(iso) : new Date();
        if (Number.isNaN(d.getTime())) return '';
        var now = new Date();
        var hhmm = pad(d.getHours()) + ':' + pad(d.getMinutes());
        return d.toDateString() === now.toDateString() ? hhmm : hhmm + ' ' + pad(d.getDate()) + '/' + pad(d.getMonth() + 1);
    }

    // lỗi từ hub có tiền tố kỹ thuật khi chạy môi trường phát triển: chỉ giữ lại câu thông báo cho người dùng
    function friendlyError(err) {
        var msg = err && err.message ? String(err.message) : '';
        var i = msg.indexOf('HubException: ');
        if (i >= 0) return msg.slice(i + 'HubException: '.length);
        if (/unauthorized|forbidden|401|403/i.test(msg)) return 'Bạn cần đăng nhập lại để chat với nhân viên.';
        return 'Không gửi được tin nhắn. Vui lòng thử lại.';
    }

    // ---------- Dựng giao diện ----------
    function build(context) {
        if (ui) return;
        ctx = context;
        var win = ctx.chatWindow;

        // thanh tab
        var tabs = el('div', 'cb-tabs');
        tabs.setAttribute('role', 'tablist');
        var tabAi = makeTab('Trợ lý AI', 'bi-robot');
        var tabStaff = makeTab('Nhân viên', 'bi-headset');
        var tabBadge = el('span', 'cb-tab-badge');
        tabBadge.hidden = true;
        tabStaff.appendChild(tabBadge);
        tabs.appendChild(tabAi);
        tabs.appendChild(tabStaff);
        win.insertBefore(tabs, ctx.messagesEl);

        // panel chat với nhân viên: dùng lại các lớp CSS của khung AI để đồng bộ giao diện
        var panel = el('div', 'sc-panel');
        panel.hidden = true;
        var note = el('div', 'sc-note');
        note.setAttribute('role', 'status');
        var noteIcon = el('span', 'sc-dot');
        noteIcon.setAttribute('aria-hidden', 'true');
        note.appendChild(noteIcon);
        var noteText = el('span', null, ONLINE_NOTE);
        note.appendChild(noteText);
        var log = el('div', 'cb-messages sc-messages');
        log.setAttribute('role', 'log');
        log.setAttribute('aria-live', 'polite');
        log.setAttribute('aria-relevant', 'additions');

        var footer = el('div', 'cb-footer');
        var form = el('form', 'cb-form');
        form.setAttribute('novalidate', 'novalidate');
        var input = el('textarea', 'cb-input');
        input.rows = 1;
        input.maxLength = MAX_LENGTH;
        input.placeholder = 'Nhập tin nhắn gửi nhân viên...';
        input.setAttribute('aria-label', 'Nhập tin nhắn gửi nhân viên');
        var sendBtn = el('button', 'cb-send-btn');
        sendBtn.type = 'submit';
        sendBtn.setAttribute('aria-label', 'Gửi tin nhắn cho nhân viên');
        var icon = el('i', 'bi bi-send-fill');
        icon.setAttribute('aria-hidden', 'true');
        sendBtn.appendChild(icon);
        sendBtn.appendChild(el('span', null, 'Gửi'));
        form.appendChild(input);
        form.appendChild(sendBtn);

        var meta = el('div', 'cb-meta-bar');
        meta.appendChild(el('span', 'cb-hint', 'Enter để gửi • Shift+Enter xuống dòng'));
        var counter = el('span', 'cb-counter', '0/' + MAX_LENGTH);
        meta.appendChild(counter);
        footer.appendChild(form);
        footer.appendChild(meta);

        panel.appendChild(note);
        panel.appendChild(log);
        panel.appendChild(footer);
        win.appendChild(panel);

        // huy hiệu số tin chưa đọc trên nút tròn nổi
        var fabBadge = el('span', 'cb-fab-badge');
        fabBadge.hidden = true;
        ctx.fabBtn.appendChild(fabBadge);

        ui = {
            note: note, noteText: noteText, tabs: tabs, tabAi: tabAi, tabStaff: tabStaff, tabBadge: tabBadge, panel: panel, log: log, input: input,
            sendBtn: sendBtn, counter: counter, fabBadge: fabBadge,
            title: ctx.header.querySelector('.cb-title'), subtitle: ctx.header.querySelector('.cb-subtitle'),
            clearBtn: ctx.header.querySelector('.cb-clear-btn')
        };

        tabAi.addEventListener('click', function () { switchTab('ai'); });
        tabStaff.addEventListener('click', function () { switchTab('staff'); });
        form.addEventListener('submit', function (e) { e.preventDefault(); send(); });
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey && !e.isComposing) {
                e.preventDefault();
                send();
            }
        });
        input.addEventListener('input', function () {
            counter.textContent = input.value.length + '/' + MAX_LENGTH;
            counter.classList.toggle('is-near-limit', input.value.length >= MAX_LENGTH - 50);
            input.style.height = 'auto';
            input.style.height = Math.min(input.scrollHeight, 96) + 'px';
        });

        setUnread(unread);
        renderPresence();
    }

    // chấm xanh/xám và câu chữ theo việc nhân viên có đang trực tuyến hay không
    function renderPresence() {
        if (!ui) return;
        ui.note.classList.toggle('is-online', staffOnline === true);
        ui.note.classList.toggle('is-offline', staffOnline === false);
        ui.noteText.textContent = staffOnline === true ? NOTE_ONLINE : (staffOnline === false ? NOTE_OFFLINE : ONLINE_NOTE);
        if (tab === 'staff' && ui.subtitle) {
            ui.subtitle.textContent = staffOnline === true ? 'Basau Bags • Nhân viên đang trực tuyến' : 'Basau Bags • Nhân viên trả lời bạn tại đây';
        }
    }

    function setPresence(online) {
        staffOnline = typeof online === 'boolean' ? online : null;
        renderPresence();
    }

    function makeTab(label, iconClass) {
        var btn = el('button', 'cb-tab');
        btn.type = 'button';
        btn.setAttribute('role', 'tab');
        var icon = el('i', 'bi ' + iconClass);
        icon.setAttribute('aria-hidden', 'true');
        btn.appendChild(icon);
        btn.appendChild(el('span', null, label));
        return btn;
    }

    // ---------- Chuyển tab ----------
    function switchTab(next) {
        if (!ui) return;
        tab = next;
        var staff = next === 'staff';
        ctx.messagesEl.hidden = staff;
        ctx.footer.hidden = staff;
        ui.panel.hidden = !staff;
        if (ui.clearBtn) ui.clearBtn.hidden = staff;
        ui.tabAi.classList.toggle('is-active', !staff);
        ui.tabStaff.classList.toggle('is-active', staff);
        ui.tabAi.setAttribute('aria-selected', String(!staff));
        ui.tabStaff.setAttribute('aria-selected', String(staff));
        if (ui.title) ui.title.textContent = staff ? 'Chat với nhân viên' : 'Trợ lý tư vấn túi';
        if (ui.subtitle) ui.subtitle.textContent = staff ? 'Basau Bags • Nhân viên trả lời bạn tại đây' : 'Basau Bags • Gợi ý mẫu phù hợp';
        renderPresence();

        if (staff) {
            if (!loaded) {
                loadHistory();
            } else {
                markReadIfVisible();
                scrollToBottom();
            }
            ui.input.focus();
        }
    }

    function isStaffVisible() {
        return Boolean(ctx && ctx.isOpen() && tab === 'staff');
    }

    // ---------- Huy hiệu chưa đọc ----------
    function setUnread(n) {
        unread = Math.max(0, n);
        if (!ui) return;
        var text = unread > 9 ? '9+' : String(unread);
        ui.tabBadge.textContent = text;
        ui.tabBadge.hidden = unread === 0;
        ui.fabBadge.textContent = text;
        ui.fabBadge.hidden = unread === 0;
    }

    function markReadIfVisible() {
        if (isStaffVisible() && unread > 0 && connection.state === signalR.HubConnectionState.Connected) {
            setUnread(0);
            connection.invoke('MarkReadByCustomer').catch(function () { /* lần sau mở lại sẽ đánh dấu */ });
        }
    }

    // ---------- Hiển thị tin nhắn ----------
    function scrollToBottom() {
        if (ui) ui.log.scrollTop = ui.log.scrollHeight;
    }

    function clearLog() {
        while (ui.log.firstChild) ui.log.removeChild(ui.log.firstChild);
        seen.clear();
    }

    function showWelcome() {
        var wrap = el('div', 'cb-msg cb-msg-assistant sc-welcome');
        wrap.appendChild(el('div', 'cb-bubble', WELCOME));
        ui.log.appendChild(wrap);
    }

    function removeWelcome() {
        var w = ui.log.querySelector('.sc-welcome');
        if (w) w.remove();
    }

    function appendMessage(m) {
        if (!ui || !m || seen.has(m.maTn)) return;
        seen.add(m.maTn);
        removeWelcome();

        var mine = !!m.tuKhach;
        var wrap = el('div', 'cb-msg ' + (mine ? 'cb-msg-user' : 'cb-msg-assistant'));
        if (!mine) wrap.appendChild(el('div', 'sc-sender', 'Nhân viên cửa hàng'));
        wrap.appendChild(el('div', 'cb-bubble', String(m.noiDung == null ? '' : m.noiDung)));
        wrap.appendChild(el('div', 'sc-time', formatTime(m.guiLuc)));
        ui.log.appendChild(wrap);
        scrollToBottom();
    }

    function showError(text) {
        var wrap = el('div', 'cb-msg cb-msg-assistant cb-msg-error sc-error');
        wrap.appendChild(el('div', 'cb-bubble', text));
        ui.log.appendChild(wrap);
        scrollToBottom();
        setTimeout(function () { wrap.remove(); }, 8000);
    }

    // ---------- Nạp lịch sử ----------
    async function loadHistory() {
        try {
            var res = await fetch('/api/support-chat/me', { credentials: 'same-origin', headers: { 'Accept': 'application/json' } });
            if (!res.ok) throw new Error('http ' + res.status);
            var data = await res.json();
            clearLog();
            var history = Array.isArray(data.history) ? data.history : [];
            if (history.length === 0) showWelcome();
            history.forEach(appendMessage);
            loaded = true;
            setUnread(Number(data.unread) || 0);
            if (typeof data.staffOnline === 'boolean') setPresence(data.staffOnline);
            markReadIfVisible();
            scrollToBottom();
        } catch (_) {
            if (ui && ui.log.childElementCount === 0) showError('Không tải được lịch sử trò chuyện. Vui lòng thử lại.');
        }
    }

    async function refreshUnread() {
        try {
            var res = await fetch('/api/support-chat/unread', { credentials: 'same-origin', headers: { 'Accept': 'application/json' } });
            if (res.ok) {
                var data = await res.json();
                if (!isStaffVisible()) setUnread(Number(data.unread) || 0);
            }
        } catch (_) { /* bỏ qua, sự kiện thời gian thực sẽ cập nhật sau */ }
    }

    // ---------- Gửi tin ----------
    async function send() {
        if (!ui || sending) return;
        var text = ui.input.value.trim();
        if (!text) return;
        if (connection.state !== signalR.HubConnectionState.Connected) {
            showError('Đang kết nối lại với máy chủ, vui lòng thử lại sau giây lát.');
            return;
        }

        sending = true;
        ui.sendBtn.disabled = true;
        try {
            await connection.invoke('SendToStaff', text);
            ui.input.value = '';
            ui.input.style.height = 'auto';
            ui.counter.textContent = '0/' + MAX_LENGTH;
            ui.counter.classList.remove('is-near-limit');
        } catch (err) {
            showError(friendlyError(err));
        } finally {
            sending = false;
            ui.sendBtn.disabled = false;
            ui.input.focus();
        }
    }

    // ---------- Sự kiện thời gian thực ----------
    connection.on('supportMessage', function (m) {
        if (!m) return;
        if (loaded) appendMessage(m);
        if (m.tuKhach) return;                       // tin do chính mình gửi (từ tab này hoặc tab khác)
        if (isStaffVisible()) {
            markReadIfVisible2(m);
        } else {
            setUnread(unread + 1);
        }
    });

    // đang xem đúng tab nhân viên khi tin đến: đánh dấu đã đọc ngay (không tăng huy hiệu)
    function markReadIfVisible2() {
        if (connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke('MarkReadByCustomer').catch(function () { });
        }
    }

    connection.on('supportRead', function () { setUnread(0); });

    // máy chủ báo nhân viên vào/ra trực tuyến (cũng được gửi ngay khi vừa kết nối hoặc kết nối lại)
    connection.on('staffPresence', function (online) { setPresence(online); });

    connection.onreconnected(function () {
        if (loaded) loadHistory(); else refreshUnread();
    });

    async function startConnection(attempt) {
        try {
            await connection.start();
            refreshUnread();
        } catch (_) {
            if (attempt < 5) setTimeout(function () { startConnection(attempt + 1); }, 3000 * (attempt + 1));
        }
    }

    // ---------- Khởi động ----------
    function init(context) {
        build(context);
        document.addEventListener('cb:toggle', function (e) {
            if (e.detail && e.detail.open) markReadIfVisible();
        });
        document.addEventListener('cb:open-staff', function () {
            ctx.setOpen(true);
            switchTab('staff');
        });
        switchTab('ai');
        startConnection(0);
    }

    // mở từ nút "Chat với nhân viên" của chatbot (chatbot.js kiểm tra window.BasauSupportChat)
    window.BasauSupportChat = { open: function () { document.dispatchEvent(new CustomEvent('cb:open-staff')); } };

    if (window.BasauChatbot) {
        init(window.BasauChatbot);
    } else {
        document.addEventListener('cb:ready', function (e) { init(e.detail); }, { once: true });
    }
})();
