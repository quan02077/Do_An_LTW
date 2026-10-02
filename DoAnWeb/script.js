const emailValidation = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

//getusers: lấy dữ liệu từ localstorage
function getUsers() {
    let users = localStorage.getItem('users');
    if (users) {
        return JSON.parse(users);
    } else {
        return [];
    }
}

//saveusers: lưu dữ liệu vào localstorage
function saveUsers(users) {
    localStorage.setItem('users', JSON.stringify(users));
}

//showforgotform: hiển thị form quên mật khẩu
function showForgotForm() {
    document.getElementById('loginForm').classList.add('d-none');
    document.getElementById('registerForm').classList.add('d-none');
    document.getElementById('forgotForm').classList.remove('d-none');
    document.getElementById('pageTitle').innerText = 'Quên mật khẩu';
}

//showloginform: hiển thị form đăng nhập
function showLoginForm() {
    document.getElementById('registerForm').classList.add('d-none');
    document.getElementById('forgotForm').classList.add('d-none');
    document.getElementById('loginForm').classList.remove('d-none');
    document.getElementById('pageTitle').innerText = 'Đăng nhập';

    document.getElementById('forgotForm').reset();
    document.getElementById('registerForm').reset();
    document.getElementById('passBox').classList.add('d-none');
    document.getElementById('passBox').classList.remove('d-flex');
    document.getElementById('forgotBtn').innerText = 'Kiểm tra';
}

//showregisterform: hiển thị form đăng ký
function showRegisterForm() {
    document.getElementById('loginForm').classList.add('d-none');
    document.getElementById('forgotForm').classList.add('d-none');
    document.getElementById('registerForm').classList.remove('d-none');
    document.getElementById('pageTitle').innerText = 'Đăng ký';
}

//initmockdata: khởi tạo dữ liệu mẫu chuẩn theo quan_ly_tui_xach.sql
function initMockData() {
    let users = localStorage.getItem('users');
    let needsReset = false;
    if (users) {
        let parsed = JSON.parse(users);
        // Nếu có role cũ Nhân viên thì cập nhật lại
        if (parsed.some(u => u.role === 'Nhân viên')) needsReset = true;
    } else {
        needsReset = true;
    }

    if (needsReset) {
        let initialUsers = (typeof taiKhoanInitialList !== 'undefined') ? taiKhoanInitialList : [
            { matk: 1, username: "admin", email: "admin@tuixach.vn", password: "123456", role: "Người bán" },
            { matk: 2, username: "minhquan", email: "minhquan@gmail.com", password: "123456", role: "Khách hàng", hoten: "Nguyễn Nhật Minh Quân", phone: "0901234567", address: "TP. Hồ Chí Minh" },
            { matk: 3, username: "ngocanh", email: "ngocanh@gmail.com", password: "123456", role: "Khách hàng", hoten: "Trần Ngọc Anh", phone: "0912345678", address: "Bình Dương" },
            { matk: 4, username: "thanhhoa", email: "thanhhoa@gmail.com", password: "123456", role: "Khách hàng", hoten: "Lê Thanh Hòa", phone: "0923456789", address: "Đồng Nai" },
            { matk: 5, username: "quanghuy", email: "quanghuy@gmail.com", password: "123456", role: "Khách hàng", hoten: "Phạm Quang Huy", phone: "0934567890", address: "Long An" },
            { matk: 6, username: "thimai", email: "thimai@gmail.com", password: "123456", role: "Khách hàng", hoten: "Võ Thị Mai", phone: "0945678901", address: "TP. Hồ Chí Minh" }
        ];
        saveUsers(initialUsers);
    }
}
initMockData();

