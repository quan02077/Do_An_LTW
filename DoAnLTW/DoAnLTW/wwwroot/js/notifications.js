(function () {
    'use strict';

    const MAX_ITEMS = 10;
    const STORAGE_PREFIX = 'basau_order_noti_v1_';

    // Chuẩn hóa dữ liệu đơn hàng từ SignalR (mặc định camelCase)
    function normalizeOrder(raw) {
        if (!raw || typeof raw !== 'object') return null;
        return {
            maDh: Number(raw.maDh ?? raw.MaDh ?? 0),
            maHienThi: String(raw.maHienThi ?? raw.MaHienThi ?? ''),
            maKh: Number(raw.maKh ?? raw.MaKh ?? 0),
            tenNguoiNhan: String(raw.tenNguoiNhan ?? raw.TenNguoiNhan ?? ''),
            tongTien: Number(raw.tongTien ?? raw.TongTien ?? 0),
            ngayDat: raw.ngayDat ?? raw.NgayDat ?? null,
            trangThai: String(raw.trangThai ?? raw.TrangThai ?? ''),
            trangThaiCu: raw.trangThaiCu ?? raw.TrangThaiCu ?? null
        };
    }

    function formatCurrency(value) {
        const num = Number(value);
        if (!Number.isFinite(num)) return '0 đ';
        return num.toLocaleString('vi-VN') + ' đ';
    }

    function formatDateTime(value) {
        const d = value ? new Date(value) : new Date();
        if (Number.isNaN(d.getTime())) return '';
        const pad = function (n) { return String(n).padStart(2, '0'); };
        return pad(d.getDate()) + '/' + pad(d.getMonth() + 1) + '/' + d.getFullYear() +
            ' ' + pad(d.getHours()) + ':' + pad(d.getMinutes());
    }

    function getStatusBadgeClass(status) {
        switch (status) {
            case 'Chờ duyệt':
                return 'bg-light text-secondary border border-warning';
            case 'Đang giao':
                return 'bg-light text-secondary border border-primary';
            case 'Hoàn tất':
                return 'bg-light text-secondary border border-success';
            case 'Đã hủy':
                return 'bg-light text-secondary border border-danger';
            default:
                return 'bg-light text-secondary border border-secondary';
        }
    }

    function getStorageKey(bellEl) {
        const scope = (bellEl && bellEl.getAttribute('data-noti-scope')) || 'default';
        return STORAGE_PREFIX + scope;
    }

    function loadState(storageKey) {
        try {
            const raw = sessionStorage.getItem(storageKey);
            if (!raw) return { unread: 0, items: [] };
            const parsed = JSON.parse(raw);
            if (!parsed || !Array.isArray(parsed.items)) return { unread: 0, items: [] };
            return {
                unread: Number.isInteger(parsed.unread) && parsed.unread >= 0 ? parsed.unread : 0,
                items: parsed.items.slice(0, MAX_ITEMS)
            };
        } catch (_) {
            return { unread: 0, items: [] };
        }
    }

    function saveState(storageKey, state) {
        try {
            sessionStorage.setItem(storageKey, JSON.stringify({
                unread: state.unread,
                items: state.items.slice(0, MAX_ITEMS)
            }));
        } catch (_) {
            // Bỏ qua khi trình duyệt chặn sessionStorage hoặc đầy bộ nhớ
        }
    }

    // Hiện toast bằng SweetAlert2 nếu có sẵn, ngược lại dùng DOM toast nội bộ (chạy offline)
    function showToast(icon, title, message) {
        if (typeof window.Swal !== 'undefined' && typeof window.Swal.mixin === 'function') {
            const toast = window.Swal.mixin({
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 5000,
                timerProgressBar: true
            });
            toast.fire({
                icon: icon || 'info',
                title: String(title || ''),
                text: String(message || '')
            });
            return;
        }

        let stack = document.getElementById('notiToastStack');
        if (!stack) {
            stack = document.createElement('div');
            stack.id = 'notiToastStack';
            stack.className = 'noti-toast-stack';
            document.body.appendChild(stack);
        }

        const card = document.createElement('div');
        card.className = 'noti-toast-card is-' + (icon || 'info');
        card.setAttribute('role', 'status');

        const titleEl = document.createElement('div');
        titleEl.className = 'noti-toast-title';
        titleEl.textContent = String(title || '');

        const bodyEl = document.createElement('div');
        bodyEl.className = 'noti-toast-body';
        bodyEl.textContent = String(message || '');

        card.appendChild(titleEl);
        card.appendChild(bodyEl);
        stack.appendChild(card);

        window.setTimeout(function () {
            if (card.parentNode) {
                card.parentNode.removeChild(card);
            }
        }, 5000);
    }

    function isSafeRelativeUrl(url) {
        return typeof url === 'string' && url.startsWith('/') && !url.startsWith('//');
    }

    // Vẽ lại badge số chưa đọc và danh sách thông báo trong chuông bằng DOM/textContent
    function renderBell(bellEl, state) {
        if (!bellEl) return;
        const badgeEl = bellEl.querySelector('[data-noti-badge]');
        const listEl = bellEl.querySelector('[data-noti-list]');

        if (badgeEl) {
            if (state.unread > 0) {
                badgeEl.textContent = state.unread > 99 ? '99+' : String(state.unread);
                badgeEl.classList.remove('d-none');
            } else {
                badgeEl.textContent = '0';
                badgeEl.classList.add('d-none');
            }
        }

        if (!listEl) return;
        listEl.replaceChildren();

        if (!state.items.length) {
            const emptyLi = document.createElement('li');
            emptyLi.className = 'noti-empty';
            emptyLi.textContent = 'Chưa có thông báo nào.';
            listEl.appendChild(emptyLi);
            return;
        }

        state.items.forEach(function (item) {
            const li = document.createElement('li');
            li.className = 'noti-item' + (item.read ? '' : ' is-unread');

            const link = document.createElement('a');
            link.className = 'noti-item-link';
            link.href = isSafeRelativeUrl(item.url) ? item.url : '#';

            const titleDiv = document.createElement('div');
            titleDiv.className = 'noti-item-title';
            titleDiv.textContent = String(item.title || '');

            const msgDiv = document.createElement('div');
            msgDiv.className = 'noti-item-message';
            msgDiv.textContent = String(item.message || '');

            const timeDiv = document.createElement('div');
            timeDiv.className = 'noti-item-time';
            timeDiv.textContent = String(item.timeText || '');

            link.appendChild(titleDiv);
            link.appendChild(msgDiv);
            link.appendChild(timeDiv);
            li.appendChild(link);
            listEl.appendChild(li);
        });
    }

    // Trang quản lý đơn hàng Admin: chèn dòng mới lên đầu bảng hoặc hiện banner nếu bộ lọc không khớp
    function handleAdminOrderList(order) {
        const normalizedPath = window.location.pathname.toLowerCase().replace(/\/+$/, '');
        if (normalizedPath !== '/admin/adminorder' && normalizedPath !== '/admin/adminorder/index') {
            return;
        }

        const tbody = document.getElementById('adminOrderTableBody');
        if (!tbody) return;

        const keyword = (tbody.getAttribute('data-keyword') || '').trim();
        const selectedStatus = (tbody.getAttribute('data-status') || '').trim();
        const orderStatus = order.trangThai || 'Chờ duyệt';

        const statusMatches = selectedStatus === '' ||
            selectedStatus.toLowerCase() === 'all' ||
            selectedStatus === orderStatus;
        const filterMatches = keyword === '' && statusMatches;

        const template = tbody.getAttribute('data-detail-url-template') || '/Admin/AdminOrder/Detail/__ID__';
        const safeId = Number.isInteger(order.maDh) && order.maDh > 0 ? String(order.maDh) : '';
        const detailUrl = safeId ? template.replace('__ID__', encodeURIComponent(safeId)) : '/Admin/AdminOrder';

        if (!filterMatches) {
            showAdminNewOrderBanner(order, detailUrl);
            return;
        }

        // Tránh chèn trùng dòng nếu đơn đã xuất hiện trong bảng
        if (safeId && tbody.querySelector('tr[data-order-id="' + safeId + '"]')) {
            return;
        }

        const emptyRow = tbody.querySelector('tr[data-empty-row="true"]');
        if (emptyRow) {
            emptyRow.remove();
        }

        const tr = document.createElement('tr');
        tr.className = 'noti-row-new';
        if (safeId) {
            tr.setAttribute('data-order-id', safeId);
        }

        // Cột 1: Mã đơn
        const tdCode = document.createElement('td');
        tdCode.className = 'text-center';
        const codeLink = document.createElement('a');
        codeLink.href = detailUrl;
        codeLink.className = 'body-title-2 fw-bold text-primary';
        codeLink.textContent = order.maHienThi || ('DH' + safeId);
        tdCode.appendChild(codeLink);

        // Cột 2: Khách hàng (chỉ hiện tên người nhận, không có SĐT/email trong payload)
        const tdCustomer = document.createElement('td');
        const nameDiv = document.createElement('div');
        nameDiv.className = 'fw-bold text-dark';
        nameDiv.textContent = order.tenNguoiNhan || 'Khách hàng';
        tdCustomer.appendChild(nameDiv);

        // Cột 3: Ngày đặt
        const tdDate = document.createElement('td');
        tdDate.className = 'text-center text-muted small';
        tdDate.textContent = formatDateTime(order.ngayDat);

        // Cột 4: Tổng tiền
        const tdTotal = document.createElement('td');
        tdTotal.className = 'text-center fw-bold text-danger';
        tdTotal.textContent = formatCurrency(order.tongTien);

        // Cột 5: Địa chỉ giao hàng (không gửi dữ liệu nhạy cảm qua SignalR)
        const tdAddress = document.createElement('td');
        const addrSpan = document.createElement('span');
        addrSpan.className = 'small text-secondary';
        addrSpan.textContent = 'Xem trong chi tiết';
        tdAddress.appendChild(addrSpan);

        // Cột 6: Trạng thái
        const tdStatus = document.createElement('td');
        tdStatus.className = 'text-center';
        const statusBadge = document.createElement('span');
        statusBadge.className = 'badge px-3 py-2 rounded-pill ' + getStatusBadgeClass(orderStatus);
        statusBadge.textContent = orderStatus;
        tdStatus.appendChild(statusBadge);

        // Cột 7: Thao tác
        const tdAction = document.createElement('td');
        tdAction.className = 'text-center';
        const funcWrap = document.createElement('div');
        funcWrap.className = 'list-icon-function justify-content-center';
        const actionLink = document.createElement('a');
        actionLink.href = detailUrl;
        actionLink.title = 'Xem chi tiết & Đổi trạng thái';
        const itemDiv = document.createElement('div');
        itemDiv.className = 'item edit text-primary';
        const eyeIcon = document.createElement('i');
        eyeIcon.className = 'icon-eye';
        itemDiv.appendChild(eyeIcon);
        actionLink.appendChild(itemDiv);
        funcWrap.appendChild(actionLink);
        tdAction.appendChild(funcWrap);

        tr.appendChild(tdCode);
        tr.appendChild(tdCustomer);
        tr.appendChild(tdDate);
        tr.appendChild(tdTotal);
        tr.appendChild(tdAddress);
        tr.appendChild(tdStatus);
        tr.appendChild(tdAction);

        tbody.insertBefore(tr, tbody.firstChild);
    }

    function showAdminNewOrderBanner(order, detailUrl) {
        const container = document.getElementById('adminOrderRealtimeBanner');
        if (!container) return;

        container.classList.remove('d-none');
        container.replaceChildren();

        const banner = document.createElement('div');
        banner.className = 'noti-inline-banner';
        banner.setAttribute('role', 'status');

        const textSpan = document.createElement('span');
        textSpan.textContent = 'Có đơn mới ' + (order.maHienThi ? ('#' + order.maHienThi + ' ') : '') +
            'không hiển thị do bộ lọc hiện tại.';

        const actions = document.createElement('div');
        actions.className = 'noti-inline-banner-actions';

        const detailLink = document.createElement('a');
        detailLink.href = isSafeRelativeUrl(detailUrl) ? detailUrl : '/Admin/AdminOrder';
        detailLink.className = 'btn btn-sm btn-outline-dark';
        detailLink.textContent = 'Xem chi tiết';

        const resetLink = document.createElement('a');
        resetLink.href = '/Admin/AdminOrder';
        resetLink.className = 'btn btn-sm btn-dark';
        resetLink.textContent = 'Xem tất cả đơn';

        actions.appendChild(detailLink);
        actions.appendChild(resetLink);
        banner.appendChild(textSpan);
        banner.appendChild(actions);
        container.appendChild(banner);
    }

    // Trang lịch sử đơn hàng của khách: hiện banner yêu cầu tải lại để xem trạng thái mới
    function handleCustomerOrderHistory(order) {
        const normalizedPath = window.location.pathname.toLowerCase().replace(/\/+$/, '');
        if (normalizedPath !== '/orders/history') {
            return;
        }

        let banner = document.getElementById('customerOrderReloadBanner');
        if (!banner) {
            banner = document.createElement('div');
            banner.id = 'customerOrderReloadBanner';
            banner.className = 'noti-inline-banner';
            banner.setAttribute('role', 'status');

            const shopPage = document.querySelector('section.shop-page');
            const heading = shopPage ? shopPage.querySelector('.page-heading') : null;
            if (heading && heading.parentNode) {
                heading.parentNode.insertBefore(banner, heading.nextSibling);
            } else if (shopPage) {
                shopPage.insertBefore(banner, shopPage.firstChild);
            } else {
                const main = document.querySelector('main');
                if (main) {
                    main.insertBefore(banner, main.firstChild);
                } else {
                    return;
                }
            }
        }

        banner.replaceChildren();

        const msgSpan = document.createElement('span');
        msgSpan.textContent = 'Đơn hàng #' + (order.maHienThi || order.maDh) +
            ' đã chuyển sang trạng thái "' + order.trangThai + '".';

        const reloadBtn = document.createElement('button');
        reloadBtn.type = 'button';
        reloadBtn.className = 'btn btn-sm btn-dark';
        reloadBtn.textContent = 'Tải lại để xem trạng thái mới';
        reloadBtn.addEventListener('click', function () {
            window.location.reload();
        });

        banner.appendChild(msgSpan);
        banner.appendChild(reloadBtn);
    }

    function init() {
        const bellEl = document.querySelector('[data-noti-bell]');
        if (!bellEl) return;

        const storageKey = getStorageKey(bellEl);
        const state = loadState(storageKey);
        renderBell(bellEl, state);

        const toggleBtn = bellEl.querySelector('[data-noti-toggle]');
        const clearBtn = bellEl.querySelector('[data-noti-clear]');

        function closePanel() {
            bellEl.classList.remove('is-open');
            if (toggleBtn) toggleBtn.setAttribute('aria-expanded', 'false');
        }

        if (toggleBtn) {
            toggleBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                const willOpen = !bellEl.classList.contains('is-open');
                bellEl.classList.toggle('is-open', willOpen);
                toggleBtn.setAttribute('aria-expanded', willOpen ? 'true' : 'false');

                // Bấm chuông thì đánh dấu đã đọc toàn bộ thông báo hiện có
                if (willOpen && state.unread > 0) {
                    state.unread = 0;
                    state.items.forEach(function (it) { it.read = true; });
                    saveState(storageKey, state);
                    renderBell(bellEl, state);
                }
            });
        }

        if (clearBtn) {
            clearBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                state.unread = 0;
                state.items = [];
                saveState(storageKey, state);
                renderBell(bellEl, state);
            });
        }

        document.addEventListener('click', function (e) {
            if (!bellEl.contains(e.target)) {
                closePanel();
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closePanel();
            }
        });

        // Khởi tạo kết nối SignalR khi thư viện đã được nạp
        if (typeof window.signalR === 'undefined') {
            return;
        }

        function pushNotification(entry) {
            const isOpen = bellEl.classList.contains('is-open');
            state.items.unshift({
                title: entry.title,
                message: entry.message,
                timeText: entry.timeText,
                url: entry.url,
                read: isOpen
            });
            if (state.items.length > MAX_ITEMS) {
                state.items = state.items.slice(0, MAX_ITEMS);
            }
            if (!isOpen) {
                state.unread += 1;
            }
            saveState(storageKey, state);
            renderBell(bellEl, state);
        }

        const connection = new window.signalR.HubConnectionBuilder()
            .withUrl('/hubs/notifications')
            .withAutomaticReconnect()
            .configureLogging(window.signalR.LogLevel.Warning)
            .build();

        connection.on('newOrder', function (raw) {
            const order = normalizeOrder(raw);
            if (!order) return;

            const title = 'Đơn hàng mới #' + (order.maHienThi || order.maDh);
            const message = (order.tenNguoiNhan ? order.tenNguoiNhan + ' · ' : '') + formatCurrency(order.tongTien);
            const url = Number.isInteger(order.maDh) && order.maDh > 0
                ? '/Admin/AdminOrder/Detail/' + encodeURIComponent(String(order.maDh))
                : '/Admin/AdminOrder';

            showToast('info', title, message);
            pushNotification({
                title: title,
                message: message,
                timeText: formatDateTime(order.ngayDat),
                url: url
            });
            handleAdminOrderList(order);
        });

        connection.on('orderCancelled', function (raw) {
            const order = normalizeOrder(raw);
            if (!order) return;

            const title = 'Khách đã hủy đơn #' + (order.maHienThi || order.maDh);
            const message = (order.tenNguoiNhan ? order.tenNguoiNhan + ' · ' : '') + formatCurrency(order.tongTien);
            const url = Number.isInteger(order.maDh) && order.maDh > 0
                ? '/Admin/AdminOrder/Detail/' + encodeURIComponent(String(order.maDh))
                : '/Admin/AdminOrder';

            showToast('warning', title, message);
            pushNotification({
                title: title,
                message: message,
                timeText: formatDateTime(order.ngayDat),
                url: url
            });
        });

        connection.on('orderStatusChanged', function (raw) {
            const order = normalizeOrder(raw);
            if (!order) return;

            const title = 'Cập nhật đơn #' + (order.maHienThi || order.maDh);
            const message = order.trangThaiCu
                ? 'Trạng thái: ' + order.trangThaiCu + ' → ' + order.trangThai
                : 'Trạng thái mới: ' + order.trangThai;
            const icon = order.trangThai === 'Đã hủy' ? 'warning' : 'success';

            showToast(icon, title, message);
            pushNotification({
                title: title,
                message: message,
                timeText: formatDateTime(new Date().toISOString()),
                url: '/Orders/History'
            });
            handleCustomerOrderHistory(order);
        });

        let retryAttempt = 0;
        const maxInitialRetries = 5;

        function startConnection() {
            connection.start().catch(function () {
                if (retryAttempt >= maxInitialRetries) return;
                const delayMs = Math.min(3000 * Math.pow(2, retryAttempt), 30000);
                retryAttempt += 1;
                window.setTimeout(startConnection, delayMs);
            });
        }

        startConnection();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
