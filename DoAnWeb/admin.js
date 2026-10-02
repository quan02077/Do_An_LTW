const BRAND_MAP = {
    "1": "CHARLES & KEITH",
    "2": "Natoli",
    "3": "Pedro",
    "4": "ELLY",
    "5": "Jamlos"
};

const CATEGORY_MAP = {
    "1": "Balo",
    "2": "Túi đeo chéo",
    "3": "Túi tote",
    "4": "Túi dự tiệc",
    "5": "Túi đeo vai",
    "6": "Clutch"
};

const PROMO_MAP = {
    "0": { name: "Không khuyến mãi", discount: 0 },
    "1": { name: "Khuyến mãi tháng 9", discount: 10 },
    "2": { name: "Sale cuối tuần", discount: 15 },
    "3": { name: "Ưu đãi khách hàng mới", discount: 20 },
    "4": { name: "Sale mùa thu", discount: 25 },
    "5": { name: "Siêu sale cuối năm", discount: 30 }
};

//getallproducts: lấy danh sách tất cả sản phẩm túi xách bao gồm thêm mới và chỉnh sửa
function getAllProducts() {
    let base = [];
    if (typeof productsDatabase !== 'undefined') {
        for (let i = 0; i < productsDatabase.length; i++) {
            base.push({ ...productsDatabase[i] });
        }
    }
    let addedData = localStorage.getItem('basau_addedProducts');
    let added = addedData ? JSON.parse(addedData) : [];
    let editedData = localStorage.getItem('basau_editedProducts');
    let edited = editedData ? JSON.parse(editedData) : {};
    let deletedData = localStorage.getItem('basau_deletedIds');
    let deleted = deletedData ? JSON.parse(deletedData) : [];

    let allItems = [];
    for (let i = added.length - 1; i >= 0; i--) {
        allItems.push({ ...added[i] });
    }
    for (let i = 0; i < base.length; i++) {
        let isDuplicate = added.some(a => a.id === base[i].id);
        if (!isDuplicate) {
            allItems.push(base[i]);
        }
    }

    let finalResult = [];
    for (let i = 0; i < allItems.length; i++) {
        let item = allItems[i];
        if (!deleted.includes(item.id)) {
            if (edited[item.id]) {
                let e = edited[item.id];
                Object.assign(item, e);
            }
            finalResult.push(item);
        }
    }
    return finalResult;
}

//patchproductsdatabase: ghi đè lại dữ liệu tạm thời
function patchProductsDatabase() {
    if (typeof productsDatabase === 'undefined') return;
    let patched = getAllProducts();
    productsDatabase.length = 0;
    for (let i = 0; i < patched.length; i++) productsDatabase.push(patched[i]);
}

//switchadmintab: chuyển đổi tab giao diện admin
function switchAdminTab(tab) {
    let tabs = ['add', 'edit', 'delete', 'orders', 'stats'];
    for (let i = 0; i < tabs.length; i++) {
        let t = tabs[i];
        let panel = document.getElementById('adm-panel-' + t);
        let btn = document.getElementById('adm-tab-' + t);

        if (panel) panel.classList.remove('active');
        if (btn) {
            btn.classList.remove('active', 'border-bottom', 'border-3', 'border-dark');
        }

        if (t === tab) {
            if (panel) panel.classList.add('active');
            if (btn) {
                btn.classList.add('active', 'border-bottom', 'border-3', 'border-dark');
            }
        }
    }
    if (tab === 'edit') loadEditList();
    if (tab === 'delete') loadDeleteList();
    if (tab === 'orders') loadOrdersList();
    if (tab === 'stats') loadStatsDashboard();
}

let currentEditId = null;

//selectbadge: chọn nhãn dán cho sản phẩm
function selectBadge(mode, btn) {
    let buttons = document.getElementById(mode + 'BadgeRow').querySelectorAll('.adm-badge-opt');
    for (let i = 0; i < buttons.length; i++) buttons[i].classList.remove('selected', 'btn-dark');
    btn.classList.add('selected', 'btn-dark');
    document.getElementById(mode + '_badge').value = btn.getAttribute('data-v');
}