//login: xử lý đăng nhập
function login() {
    let usernameEl = document.getElementById('username');
    let passwordEl = document.getElementById('password');
    let roleEl = document.getElementById('roleSelection');
    if (!usernameEl || !passwordEl || !roleEl) return;

    let username = usernameEl.value.trim();
    let password = passwordEl.value;
    let role = roleEl.value;
    let users = getUsers();

    let user = null;
    for (let i = 0; i < users.length; i++) {
        let u = users[i];
        if ((u.username === username || u.email === username) && u.password === password && u.role === role) {
            user = u;
            break;
        }
    }

    if (user != null) {
        Swal.fire({
            title: 'Xin chào, ' + (user.hoten || user.username) + '!',
            text: 'Đăng nhập thành công với vai trò: ' + user.role,
            icon: 'success',
            timer: 1400,
            showConfirmButton: false
        }).then(() => {
            localStorage.setItem('currentUser', JSON.stringify(user));
            if (user.role === 'Người bán') {
                window.location.href = "admin.html";
            } else {
                window.location.href = "homePage.html";
            }
        });
    } else {
        Swal.fire('Thất bại', 'Tên đăng nhập, mật khẩu hoặc vai trò không đúng.', 'error');
        passwordEl.value = '';
    }
}

//forgotpassword: xử lý quên mật khẩu
function forgotPassword() {
    let username = document.getElementById('forgotUsername').value.trim();
    let newPass = document.getElementById('newPassword').value;
    let confirmPass = document.getElementById('confirmPassword').value;
    let passBox = document.getElementById('passBox');

    let users = getUsers();
    let index = -1;
    for (let i = 0; i < users.length; i++) {
        if (users[i].username === username || users[i].email === username) {
            index = i;
            break;
        }
    }

    if (index === -1) {
        Swal.fire('Lỗi', 'Tài khoản không tồn tại. Vui lòng kiểm tra lại!', 'error');
        return;
    }

    if (passBox.classList.contains('d-none')) {
        Swal.fire({
            title: 'Xác thực thành công',
            text: 'Tài khoản hợp lệ. Vui lòng nhập mật khẩu mới bên dưới.',
            icon: 'success',
            confirmButtonColor: '#111'
        });
        passBox.classList.remove('d-none');
        passBox.classList.add('d-flex');
        document.getElementById('forgotBtn').innerText = 'Xác nhận thay đổi';
        return;
    }

    if (newPass === "" || confirmPass === "") {
        Swal.fire('Thông báo', 'Vui lòng nhập đầy đủ mật khẩu mới!', 'warning');
        return;
    }

    if (newPass === confirmPass) {
        users[index].password = newPass;
        saveUsers(users);
        Swal.fire({
            title: 'Thành công',
            text: 'Mật khẩu của bạn đã được cập nhật!',
            icon: 'success',
            confirmButtonColor: '#111'
        }).then(() => {
            showLoginForm();
        });
    } else {
        Swal.fire('Lỗi', 'Mật khẩu xác nhận không khớp. Thử lại nhé!', 'error');
    }
}

//register: xử lý đăng ký tài khoản
function register() {
    let email = document.getElementById('registerEmail').value.trim();
    let username = document.getElementById('registerUsername').value.trim();
    let password = document.getElementById('registerPassword').value;
    let confirmPassword = document.getElementById('registerConfirmPassword').value;
    let role = document.getElementById('registerRoleSelection').value;

    let users = getUsers();

    if (!emailValidation.test(email)) {
        Swal.fire('Định dạng sai', 'Vui lòng nhập đúng địa chỉ email!', 'warning');
        return;
    }

    if (password !== confirmPassword) {
        Swal.fire('Lỗi', 'Mật khẩu xác nhận không khớp nhau!', 'error');
        return;
    }

    let isDuplicate = users.some(u => u.username === username || u.email === email);
    if (isDuplicate) {
        Swal.fire('Đã tồn tại', 'Email hoặc tên đăng nhập này đã có người dùng rồi!', 'error');
        return;
    }

    let newUser = {
        matk: users.length + 1,
        email: email,
        username: username,
        password: password,
        role: role,
        hoten: username,
        phone: "",
        address: ""
    };
    users.push(newUser);
    saveUsers(users);

    Swal.fire({
        title: 'Chúc mừng!',
        text: 'Bạn đã đăng ký tài khoản thành công.',
        icon: 'success',
        confirmButtonColor: '#111'
    }).then(() => {
        showLoginForm();
    });
}

