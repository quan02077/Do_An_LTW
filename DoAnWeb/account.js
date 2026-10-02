//injectaccounthtml: tạo giao diện popup tài khoản người dùng
function injectAccountHTML() {
    if (document.getElementById('accPanel')) return;

    let overlay = document.createElement('div');
    overlay.id = 'accOverlay';
    overlay.onclick = closeAccountPanel;

    let panel = document.createElement('div');
    panel.id = 'accPanel';

    panel.innerHTML = `
        <div class="panel-header">
            <div>
                <h5 class="panel-title">👤 HỒ SƠ TÀI KHOẢN</h5>
                <p id="accHeaderRole" class="panel-subtitle green">Khách hàng</p>
            </div>
            <button onclick="closeAccountPanel()" class="panel-close-btn">✕</button>
        </div>
        <div class="panel-body">
            <h6 class="acc-section-title">THÔNG TIN KHÁCH HÀNG</h6>
            <div class="mb-3">
                <label class="form-label small text-secondary fw-bold">Họ và tên</label>
                <input type="text" id="accEditName" class="form-control bg-light" placeholder="Nhập họ và tên">
            </div>
            <div class="mb-3">
                <label class="form-label small text-secondary fw-bold">Địa chỉ Email</label>
                <input type="email" id="accEditEmail" class="form-control bg-light" placeholder="email@gmail.com">
            </div>
            <div class="mb-3">
                <label class="form-label small text-secondary fw-bold">Số điện thoại</label>
                <input type="tel" id="accEditPhone" class="form-control bg-light" placeholder="0901234567">
            </div>
            <div class="mb-3">
                <label class="form-label small text-secondary fw-bold">Địa chỉ giao hàng mặc định</label>
                <input type="text" id="accEditAddress" class="form-control bg-light" placeholder="Số nhà, đường, tỉnh/thành">
            </div>
            <div class="mb-4">
                <label class="form-label small text-secondary fw-bold">Vai trò hệ thống</label>
                <input type="text" id="accEditRole" class="form-control text-muted bg-light" disabled>
            </div>
            <button onclick="saveAccountInfo()" class="btn btn-dark w-100 fw-bold mb-4 rounded-pill">LƯU THÔNG TIN MỚI</button>

            <div id="accAdminLinkWrap" class="mb-4 d-none">
                <a href="admin.html" class="btn btn-warning w-100 fw-bold rounded-pill text-dark">⚙️ TRANG QUẢN TRỊ NGƯỜI BÁN</a>
            </div>

            <h6 class="acc-section-title">BẢO MẬT TÀI KHOẢN</h6>
            <div class="mb-3">
                <label class="form-label small text-secondary fw-bold">Mật khẩu mới</label>
                <input type="password" id="accNewPass" class="form-control" placeholder="Nhập mật khẩu muốn đổi">
            </div>
            <button onclick="changeAccountPassword()" class="btn btn-outline-dark w-100 fw-bold mb-4 rounded-pill">CẬP NHẬT MẬT KHẨU</button>
            <button onclick="viewMyOrders()" class="btn btn-outline-primary w-100 fw-bold mb-3 rounded-pill">📦 LỊCH SỬ ĐẶT HÀNG</button>
            <button onclick="logoutUser()" class="btn btn-danger w-100 fw-bold py-2 mt-2 rounded-pill">ĐĂNG XUẤT</button>
        </div>
    `;
    document.body.appendChild(overlay);
    document.body.appendChild(panel);
}

//openaccountpanel: mở popup tài khoản
function openAccountPanel() {
    let userData = localStorage.getItem('currentUser');
    if (!userData || userData === "null") {
        window.location.href = "login.html";
        return;
    }

    let user = JSON.parse(userData);

    document.getElementById('accEditName').value = user.hoten || user.username || "";
    document.getElementById('accEditEmail').value = user.email || "";
    document.getElementById('accEditPhone').value = user.phone || "";
    document.getElementById('accEditAddress').value = user.address || "";
    document.getElementById('accEditRole').value = user.role || "Khách hàng";
    document.getElementById('accHeaderRole').innerText = user.role || "Khách hàng";
    document.getElementById('accNewPass').value = "";

    let adminLink = document.getElementById('accAdminLinkWrap');
    if (adminLink) {
        if (user.role === 'Người bán') {
            adminLink.classList.remove('d-none');
        } else {
            adminLink.classList.add('d-none');
        }
    }

    document.getElementById('accPanel').classList.add('open');
    document.getElementById('accOverlay').classList.add('open');
    document.body.style.overflow = 'hidden';
}