function setBadge(mode, val) {
    if (!val) val = '';
    let buttons = document.getElementById(mode + 'BadgeRow').querySelectorAll('.adm-badge-opt');
    for (let i = 0; i < buttons.length; i++) {
        if (buttons[i].getAttribute('data-v') === val) {
            buttons[i].classList.add('selected', 'btn-dark');
        } else {
            buttons[i].classList.remove('selected', 'btn-dark');
        }
    }
    document.getElementById(mode + '_badge').value = val;
}

//submitaddproduct: thêm sản phẩm túi xách mới (bảng tuixach)
function submitAddProduct() {
    let name = document.getElementById('add_name').value.trim();
    let brandId = document.getElementById('add_brand').value;
    let catId = document.getElementById('add_category').value;
    let kmId = document.getElementById('add_km').value;
    let price = parseInt(document.getElementById('add_price').value) || 0;
    let stock = parseInt(document.getElementById('add_stock').value) || 0;
    let img = document.getElementById('add_img').value.trim();
    let mota = document.getElementById('add_mota').value.trim();
    let badge = document.getElementById('add_badge').value;

    if (name === "" || price <= 0 || img === "") {
        Swal.fire('Thiếu thông tin', 'Vui lòng điền đầy đủ Tên túi, Đơn giá và Ảnh sản phẩm!', 'warning');
        return;
    }

    let products = getAllProducts();
    let maxId = products.reduce((max, p) => Math.max(max, p.id || 0), 0);

    let promoInfo = PROMO_MAP[kmId] || { name: "Không khuyến mãi", discount: 0 };
    let discountPercent = promoInfo.discount;
    let finalPrice = discountPercent > 0 ? Math.round(price * (1 - discountPercent / 100)) : price;
    let oldPrice = discountPercent > 0 ? price : 0;

    let newProduct = {
        id: maxId + 1,
        matui: maxId + 1,
        name: name,
        mathuonghieu: parseInt(brandId),
        brand: BRAND_MAP[brandId] || "Khác",
        maloai: parseInt(catId),
        category: CATEGORY_MAP[catId] || "Túi đeo chéo",
        dongia: price,
        soluong: stock,
        mota: mota || `Sản phẩm ${name} cao cấp thời trang.`,
        makm: kmId === "0" ? null : parseInt(kmId),
        promotionName: promoInfo.name,
        discountPercent: discountPercent,
        oldPrice: oldPrice,
        price: finalPrice,
        badge: badge || (discountPercent > 0 ? "Sale Off" : ""),
        img: img,
        thumbnails: [img]
    };

    let addedData = localStorage.getItem('basau_addedProducts');
    let added = addedData ? JSON.parse(addedData) : [];
    added.push(newProduct);
    localStorage.setItem('basau_addedProducts', JSON.stringify(added));

    Swal.fire({
        icon: 'success',
        title: 'Thêm thành công!',
        text: `Đã thêm túi xách "${name}" vào kho hàng.`,
        timer: 1500,
        showConfirmButton: false
    }).then(() => {
        location.reload();
    });
}

//loadeditlist: danh sách túi xách trong tab chỉnh sửa
function loadEditList() {
    let container = document.getElementById('editList');
    if (!container) return;

    let search = (document.getElementById('editSearch')?.value || '').toLowerCase();
    let products = getAllProducts().filter(p => {
        return (p.name || '').toLowerCase().includes(search) || (p.brand || '').toLowerCase().includes(search);
    });

    if (products.length === 0) {
        container.innerHTML = "<p class='text-muted text-center py-3'>Không tìm thấy túi xách nào.</p>";
        return;
    }

    let html = "";
    products.forEach(p => {
        html += `
            <div class="d-flex justify-content-between align-items-center p-3 mb-2 bg-white rounded-3 shadow-sm border">
                <div class="d-flex align-items-center gap-3">
                    <img src="${p.img}" class="rounded-2" style="width: 50px; height: 50px; object-fit: contain;">
                    <div>
                        <div class="fw-bold fs-6">${p.name}</div>
                        <div class="small text-muted">${p.brand} · ${p.category} | Giá: <strong class="text-danger">${p.price.toLocaleString('vi-VN')}₫</strong> | Kho: <strong>${p.soluong || 0}</strong></div>
                    </div>
                </div>
                <button class="btn btn-outline-primary btn-sm rounded-pill px-3 fw-bold" onclick="selectProductToEdit(${p.id})">Sửa</button>
            </div>
        `;
    });
    container.innerHTML = html;
}

