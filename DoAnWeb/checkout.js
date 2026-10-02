let dataTinhThanh = [];

//fetchprovinces: tải dữ liệu tỉnh thành từ api
function fetchProvinces() {
    fetch('https://provinces.open-api.vn/api/?depth=3')
        .then(response => response.json())
        .then(data => {
            dataTinhThanh = data;
            renderCities();
        })
        .catch(error => {
            console.warn("Lỗi tải Tỉnh thành từ API, nạp danh sách dự phòng:", error);
            dataTinhThanh = [
                {
                    code: 79, name: "TP. Hồ Chí Minh",
                    districts: [
                        { code: 760, name: "Quận 1", wards: [{ code: 26734, name: "Phường Bến Nghé" }, { code: 26740, name: "Phường Bến Thành" }] },
                        { code: 769, name: "Thành phố Thủ Đức", wards: [{ code: 26887, name: "Phường Linh Trung" }, { code: 26890, name: "Phường Tam Bình" }] }
                    ]
                },
                {
                    code: 1, name: "Hà Nội",
                    districts: [
                        { code: 1, name: "Quận Ba Đình", wards: [{ code: 1, name: "Phường Phúc Xá" }, { code: 4, name: "Phường Trúc Bạch" }] }
                    ]
                },
                {
                    code: 74, name: "Bình Dương",
                    districts: [
                        { code: 718, name: "Thành phố Thủ Dầu Một", wards: [{ code: 25774, name: "Phường Phú Hòa" }] }
                    ]
                }
            ];
            renderCities();
        });
}

//rendercities: hiển thị danh sách tỉnh thành
function renderCities() {
    let citySelect = document.getElementById('citySelect');
    if (!citySelect) return;
    let html = '<option value="" selected disabled>Chọn Tỉnh / Thành</option>';

    for (let i = 0; i < dataTinhThanh.length; i++) {
        html += `<option value="${dataTinhThanh[i].code}">${dataTinhThanh[i].name}</option>`;
    }
    citySelect.innerHTML = html;
}

//handlecitychange: xử lý khi chọn tỉnh thành
function handleCityChange(select) {
    let cityCode = select.value;
    let districtSelect = document.getElementById('districtSelect');
    let wardSelect = document.getElementById('wardSelect');

    let selectedCity = dataTinhThanh.find(tinh => tinh.code == cityCode);
    if (!selectedCity) return;

    let html = '<option value="" selected disabled>Chọn Quận / Huyện</option>';
    for (let i = 0; i < selectedCity.districts.length; i++) {
        html += `<option value="${selectedCity.districts[i].code}">${selectedCity.districts[i].name}</option>`;
    }

    districtSelect.innerHTML = html;
    districtSelect.disabled = false;

    wardSelect.innerHTML = '<option value="" selected disabled>Chọn Phường / Xã</option>';
    wardSelect.disabled = true;
}

//handledistrictchange: xử lý khi chọn quận huyện
function handleDistrictChange(select) {
    let districtCode = select.value;
    let cityCode = document.getElementById('citySelect').value;
    let wardSelect = document.getElementById('wardSelect');

    let selectedCity = dataTinhThanh.find(tinh => tinh.code == cityCode);
    if (!selectedCity) return;
    let selectedDistrict = selectedCity.districts.find(quan => quan.code == districtCode);
    if (!selectedDistrict) return;

    let html = '<option value="" selected disabled>Chọn Phường / Xã</option>';
    for (let i = 0; i < selectedDistrict.wards.length; i++) {
        html += `<option value="${selectedDistrict.wards[i].code}">${selectedDistrict.wards[i].name}</option>`;
    }

    wardSelect.innerHTML = html;
    wardSelect.disabled = false;
}

//autofillcustomerinfo: tự động điền thông tin khách hàng từ hồ sơ đăng nhập (khachhang)
function autofillCustomerInfo() {
    let userData = localStorage.getItem('currentUser');
    if (!userData) return;

    try {
        let user = JSON.parse(userData);
        let nameEl = document.getElementById('cusName');
        let emailEl = document.getElementById('cusEmail');
        let phoneEl = document.getElementById('cusPhone');
        let addrEl = document.getElementById('homeAddress');

        if (nameEl && (user.hoten || user.username)) nameEl.value = user.hoten || user.username;
        if (emailEl && user.email) emailEl.value = user.email;
        if (phoneEl && user.phone) phoneEl.value = user.phone;
        if (addrEl && user.address) addrEl.value = user.address;
    } catch(e) {
        console.error("Lỗi đọc dữ liệu người dùng:", e);
    }
}