//closeaccountpanel: đóng popup tài khoản
function closeAccountPanel() {
    let panel = document.getElementById('accPanel');
    let overlay = document.getElementById('accOverlay');
    if (panel) panel.classList.remove('open');
    if (overlay) overlay.classList.remove('open');
    document.body.style.overflow = '';
}

//saveaccountinfo: lưu thông tin khách hàng mới
function saveAccountInfo() {
    let newName = document.getElementById('accEditName').value.trim();
    let newEmail = document.getElementById('accEditEmail').value.trim();
    let newPhone = document.getElementById('accEditPhone').value.trim();
    let newAddress = document.getElementById('accEditAddress').value.trim();

    if (newName === "" || newEmail === "") {
        Swal.fire('Khoan đã', 'Vui lòng không để trống Họ tên và Email!', 'warning');
        return;
    }

    let user = JSON.parse(localStorage.getItem('currentUser'));
    user.hoten = newName;
    user.email = newEmail;
    user.phone = newPhone;
    user.address = newAddress;
    localStorage.setItem('currentUser', JSON.stringify(user));

    // Cập nhật vào danh sách users chung
    let allUsers = JSON.parse(localStorage.getItem('users')) || [];
    let idx = allUsers.findIndex(u => u.username === user.username);
    if (idx !== -1) {
        allUsers[idx].hoten = newName;
        allUsers[idx].email = newEmail;
        allUsers[idx].phone = newPhone;
        allUsers[idx].address = newAddress;
        localStorage.setItem('users', JSON.stringify(allUsers));
    }

    let accText = document.getElementById('userAccountText');
    if (accText) accText.innerText = newName;

    Swal.fire('Tuyệt vời', 'Đã cập nhật thông tin thành công!', 'success');
}

//changeaccountpassword: đổi mật khẩu tài khoản
function changeAccountPassword() {
    let newPass = document.getElementById('accNewPass').value.trim();
    if (newPass.length < 3) {
        Swal.fire('Lưu ý', 'Mật khẩu mới phải từ 3 ký tự trở lên nha.', 'info');
        return;
    }
    let user = JSON.parse(localStorage.getItem('currentUser'));
    user.password = newPass;
    localStorage.setItem('currentUser', JSON.stringify(user));

    let allUsers = JSON.parse(localStorage.getItem('users')) || [];
    let idx = allUsers.findIndex(u => u.username === user.username);
    if (idx !== -1) {
        allUsers[idx].password = newPass;
        localStorage.setItem('users', JSON.stringify(allUsers));
    }

    document.getElementById('accNewPass').value = "";
    Swal.fire('Thành công', 'Đã đổi mật khẩu mới!', 'success');
}

//logoutuser: đăng xuất tài khoản
function logoutUser() {
    Swal.fire({
        title: 'Đăng xuất?',
        text: "Bạn có chắc chắn muốn đăng xuất khỏi hệ thống?",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#111',
        confirmButtonText: 'Đăng xuất',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            localStorage.removeItem('currentUser');
            window.location.href = "homePage.html";
        }
    });
}

//handleaccountlinkclick: xử lý click vào link tài khoản
function handleAccountLinkClick(e) {
    let checkData = localStorage.getItem('currentUser');
    if (checkData && checkData !== "null" && checkData !== "undefined") {
        e.preventDefault();
        openAccountPanel();
    }
}

//initaccount: khởi tạo và nạp thông tin user
function initAccount() {
    injectAccountHTML();

    let accText = document.getElementById('userAccountText');
    let userData = localStorage.getItem('currentUser');

    if (userData && userData !== "null" && userData !== "undefined") {
        try {
            let user = JSON.parse(userData);
            if (accText && (user.hoten || user.username)) {
                accText.innerText = user.hoten || user.username;
            }
        } catch(e) {}
    }
}
initAccount();