//selectproducttoedit: chọn sản phẩm để sửa
function selectProductToEdit(id) {
    let products = getAllProducts();
    let p = products.find(item => item.id === id);
    if (!p) return;

    currentEditId = id;

    document.getElementById('edit_name').value = p.name || '';
    document.getElementById('edit_price').value = p.dongia || p.oldPrice || p.price;
    document.getElementById('edit_stock').value = p.soluong || 0;
    document.getElementById('edit_img').value = p.img || '';
    document.getElementById('edit_mota').value = p.mota || '';

    let brandSelect = document.getElementById('edit_brand');
    if (brandSelect && p.mathuonghieu) brandSelect.value = p.mathuonghieu;

    let catSelect = document.getElementById('edit_category');
    if (catSelect && p.maloai) catSelect.value = p.maloai;

    let kmSelect = document.getElementById('edit_km');
    if (kmSelect) kmSelect.value = p.makm ? String(p.makm) : "0";

    setBadge('edit', p.badge || '');

    let wrap = document.getElementById('editFormWrap');
    if (wrap) {
        wrap.style.display = 'block';
        wrap.scrollIntoView({ behavior: 'smooth' });
    }
}

//submiteditproduct: xác nhận sửa sản phẩm
function submitEditProduct() {
    if (currentEditId === null) return;

    let name = document.getElementById('edit_name').value.trim();
    let brandId = document.getElementById('edit_brand').value;
    let catId = document.getElementById('edit_category').value;
    let kmId = document.getElementById('edit_km').value;
    let price = parseInt(document.getElementById('edit_price').value) || 0;
    let stock = parseInt(document.getElementById('edit_stock').value) || 0;
    let img = document.getElementById('edit_img').value.trim();
    let mota = document.getElementById('edit_mota').value.trim();
    let badge = document.getElementById('edit_badge').value;

    if (name === "" || price <= 0 || img === "") {
        Swal.fire('Lỗi', 'Vui lòng điền đầy đủ Tên, Giá và Đường dẫn ảnh!', 'warning');
        return;
    }

    let promoInfo = PROMO_MAP[kmId] || { name: "Không khuyến mãi", discount: 0 };
    let discountPercent = promoInfo.discount;
    let finalPrice = discountPercent > 0 ? Math.round(price * (1 - discountPercent / 100)) : price;
    let oldPrice = discountPercent > 0 ? price : 0;

    let editedData = localStorage.getItem('basau_editedProducts');
    let edited = editedData ? JSON.parse(editedData) : {};

    edited[currentEditId] = {
        name: name,
        mathuonghieu: parseInt(brandId),
        brand: BRAND_MAP[brandId] || "Khác",
        maloai: parseInt(catId),
        category: CATEGORY_MAP[catId] || "Túi đeo chéo",
        dongia: price,
        soluong: stock,
        mota: mota,
        makm: kmId === "0" ? null : parseInt(kmId),
        promotionName: promoInfo.name,
        discountPercent: discountPercent,
        oldPrice: oldPrice,
        price: finalPrice,
        badge: badge || (discountPercent > 0 ? "Sale Off" : ""),
        img: img
    };

    localStorage.setItem('basau_editedProducts', JSON.stringify(edited));

    Swal.fire({
        icon: 'success',
        title: 'Cập nhật thành công!',
        text: `Đã lưu thay đổi cho "${name}".`,
        timer: 1400,
        showConfirmButton: false
    }).then(() => {
        location.reload();
    });
}