//togglepassword: ẩn hiện mật khẩu trên các ô input
function togglePassword(checkbox) {
    let form = checkbox.closest("form");
    if (!form) return;
    let passwords = form.querySelectorAll(".password-field");

    for (let j = 0; j < passwords.length; j++) {
        passwords[j].type = checkbox.checked ? "text" : "password";
    }
}

//initusersession: hiển thị tài khoản khi đăng nhập thành công
function initUserSession() {
    let currentUserData = localStorage.getItem('currentUser');
    if (currentUserData) {
        let currentUser = JSON.parse(currentUserData);
        let userAccountText = document.getElementById('userAccountText');
        let userIconImg = document.getElementById('userIconImg');
        let userAccountLink = document.getElementById('userAccountLink');

        if (userAccountText && userIconImg && userAccountLink) {
            userAccountText.innerText = currentUser.hoten || currentUser.username;
            userIconImg.src = "hinhAnh/userHomeIcon.png";
            userAccountLink.href = "#";
        }
    }
}
initUserSession();

//renderhotproducts: tải động 8 sản phẩm túi xách nổi bật trên trang chủ
function renderHotProducts() {
    let container = document.getElementById('hotProductsContainer');
    if (!container) return;

    let allProds = (typeof getAllProducts === 'function') ? getAllProducts() : (typeof productsDatabase !== 'undefined' ? productsDatabase : []);
    
    // Ưu tiên hiển thị túi có khuyến mãi hoặc badge
    let hotProds = allProds.filter(p => p.badge && p.badge !== "");
    if (hotProds.length < 8) {
        let normalProds = allProds.filter(p => !p.badge || p.badge === "");
        for (let i = 0; i < normalProds.length; i++) {
            if (hotProds.length >= 8) break;
            hotProds.push(normalProds[i]);
        }
    }

    let displayProds = hotProds.slice(0, 8);
    let htmlContent = "";
    displayProds.forEach(p => {
        let priceSale = p.price.toLocaleString('vi-VN') + ' ₫';
        let oldPriceHtml = (p.oldPrice && p.oldPrice > p.price) 
            ? `<span class="text-muted text-decoration-line-through small me-2">${p.oldPrice.toLocaleString('vi-VN')} ₫</span>`
            : '';
        let badgeHtml = p.discountPercent > 0 
            ? `<span class="badge bg-danger position-absolute top-0 end-0 m-2">-${p.discountPercent}%</span>`
            : (p.badge ? `<span class="badge bg-dark position-absolute top-0 end-0 m-2">${p.badge}</span>` : '');

        htmlContent += `
            <div class="col-6 col-md-3">
                <a href="productDetail.html?id=${p.id}" class="text-decoration-none text-dark">
                    <div class="product-card border p-3 rounded shadow-sm text-center position-relative h-100 d-flex flex-column bg-white">
                        ${badgeHtml}
                        <div style="height: 180px; display: flex; align-items: center; justify-content: center; overflow: hidden; margin-bottom: 12px;">
                            <img src="${p.img}" class="img-fluid" style="max-height: 100%; object-fit: contain;">
                        </div>
                        <span class="text-secondary small fw-bold text-uppercase">${p.brand} · ${p.category}</span>
                        <h5 class="fw-bold my-2" style="font-size: 15px; min-height: 44px; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; text-overflow: ellipsis; line-height: 1.45;">${p.name}</h5>
                        <div class="mt-auto">
                            <p class="mb-2">
                                ${oldPriceHtml}
                                <span class="text-danger fw-bold fs-5">${priceSale}</span>
                            </p>
                            <button class="btn btn-dark w-100 rounded-pill fw-bold py-2">Xem chi tiết</button>
                        </div>
                    </div>
                </a>
            </div>
        `;
    });
    container.innerHTML = htmlContent;
}
renderHotProducts();

