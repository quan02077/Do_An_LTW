//injectcarthtml: khởi tạo giao diện giỏ hàng
function injectCartHTML() {
    if (document.getElementById('cartPanel')) return;

    let overlay = document.createElement('div');
    overlay.id = 'cartOverlay';
    overlay.onclick = closeCartPanel;

    let panel = document.createElement('div');
    panel.id = 'cartPanel';
    panel.innerHTML = `
        <div class="panel-header">
            <div>
                <h5 class="panel-title">GIỎ HÀNG</h5>
                <p id="cartHeaderCount" class="panel-subtitle">0 sản phẩm</p>
            </div>
            <button onclick="closeCartPanel()" class="panel-close-btn">Đóng</button>
        </div>
        <div id="cartItemsContainer" class="panel-body"></div>
        <div id="cartFooter" class="panel-footer"></div>
    `;

    document.body.appendChild(overlay);
    document.body.appendChild(panel);
}

//checkloginforcart: kiểm tra đăng nhập trước khi thao tác với giỏ hàng
function checkLoginForCart(actionText) {
    let userData = localStorage.getItem('currentUser');
    if (!userData || userData === "null") {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'Yêu cầu đăng nhập',
                text: `Vui lòng đăng nhập tài khoản để ${actionText}!`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#111',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Đăng nhập ngay',
                cancelButtonText: 'Để sau'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = "login.html";
                }
            });
        } else {
            if (confirm(`Vui lòng đăng nhập tài khoản để ${actionText}!`)) {
                window.location.href = "login.html";
            }
        }
        return false;
    }
    return true;
}

//opencartpanel: mở giao diện giỏ hàng (Yêu cầu đăng nhập)
function openCartPanel() {
    if (!checkLoginForCart('xem giỏ hàng')) {
        return;
    }
    renderCartPanel();
    let panel = document.getElementById('cartPanel');
    let overlay = document.getElementById('cartOverlay');
    if (panel) panel.classList.add('open');
    if (overlay) overlay.classList.add('open');
    document.body.style.overflow = 'hidden';
}

//closecartpanel: đóng giao diện giỏ hàng
function closeCartPanel() {
    let panel = document.getElementById('cartPanel');
    let overlay = document.getElementById('cartOverlay');
    if (panel) panel.classList.remove('open');
    if (overlay) overlay.classList.remove('open');
    document.body.style.overflow = '';
}

//rendercartpanel: hiển thị danh sách túi xách trong giỏ hàng
function renderCartPanel() {
    let cart = getCart();
    let container = document.getElementById('cartItemsContainer');
    let footer = document.getElementById('cartFooter');
    let countEl = document.getElementById('cartHeaderCount');
    if (!container || !footer) return;

    let totalQty = 0, totalPrice = 0;
    cart.forEach(item => {
        totalQty += item.quantity;
        totalPrice += (item.price * item.quantity);
    });
    if (countEl) countEl.textContent = totalQty + ' sản phẩm';

    if (cart.length === 0) {
        container.innerHTML = `
            <div style="text-align:center; padding:70px 20px; color:#bbb;">
                <p class="text-uppercase fw-bold text-secondary small mb-2" style="letter-spacing: 1px;">Giỏ hàng trống</p>
                <h5 style="color:#18181b;" class="fw-bold mb-2">Chưa có sản phẩm trong giỏ</h5>
                <p class="small text-muted mb-4">Hãy chọn cho mình một chiếc túi hoặc balo ưng ý nhé!</p>
                <button onclick="closeCartPanel()" class="btn btn-dark rounded-pill px-4 py-2 fw-bold">MUA SẮM NGAY</button>
            </div>`;
        footer.innerHTML = '';
        return;
    }

    let htmlContent = "";
    cart.forEach((item, i) => {
        let brandCat = (item.brand || '') + (item.category ? ` · ${item.category}` : '');
        htmlContent += `
            <div class="cart-item">
                <img src="${item.img}" class="cart-item-img rounded-3" style="object-fit: contain;">
                <div style="flex:1; min-width:0;">
                    <div style="font-weight:700; font-size:16px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;">${item.name}</div>
                    <div style="color:#888; font-size:13px; margin:2px 0;">${brandCat}</div>
                    <div style="color:#dc3545; font-weight:700; font-size:15px;">${(item.price * item.quantity).toLocaleString('vi-VN')} ₫</div>
                    <div style="display:flex; align-items:center; gap:10px; margin-top:8px;">
                        <button class="cart-qty-btn" onclick="changeCartQty(${i}, -1)">−</button>
                        <span style="font-weight:700; font-size:15px;">${item.quantity}</span>
                        <button class="cart-qty-btn" onclick="changeCartQty(${i}, 1)">+</button>
                    </div>
                </div>
                <button class="cart-remove-btn" onclick="removeCartItem(${i})">✕</button>
            </div>`;
    });
    container.innerHTML = htmlContent;

    footer.innerHTML = `
        <div style="display:flex; justify-content:space-between; align-items:baseline; margin-bottom:16px;">
            <span style="color:#888; font-weight:bold;">Tổng cộng:</span>
            <span style="font-size:22px; font-weight:800; color:#dc3545;">${totalPrice.toLocaleString('vi-VN')} ₫</span>
        </div>
        <button class="btn btn-dark w-100 rounded-pill py-3 fw-bold" onclick="window.location.href='checkout.html'">TIẾN HÀNH THANH TOÁN</button>
        <button class="cart-clear-btn mt-2" onclick="clearCart()">Xóa tất cả</button>
    `;
}