//loaddeletelist: hiển thị danh sách để xóa
function loadDeleteList() {
    let container = document.getElementById('deleteList');
    if (!container) return;

    let search = (document.getElementById('deleteSearch')?.value || '').toLowerCase();
    let products = getAllProducts().filter(p => {
        return (p.name || '').toLowerCase().includes(search) || (p.brand || '').toLowerCase().includes(search);
    });

    if (products.length === 0) {
        container.innerHTML = "<p class='text-muted text-center py-3'>Không tìm thấy túi xách nào.</p>";
        return;
    }

    let html = "";
    products.forEach(p => {
        html += `
            <div class="d-flex justify-content-between align-items-center p-3 mb-2 bg-white rounded-3 shadow-sm border">
                <div class="d-flex align-items-center gap-3">
                    <img src="${p.img}" class="rounded-2" style="width: 50px; height: 50px; object-fit: contain;">
                    <div>
                        <div class="fw-bold fs-6">${p.name}</div>
                        <div class="small text-muted">${p.brand} · ${p.category} | ${p.price.toLocaleString('vi-VN')}₫ | Kho: ${p.soluong || 0}</div>
                    </div>
                </div>
                <button class="btn btn-outline-danger btn-sm rounded-pill px-3 fw-bold" onclick="deleteProduct(${p.id})">Xóa</button>
            </div>
        `;
    });
    container.innerHTML = html;
}

//deleteproduct: xóa túi xách khỏi hệ thống
function deleteProduct(id) {
    Swal.fire({
        title: 'Xóa túi xách này?',
        text: 'Sản phẩm sẽ bị gỡ khỏi cửa hàng.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Đồng ý xóa',
        cancelButtonText: 'Hủy'
    }).then(result => {
        if (result.isConfirmed) {
            let deletedData = localStorage.getItem('basau_deletedIds');
            let deleted = deletedData ? JSON.parse(deletedData) : [];
            if (!deleted.includes(id)) deleted.push(id);
            localStorage.setItem('basau_deletedIds', JSON.stringify(deleted));

            Swal.fire({
                icon: 'success',
                title: 'Đã xóa!',
                timer: 1200,
                showConfirmButton: false
            }).then(() => {
                loadDeleteList();
            });
        }
    });
}

function filterAdminList(mode) {
    if (mode === 'edit') loadEditList();
    if (mode === 'delete') loadDeleteList();
}