//handleenterkey: nhấn enter để kích hoạt form
function handleEnterKey(event) {
    if (event.key === 'Enter') {
        let loginForm = document.getElementById('loginForm');
        let registerForm = document.getElementById('registerForm');
        let forgotForm = document.getElementById('forgotForm');

        if (loginForm && !loginForm.classList.contains('d-none')) {
            event.preventDefault();
            document.getElementById('loginBtn').click();
        } else if (registerForm && !registerForm.classList.contains('d-none')) {
            event.preventDefault();
            document.getElementById('registerBtn').click();
        } else if (forgotForm && !forgotForm.classList.contains('d-none')) {
            event.preventDefault();
            document.getElementById('forgotBtn').click();
        }
    }
}
document.addEventListener('keydown', handleEnterKey);

//clearform: xóa dữ liệu cũ trong form khi tải trang
function clearForm() {
    let loginForm = document.getElementById('loginForm');
    let registerForm = document.getElementById('registerForm');
    let forgotForm = document.getElementById('forgotForm');

    if (loginForm) loginForm.reset();
    if (registerForm) registerForm.reset();
    if (forgotForm) forgotForm.reset();
}
window.onload = clearForm;

//togglemenu: đóng mở menu 3 gạch
function toggleMenu() {
    let menu = document.querySelector('.menu');
    if (menu) {
        menu.classList.toggle('active');
    }
}

//checkpaymentsuccess: hiện thông báo khi thanh toán thành công
function checkPaymentSuccess() {
    let urlParams = new URLSearchParams(window.location.search);
    let isPaymentSuccess = urlParams.get('payment_success');
    let orderId = urlParams.get('order_id');

    if (isPaymentSuccess === 'true' && orderId) {
        let orders = JSON.parse(localStorage.getItem('basau_orders')) || [];
        let orderFound = false;

        for (let i = 0; i < orders.length; i++) {
            if (orders[i].id === orderId) {
                if (orders[i].status !== 'Đã thanh toán 🟢') {
                    orders[i].status = 'Đã thanh toán 🟢';
                    orderFound = true;
                }
                break;
            }
        }

        if (orderFound) {
            localStorage.setItem('basau_orders', JSON.stringify(orders));
            Swal.fire({
                title: 'Thanh toán thành công!',
                text: 'Tuyệt vời! Đơn hàng ' + orderId + ' của bạn đã được xác nhận.',
                icon: 'success',
                confirmButtonColor: '#27ae60'
            }).then(() => {
                window.history.replaceState(null, null, window.location.pathname);
            });
        }
    }
}
checkPaymentSuccess();

//timer: đếm ngược ở homepage
const targetDate = new Date("Sep 30 2026 23:59:59").getTime();
function timer() {
    let Days = document.getElementById('days');
    let Hours = document.getElementById('hours');
    let Minutes = document.getElementById('minutes');
    let Seconds = document.getElementById('seconds');

    if (!Days || !Hours || !Minutes || !Seconds) return;

    const now = new Date().getTime();
    const distance = targetDate - now;

    const days = Math.floor(distance / 1000 / 60 / 60 / 24);
    const hours = Math.floor(distance / 1000 / 60 / 60) % 24;
    const minutes = Math.floor(distance / 1000 / 60) % 60;
    const seconds = Math.floor(distance / 1000) % 60;

    Days.innerHTML = Math.max(0, days) + ' <small class="d-block fs-6 fw-normal">Ngày</small>';
    Hours.innerHTML = Math.max(0, hours) + ' <small class="d-block fs-6 fw-normal">Giờ</small>';
    Minutes.innerHTML = Math.max(0, minutes) + ' <small class="d-block fs-6 fw-normal">Phút</small>';
    Seconds.innerHTML = Math.max(0, seconds) + ' <small class="d-block fs-6 fw-normal">Giây</small>';
}
setInterval(timer, 1000);

//handlegeneralsearch: xử lý tìm kiếm chung chuyển hướng đến trang danh mục
function handleGeneralSearch(e, input) {
    if (e.key === 'Enter') {
        e.preventDefault();
        let keyword = input.value.trim();
        if (keyword !== "") {
            window.location.href = "catalog.html?search=" + encodeURIComponent(keyword);
        }
    }
}
