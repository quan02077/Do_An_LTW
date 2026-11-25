/*
 * Phía nhân viên của chat khách hàng - nhân viên (SignalR /hubs/support-chat).
 * - Mọi trang admin: cập nhật huy hiệu "Hỗ trợ khách" ở menu theo số tin chưa đọc, theo thời gian thực.
 * - Trang /Admin/SupportChat (có #supportInbox): danh sách khách, nội dung cuộc trò chuyện, trả lời.
 * Mọi nội dung hiển thị bằng textContent (không innerHTML). Quyền do máy chủ kiểm tra: khách không gọi được các hàm này.
 */
(function () {
    'use strict';

    if (typeof signalR === 'undefined') return;

    var badge = document.getElementById('supportUnreadBadge');
    var inbox = document.getElementById('supportInbox');
    if (!badge && !inbox) return;

    var MAX_LENGTH = 1000;
    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/support-chat')
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
        var hhmm = pad(d.getHours()) + ':' + pad(d.getMinutes());
        return d.toDateString() === new Date().toDateString() ? hhmm : hhmm + ' ' + pad(d.getDate()) + '/' + pad(d.getMonth() + 1);
    }

    function friendlyError(err) {
        var msg = err && err.message ? String(err.message) : '';
        var i = msg.indexOf('HubException: ');
        if (i >= 0) return msg.slice(i + 'HubException: '.length);
        return 'Không gửi được tin nhắn. Vui lòng thử lại.';
    }

    async function getJson(url) {
        var res = await fetch(url, { credentials: 'same-origin', headers: { 'Accept': 'application/json' } });
        if (!res.ok) throw new Error('http ' + res.status);
        return res.json();
    }

    // ---------- Huy hiệu ở menu ----------
    function setBadge(n) {
        if (!badge) return;
        badge.textContent = n > 99 ? '99+' : String(n);
        badge.hidden = n <= 0;
    }

    async function refreshBadge() {
        try {
            var data = await getJson('/api/support-chat/unread');
            setBadge(Number(data.unread) || 0);
        } catch (_) { /* giữ nguyên số cũ */ }
    }

    // ---------- Trang hộp thư ----------
    var conversations = new Map();   // maKh -> { maKh, tenKhach, email, tinCuoi, tinCuoiTuKhach, capNhatLuc, chuaDoc }
    var selected = null;             // maKh đang mở
    var seen = new Set();            // mã tin đã hiển thị trong cuộc trò chuyện đang mở
    var sending = false;

    var ui = null;
    if (inbox) {
        ui = {
            list: document.getElementById('scList'),
            listEmpty: document.getElementById('scListEmpty'),
            title: document.getElementById('scTitle'),
            sub: document.getElementById('scSub'),
            log: document.getElementById('scLog'),
            error: document.getElementById('scError'),
            form: document.getElementById('scForm'),
            input: document.getElementById('scInput'),
            send: document.getElementById('scSend')
        };
    }

    function showError(text) {
        if (!ui) return;
        ui.error.textContent = text;
        ui.error.hidden = false;
        setTimeout(function () { ui.error.hidden = true; }, 6000);
    }

    function renderList() {
        var items = Array.from(conversations.values()).sort(function (a, b) {
            return new Date(b.capNhatLuc) - new Date(a.capNhatLuc);
        });
        while (ui.list.firstChild) ui.list.removeChild(ui.list.firstChild);
        if (items.length === 0) {
            ui.list.appendChild(ui.listEmpty);
            return;
        }
        items.forEach(function (c) {
            var btn = el('button', 'sc-item' + (c.maKh === selected ? ' is-active' : '') + (c.chuaDoc > 0 ? ' has-unread' : ''));
            btn.type = 'button';
            btn.addEventListener('click', function () { openConversation(c.maKh); });

            var top = el('div', 'sc-item-top');
            top.appendChild(el('span', 'sc-item-name', c.tenKhach));
            top.appendChild(el('span', 'sc-item-time', formatTime(c.capNhatLuc)));
            var bottom = el('div', 'sc-item-bottom');
            bottom.appendChild(el('span', 'sc-item-snippet', (c.tinCuoiTuKhach ? '' : 'Bạn: ') + c.tinCuoi));
            if (c.chuaDoc > 0) bottom.appendChild(el('span', 'sc-item-badge', c.chuaDoc > 99 ? '99+' : String(c.chuaDoc)));
            btn.appendChild(top);
            btn.appendChild(bottom);
            ui.list.appendChild(btn);
        });
    }

    async function loadList() {
        try {
            var data = await getJson('/api/support-chat/conversations');
            conversations.clear();
            data.forEach(function (c) { conversations.set(c.maKh, c); });
            renderList();
        } catch (_) {
            showError('Không tải được danh sách khách hàng.');
        }
    }

    function clearLog() {
        while (ui.log.firstChild) ui.log.removeChild(ui.log.firstChild);
        seen.clear();
    }

    function appendMessage(m) {
        if (seen.has(m.maTn)) return;
        seen.add(m.maTn);
        var empty = ui.log.querySelector('.sc-empty');
        if (empty) empty.remove();

        var fromStaff = !m.tuKhach;
        var row = el('div', 'sc-row ' + (fromStaff ? 'is-staff' : 'is-customer'));
        row.appendChild(el('div', 'sc-bubble', String(m.noiDung == null ? '' : m.noiDung)));
        var who = fromStaff ? (m.tenNhanVien ? 'Nhân viên ' + m.tenNhanVien : 'Nhân viên') : (m.tenKhach || 'Khách');
        row.appendChild(el('div', 'sc-time', who + ' • ' + formatTime(m.guiLuc)));
        ui.log.appendChild(row);
        ui.log.scrollTop = ui.log.scrollHeight;
    }

    async function openConversation(maKh) {
        selected = maKh;
        var c = conversations.get(maKh);
        ui.title.textContent = c ? c.tenKhach : 'Khách hàng';
        ui.sub.textContent = c && c.email ? c.email : 'Khách hàng của Basau Bags';
        ui.input.disabled = false;
        ui.send.disabled = false;
        clearLog();
        renderList();

        try {
            var messages = await getJson('/api/support-chat/conversations/' + maKh);
            if (selected !== maKh) return;                // đã chuyển sang khách khác trong lúc tải
            if (messages.length === 0) ui.log.appendChild(el('div', 'sc-empty', 'Chưa có tin nhắn.'));
            messages.forEach(appendMessage);
            if (c && c.chuaDoc > 0) {
                c.chuaDoc = 0;
                renderList();
            }
            if (connection.state === signalR.HubConnectionState.Connected) {
                connection.invoke('MarkReadByStaff', maKh).catch(function () { });
            }
        } catch (_) {
            showError('Không tải được cuộc trò chuyện này.');
        }
        ui.input.focus();
    }

    async function sendReply() {
        if (sending || selected == null) return;
        var text = ui.input.value.trim();
        if (!text) return;
        if (connection.state !== signalR.HubConnectionState.Connected) {
            showError('Đang kết nối lại với máy chủ, vui lòng thử lại sau giây lát.');
            return;
        }
        sending = true;
        ui.send.disabled = true;
        try {
            await connection.invoke('SendToCustomer', selected, text);
            ui.input.value = '';
            ui.input.style.height = 'auto';
        } catch (err) {
            showError(friendlyError(err));
        } finally {
            sending = false;
            ui.send.disabled = false;
            ui.input.focus();
        }
    }

    if (ui) {
        ui.form.addEventListener('submit', function (e) { e.preventDefault(); sendReply(); });
        ui.input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey && !e.isComposing) {
                e.preventDefault();
                sendReply();
            }
        });
        ui.input.addEventListener('input', function () {
            ui.input.style.height = 'auto';
            ui.input.style.height = Math.min(ui.input.scrollHeight, 120) + 'px';
        });
    }

    // ---------- Sự kiện thời gian thực ----------
    connection.on('supportMessage', function (m) {
        if (!m) return;

        if (ui) {
            var c = conversations.get(m.maKh) || { maKh: m.maKh, tenKhach: m.tenKhach, email: null, chuaDoc: 0 };
            c.tenKhach = m.tenKhach || c.tenKhach;
            c.tinCuoi = String(m.noiDung || '').replace(/\n/g, ' ').slice(0, 80);
            c.tinCuoiTuKhach = !!m.tuKhach;
            c.capNhatLuc = m.guiLuc;
            var viewing = selected === m.maKh;
            if (m.tuKhach && !viewing) c.chuaDoc = (c.chuaDoc || 0) + 1;
            conversations.set(m.maKh, c);
            renderList();

            if (viewing) {
                appendMessage(m);
                if (m.tuKhach && connection.state === signalR.HubConnectionState.Connected) {
                    connection.invoke('MarkReadByStaff', m.maKh).catch(function () { });
                }
            }
        }

        refreshBadge();
    });

    // một nhân viên (tab này hoặc tab khác) đã đọc: đồng bộ số chưa đọc
    connection.on('supportRead', function (maKh) {
        if (ui && conversations.has(maKh)) {
            conversations.get(maKh).chuaDoc = 0;
            renderList();
        }
        refreshBadge();
    });

    connection.onreconnected(function () {
        refreshBadge();
        if (ui) {
            loadList();
            if (selected != null) openConversation(selected);
        }
    });

    async function start(attempt) {
        try {
            await connection.start();
            refreshBadge();
        } catch (_) {
            if (attempt < 5) setTimeout(function () { start(attempt + 1); }, 3000 * (attempt + 1));
        }
    }

    if (ui) loadList();
    refreshBadge();
    start(0);
})();
