//injectfavhtml: khởi tạo html giao diện yêu thích
function injectFavHTML() {
    if (document.getElementById('favPanel')) return;

    let overlay = document.createElement('div');
    overlay.id = 'favOverlay';
    overlay.onclick = closeFavPanel;

    let panel = document.createElement('div');
    panel.id = 'favPanel';

    panel.innerHTML = `
        <div class="panel-header">
            <div>
                <h5 class="panel-title">🤍 YÊU THÍCH</h5>
                <p id="favHeaderCount" class="panel-subtitle">0 sản phẩm</p>
            </div>
            <button onclick="closeFavPanel()" class="panel-close-btn">✕</button>
        </div>
        <div id="favItemsContainer" class="panel-body"></div>
    `;

    document.body.appendChild(overlay);
    document.body.appendChild(panel);
}

//getfavkey: lấy key lưu trữ yêu thích riêng theo từng tài khoản
function getFavKey() {
    let userData = localStorage.getItem('currentUser');
    if (userData && userData !== "null") {
        try {
            let u = JSON.parse(userData);
            if (u && u.username) {
                return 'tuixach_fav_' + u.username;
            }
        } catch(e) {}
    }
    return 'tuixach_fav_guest';
}

//getfavlist: lấy danh sách id sản phẩm yêu thích của user hiện tại
function getFavList() {
    let key = getFavKey();
    return JSON.parse(localStorage.getItem(key)) || [];
}

//savefavlist: lưu danh sách id sản phẩm yêu thích
function saveFavList(favs) {
    let key = getFavKey();
    localStorage.setItem(key, JSON.stringify(favs));
}

//checkloginforfav: kiểm tra đăng nhập trước khi thao tác với yêu thích
function checkLoginForFav(actionText) {
    let userData = localStorage.getItem('currentUser');
    if (!userData || userData === "null") {
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
        return false;
    }
    return true;
}

//openfavpanel: mở giao diện yêu thích
function openFavPanel() {
    if (!checkLoginForFav('xem danh sách túi xách yêu thích')) {
        return;
    }
    renderFavPanel();
    let panel = document.getElementById('favPanel');
    let overlay = document.getElementById('favOverlay');
    if (panel) panel.classList.add('open');
    if (overlay) overlay.classList.add('open');
    document.body.style.overflow = 'hidden';
}

//closefavpanel: đóng giao diện yêu thích
function closeFavPanel() {
    let panel = document.getElementById('favPanel');
    let overlay = document.getElementById('favOverlay');
    if (panel) panel.classList.remove('open');
    if (overlay) overlay.classList.remove('open');
    document.body.style.overflow = '';
}

//togglefavorite: thêm hoặc xóa khỏi danh sách yêu thích (Yêu cầu đăng nhập)
function toggleFavorite(productId) {
    if (!checkLoginForFav('thêm túi xách vào danh sách yêu thích')) {
        return;
    }

    let favs = getFavList();
    let index = favs.indexOf(productId);

    if (index === -1) {
        favs.push(productId);
        showFavToast("Đã thêm vào danh sách yêu thích! ❤️", "#e74c3c");
    } else {
        favs.splice(index, 1);
        showFavToast("Đã bỏ khỏi danh sách yêu thích.", "#555");
    }

    saveFavList(favs);
    renderFavPanel();
}

//removefavorite: xóa khỏi danh sách yêu thích
function removeFavorite(productId) {
    let favs = getFavList();
    let index = favs.indexOf(productId);
    if (index !== -1) {
        favs.splice(index, 1);
        saveFavList(favs);
        renderFavPanel();
    }
}

//renderfavpanel: tải danh sách sản phẩm yêu thích ra màn hình
function renderFavPanel() {
    let favIds = getFavList();
    let container = document.getElementById('favItemsContainer');
    let countEl = document.getElementById('favHeaderCount');
    if (!container) return;

    if (countEl) countEl.innerText = favIds.length + ' sản phẩm';

    if (favIds.length === 0) {
        container.innerHTML = `
            <div style="text-align:center; padding:70px 20px; color:#bbb;">
                <div style="font-size:64px; margin-bottom:18px; filter:grayscale(1);">🤍</div>
                <h5 style="color:#888;">Chưa có sản phẩm nào</h5>
                <p style="font-size:14px; margin-bottom:15px;">Hãy thả tim những mẫu túi xách bạn yêu thích nhé!</p>
                <button onclick="closeFavPanel()" class="btn btn-dark rounded-pill px-4 py-2 fw-bold">XEM SẢN PHẨM</button>
            </div>`;
        return;
    }

    let allProds = (typeof getAllProducts === 'function') ? getAllProducts() : (typeof productsDatabase !== 'undefined' ? productsDatabase : []);
    let htmlContent = "";

    for (let i = 0; i < favIds.length; i++) {
        let p = allProds.find(item => item.id === favIds[i]);
        if (p) {
            let priceFormat = p.price.toLocaleString('vi-VN') + ' ₫';

            htmlContent += `
                <div class="cart-item" style="cursor:pointer; transition: background-color 0.2s;" onmouseover="this.style.backgroundColor='#f9f9f9'" onmouseout="this.style.backgroundColor='transparent'" onclick="window.location.href='productDetail.html?id=${p.id}'">
                    <img src="${p.img}" class="cart-item-img rounded-3" alt="Túi xách" style="flex-shrink:0; object-fit: contain;">
                    <div style="flex:1; min-width:0;">
                        <div style="font-weight:700; font-size:16px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis; color:#111;">
                            ${p.name}
                        </div>
                        <div style="color:#888; font-size:13px; margin:2px 0;">${p.brand} · ${p.category}</div>
                        <div style="color:#dc3545; font-weight:700; font-size:15px;">${priceFormat}</div>
                    </div>
                    <button onclick="event.stopPropagation(); removeFavorite(${p.id})" class="cart-remove-btn" title="Xóa khỏi yêu thích">✕</button>
                </div>`;
        }
    }
    container.innerHTML = htmlContent;
}

//showfavtoast: hiện thông báo thao tác yêu thích bằng SweetAlert2
function showFavToast(msg, color) {
    Swal.fire({
        toast: true,
        position: 'bottom-end',
        showConfirmButton: false,
        timer: 2000,
        timerProgressBar: true,
        icon: color === '#e74c3c' ? 'success' : 'info',
        title: msg
    });
}

//initfavourite: khởi tạo hệ thống yêu thích
function initFavourite() {
    injectFavHTML();
}
initFavourite();

//handlefavlinkclick: xử lý click vào link yêu thích trên header
function handleFavLinkClick(e) {
    e.preventDefault();
    openFavPanel();
}