//loadorderslist: quản lý đơn đặt hàng
function loadOrdersList() {
    let container = document.getElementById('orderListContainer');
    if (!container) return;

    let orders = JSON.parse(localStorage.getItem('basau_orders')) || [];
    if (orders.length === 0) {
        container.innerHTML = "<p class='text-muted text-center py-4'>Chưa có đơn đặt hàng nào trong hệ thống.</p>";
        return;
    }

    let html = "";
    orders.slice().reverse().forEach((o, index) => {
        let isPaid = (o.status && o.status.includes('Đã thanh toán'));
        let badgeColor = isPaid ? 'bg-success' : 'bg-warning text-dark';

        let itemsHtml = "";
        if (o.items && o.items.length > 0) {
            o.items.forEach(it => {
                itemsHtml += `<li>${it.tentui || 'Túi xách'} x <strong>${it.soluong}</strong> = ${(it.thanhtien || 0).toLocaleString('vi-VN')}₫</li>`;
            });
        }

        html += `
            <div class="card border mb-3 rounded-3 shadow-sm bg-white">
                <div class="card-header d-flex justify-content-between align-items-center bg-light">
                    <div>
                        <strong class="text-primary fs-6">Đơn: ${o.id}</strong>
                        <span class="text-muted small ms-2">${o.date}</span>
                    </div>
                    <span class="badge ${badgeColor} px-3 py-2 fw-bold">${o.status}</span>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-6">
                            <p class="mb-1"><strong>Khách hàng:</strong> ${o.customer || 'Khách vãng lai'}</p>
                            <p class="mb-1"><strong>SĐT:</strong> ${o.phone || 'Chưa có'}</p>
                            <p class="mb-1"><strong>Email:</strong> ${o.email || 'Chưa có'}</p>
                            <p class="mb-1"><strong>Địa chỉ nhận hàng:</strong> ${o.address || 'Tại cửa hàng'}</p>
                        </div>
                        <div class="col-md-6 border-start">
                            <p class="fw-bold mb-1">Chi tiết túi xách đặt mua:</p>
                            <ul class="small mb-2 ps-3">${itemsHtml || '<li>Đơn hàng túi xách thời trang</li>'}</ul>
                            <h5 class="fw-bold text-danger mt-2">Tổng tiền: ${o.total.toLocaleString('vi-VN')} ₫</h5>
                            <button class="btn btn-sm btn-outline-success rounded-pill fw-bold mt-2" onclick="toggleOrderStatus('${o.id}')">
                                Đổi trạng thái: ${isPaid ? 'Chờ thanh toán 🟡' : 'Đã thanh toán 🟢'}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    });
    container.innerHTML = html;
}

//toggleorderstatus: thay đổi trạng thái đơn hàng
function toggleOrderStatus(orderId) {
    let orders = JSON.parse(localStorage.getItem('basau_orders')) || [];
    let o = orders.find(item => item.id === orderId);
    if (o) {
        if (o.status && o.status.includes('Đã thanh toán')) {
            o.status = 'Chờ thanh toán 🟡';
        } else {
            o.status = 'Đã thanh toán 🟢';
        }
        localStorage.setItem('basau_orders', JSON.stringify(orders));
        loadOrdersList();
    }
}

//loadstatsdashboard: biểu đồ và số liệu thống kê
let _myChartBrands = null;
let _myChartCategories = null;

function loadStatsDashboard() {
    let products = getAllProducts();
    let orders = JSON.parse(localStorage.getItem('basau_orders')) || [];

    let totalRevenue = 0;
    orders.forEach(o => {
        if (o.status && (o.status.includes('Đã thanh toán') || o.status.includes('🟢'))) {
            totalRevenue += o.total;
        }
    });

    let totalStock = products.reduce((sum, p) => sum + (p.soluong || 0), 0);

    let revEl = document.getElementById('statsTotalRevenue');
    let ordEl = document.getElementById('statsTotalOrders');
    let prodEl = document.getElementById('statsTotalProducts');

    if (revEl) revEl.textContent = totalRevenue.toLocaleString('vi-VN') + ' ₫';
    if (ordEl) ordEl.textContent = orders.length + ' đơn';
    if (prodEl) prodEl.textContent = totalStock + ' cái (' + products.length + ' mẫu)';

    // Thống kê theo 5 thương hiệu túi xách
    let brandCounts = {};
    products.forEach(p => {
        let b = p.brand || 'Khác';
        brandCounts[b] = (brandCounts[b] || 0) + 1;
    });

    let ctxBrands = document.getElementById('chartBrands');
    if (ctxBrands) {
        if (_myChartBrands) _myChartBrands.destroy();
        _myChartBrands = new Chart(ctxBrands, {
            type: 'doughnut',
            data: {
                labels: Object.keys(brandCounts),
                datasets: [{
                    label: 'Số lượng mẫu túi',
                    data: Object.values(brandCounts),
                    backgroundColor: ['#222', '#d9534f', '#f0ad4e', '#5bc0de', '#5cb85c']
                }]
            }
        });
    }

    // Thống kê theo 6 loại túi xách
    let catCounts = {};
    products.forEach(p => {
        let c = p.category || 'Khác';
        catCounts[c] = (catCounts[c] || 0) + 1;
    });

    let ctxCat = document.getElementById('chartCategories');
    if (ctxCat) {
        if (_myChartCategories) _myChartCategories.destroy();
        _myChartCategories = new Chart(ctxCat, {
            type: 'bar',
            data: {
                labels: Object.keys(catCounts),
                datasets: [{
                    label: 'Số lượng mẫu theo loại',
                    data: Object.values(catCounts),
                    backgroundColor: '#111'
                }]
            },
            options: {
                scales: {
                    y: { beginAtZero: true }
                }
            }
        });
    }
}

//initadminpage: kiểm tra quyền Người bán và khởi tạo trang admin
function initAdminPage() {
    let label = document.getElementById('adminUserLabel');
    if (label) {
        let userData = localStorage.getItem('currentUser');
        let user = userData ? JSON.parse(userData) : null;

        if (!user || user.role !== 'Người bán') {
            Swal.fire({
                icon: 'warning',
                title: 'Yêu cầu quyền Người bán',
                text: 'Trang quản trị chỉ dành cho tài khoản có vai trò "Người bán". Vui lòng đăng nhập tài khoản Người bán (vd: admin / 123456).',
                confirmButtonColor: '#111'
            }).then(() => {
                window.location.href = "login.html";
            });
            return;
        }

        label.textContent = 'Quản trị viên: ' + (user.hoten || user.username) + ' · Vai trò: ' + user.role;
        patchProductsDatabase();
        switchAdminTab('add');
    }
}

initAdminPage();
patchProductsDatabase();