//loadcheckoutsummary: tải tóm tắt đơn hàng túi xách
function loadCheckoutSummary() {
    let cart = getCart();

    let itemsContainer = document.getElementById('checkoutItems');
    let btnSubmit = document.getElementById('btnSubmitOrder');

    if (!itemsContainer) return;

    if (cart.length === 0) {
        itemsContainer.innerHTML = `
            <div class="text-center py-4">
                <p class='text-danger fw-bold'>Giỏ hàng của bạn đang trống!</p>
                <a href="catalog.html" class="btn btn-outline-dark btn-sm rounded-pill px-3">Quay lại chọn túi xách</a>
            </div>`;
        if (btnSubmit) btnSubmit.disabled = true;
        return;
    }

    let tongTien = 0;
    let html = "";

    for (let i = 0; i < cart.length; i++) {
        let p = cart[i];
        let thanhTien = p.price * p.quantity;
        tongTien += thanhTien;

        html += `
            <div class="d-flex align-items-center gap-3 mb-3 border-bottom pb-3">
                <img src="${p.img}" class="rounded-3 border" style="width: 60px; height: 60px; object-fit: contain;">
                <div style="flex: 1; min-width: 0;">
                    <div class="fw-bold text-truncate" style="font-size: 15px;">${p.name}</div>
                    <div class="text-secondary small">${p.brand || ''} · ${p.category || ''} | SL: <strong class="text-dark">${p.quantity}</strong></div>
                </div>
                <div class="fw-bold text-danger text-end" style="min-width: 100px;">${thanhTien.toLocaleString('vi-VN')} ₫</div>
            </div>
        `;
    }

    html += `
        <div class="d-flex justify-content-between align-items-baseline mt-4 pt-2 border-top">
            <span class="fw-bold fs-5">TỔNG THANH TOÁN:</span>
            <span class="fw-bold fs-4 text-danger">${tongTien.toLocaleString('vi-VN')} ₫</span>
        </div>
    `;

    itemsContainer.innerHTML = html;
}

//processcheckout: xử lý đặt hàng túi xách
function processCheckout(event) {
    if (event) event.preventDefault();
    let btnSubmit = document.getElementById('btnSubmitOrder');
    btnSubmit.innerText = "ĐANG XỬ LÝ ĐẶT HÀNG...";
    btnSubmit.disabled = true;

    try {
        let customerName = document.getElementById('cusName').value.trim();
        let customerEmail = document.getElementById('cusEmail').value.trim();
        let sdtKhach = document.getElementById('cusPhone').value.trim();

        let citySelect = document.getElementById('citySelect');
        let districtSelect = document.getElementById('districtSelect');
        let wardSelect = document.getElementById('wardSelect');
        let homeInput = document.getElementById('homeAddress').value.trim();

        let cityName = citySelect.options[citySelect.selectedIndex]?.text || '';
        let districtName = districtSelect.options[districtSelect.selectedIndex]?.text || '';
        let wardName = wardSelect.options[wardSelect.selectedIndex]?.text || '';

        let diaChiHoanChinh = `${homeInput}${wardName ? ', ' + wardName : ''}${districtName ? ', ' + districtName : ''}${cityName ? ', ' + cityName : ''}`;

        let cart = getCart();
        if (cart.length === 0) {
            Swal.fire('Giỏ hàng trống', 'Vui lòng chọn túi xách trước khi thanh toán!', 'warning');
            btnSubmit.innerText = "XÁC NHẬN ĐẶT HÀNG";
            btnSubmit.disabled = false;
            return false;
        }

        let tongTien = 0;
        let orderItems = [];
        for (let i = 0; i < cart.length; i++) {
            let itemTotal = cart[i].price * cart[i].quantity;
            tongTien += itemTotal;
            orderItems.push({
                matui: cart[i].id,
                tentui: cart[i].name,
                soluong: cart[i].quantity,
                dongia: cart[i].price,
                thanhtien: itemTotal
            });
        }

        let orderId = "DH" + new Date().getTime();
        let userData = localStorage.getItem('currentUser');
        let currentUser = userData ? JSON.parse(userData) : null;

        let newOrder = {
            id: orderId,
            makh: currentUser ? currentUser.matk : null,
            customer: customerName,
            email: customerEmail,
            phone: sdtKhach,
            address: diaChiHoanChinh,
            total: tongTien,
            date: new Date().toLocaleString('vi-VN'),
            status: 'Chờ thanh toán 🟡',
            items: orderItems
        };

        let orders = JSON.parse(localStorage.getItem('basau_orders')) || [];
        orders.push(newOrder);
        localStorage.setItem('basau_orders', JSON.stringify(orders));

        Swal.fire({
            title: 'Đặt hàng thành công',
            html: `Mã đơn hàng: <strong>${orderId}</strong><br>Tổng tiền: <strong class="text-danger">${tongTien.toLocaleString('vi-VN')} ₫</strong><br>Cảm ơn bạn đã tin tưởng mua sắm tại Basau Bags!`,
            icon: 'success',
            confirmButtonColor: '#111',
            confirmButtonText: 'Về trang chủ'
        }).then(() => {
            saveCart([]);
            window.location.href = "homePage.html";
        });

    } catch (err) {
        console.error(err);
        Swal.fire('Thiếu thông tin', 'Vui lòng điền đầy đủ thông tin giao hàng!', 'warning');
        btnSubmit.innerText = "XÁC NHẬN ĐẶT HÀNG";
        btnSubmit.disabled = false;
    }
    return false;
}

//initcheckout: khởi tạo trang thanh toán (Yêu cầu đăng nhập)
function initCheckout() {
    let userData = localStorage.getItem('currentUser');
    if (!userData || userData === "null") {
        Swal.fire({
            title: 'Yêu cầu đăng nhập',
            text: 'Vui lòng đăng nhập tài khoản để tiến hành thanh toán đơn hàng!',
            icon: 'warning',
            confirmButtonColor: '#111',
            confirmButtonText: 'Đăng nhập ngay'
        }).then(() => {
            window.location.href = "login.html";
        });
        return;
    }

    fetchProvinces();
    autofillCustomerInfo();
    setTimeout(loadCheckoutSummary, 100);
}

document.addEventListener('DOMContentLoaded', initCheckout);