//getcartkey: lấy key giỏ hàng của người dùng hiện tại
function getCartKey() {
    let userData = localStorage.getItem('currentUser');
    let user = userData ? JSON.parse(userData) : null;

    if (user && user.username) {
        return 'tuixach_cart_' + user.username;
    }

    return 'tuixach_cart_guest';
}

//getcart: lấy danh sách sản phẩm trong giỏ hàng
function getCart() {
    let key = getCartKey();
    return JSON.parse(localStorage.getItem(key)) || [];
}

//savecart: lưu danh sách sản phẩm vào giỏ hàng
function saveCart(cart) {
    let key = getCartKey();
    localStorage.setItem(key, JSON.stringify(cart));
    updateCartCount();
}

//changecartqty: thay đổi số lượng túi xách trong giỏ
function changeCartQty(index, delta) {
    let cart = getCart();
    if (!cart[index]) return;

    let newQty = cart[index].quantity + delta;
    let maxStock = cart[index].maxStock || 99;

    if (newQty > maxStock) {
        Swal.fire({
            title: 'Tồn kho giới hạn',
            text: `Sản phẩm này chỉ còn ${maxStock} cái trong kho!`,
            icon: 'info'
        });
        return;
    }

    if (newQty <= 0) {
        removeCartItem(index);
        return;
    }

    cart[index].quantity = newQty;
    saveCart(cart);
    renderCartPanel();
}

//removecartitem: xóa một sản phẩm khỏi giỏ
function removeCartItem(index) {
    let cart = getCart();
    cart.splice(index, 1);
    saveCart(cart);
    renderCartPanel();
}

//clearcart: xóa toàn bộ giỏ hàng
function clearCart() {
    Swal.fire({
        title: 'Xóa toàn bộ?',
        text: 'Bạn có chắc chắn muốn xóa hết túi xách trong giỏ hàng?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#111',
        confirmButtonText: 'Xóa hết',
        cancelButtonText: 'Giữ lại'
    }).then(result => {
        if (result.isConfirmed) {
            saveCart([]);
            renderCartPanel();
        }
    });
}

//updatecartcount: cập nhật số lượng hiển thị trên menu header
function updateCartCount() {
    let userData = localStorage.getItem('currentUser');
    let countText = document.getElementById('cartCountText');
    if (!countText) return;

    if (!userData || userData === "null") {
        countText.innerText = `Giỏ hàng (0)`;
        return;
    }

    let cart = getCart();
    let totalQty = cart.reduce((sum, item) => sum + item.quantity, 0);
    countText.innerText = `Giỏ hàng (${totalQty})`;
}

//handlecartlinkclick: xử lý khi bấm vào icon giỏ hàng
function handleCartLinkClick(e) {
    e.preventDefault();
    openCartPanel();
}

document.addEventListener('DOMContentLoaded', () => {
    injectCartHTML();
    updateCartCount();
});