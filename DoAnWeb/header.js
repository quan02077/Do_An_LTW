const headerHTML = `
        <div class="rowHeader bg-light py-1 border-bottom">
            <div class="container-fluid px-4 d-flex justify-content-end align-items-center gap-4">
                <div class="tienich" id="adminMenuWrap">
                    <a href="admin.html" class="text-secondary text-decoration-none" id="QL_btn">Quản lý sản phẩm</a>
                </div>
                <div class="tienich">
                    <a href="#" class="text-secondary text-decoration-none" onclick="handleCartLinkClick(event)"><span id="cartCountText">Giỏ hàng (0)</span></a>
                </div>
                <div class="tienich">
                    <a href="#" class="text-secondary text-decoration-none" onclick="handleFavLinkClick(event)">Yêu thích</a>
                </div>
                <div class="tienich">
                    <a href="login.html" class="text-secondary text-decoration-none" id="userAccountLink"
                        onclick="handleAccountLinkClick(event)">
                        <span id="userAccountText">Đăng nhập</span>
                    </a>
                </div>
            </div>
        </div>
        <div class="container-fluid rowMenu bg-white sticky-top shadow-sm py-3">
            <div class="container d-flex align-items-center justify-content-between">

                <div class="logo d-flex justify-content-start align-items-center" style="flex: 1;">
                    <a href="homePage.html" class="text-decoration-none text-dark d-flex align-items-center gap-2">
                        <span class="oswald-font fw-bold fs-2" style="letter-spacing: 2px;">BASAU<span class="text-danger">.BAGS</span></span>
                    </a>
                </div>

                <div class="menu-toggle d-lg-none" style="cursor: pointer;" onclick="toggleMenu()">
                    <span class="bar"></span>
                    <span class="bar"></span>
                    <span class="bar"></span>
                </div>
                <nav class="menu d-none d-lg-flex justify-content-center align-items-center" id="nav-menu" style="flex: 2;">

                    <a href="catalog.html" class="text-dark text-decoration-none menu-link">TẤT CẢ</a>

                    <div class="dropdown">
                        <a href="#" class="text-dark text-decoration-none menu-link">LOẠI TÚI <small>▾</small></a>
                        <div class="mega-menu p-4">
                            <div class="container">
                                <div class="row text-start g-3">
                                    <div class="col-4">
                                        <h6 class="fw-bold text-white border-bottom border-secondary pb-2">THÔNG DỤNG</h6>
                                        <a href="catalog.html?category=Balo" class="d-block text-white-50 text-decoration-none py-1 hover-white">Balo thời trang</a>
                                        <a href="catalog.html?category=Túi đeo chéo" class="d-block text-white-50 text-decoration-none py-1 hover-white">Túi đeo chéo</a>
                                    </div>
                                    <div class="col-4">
                                        <h6 class="fw-bold text-white border-bottom border-secondary pb-2">THANH LỊCH</h6>
                                        <a href="catalog.html?category=Túi đeo vai" class="d-block text-white-50 text-decoration-none py-1 hover-white">Túi đeo vai</a>
                                        <a href="catalog.html?category=Túi tote" class="d-block text-white-50 text-decoration-none py-1 hover-white">Túi tote</a>
                                    </div>
                                    <div class="col-4">
                                        <h6 class="fw-bold text-white border-bottom border-secondary pb-2">SỰ KIỆN</h6>
                                        <a href="catalog.html?category=Clutch" class="d-block text-white-50 text-decoration-none py-1 hover-white">Clutch dự tiệc</a>
                                        <a href="catalog.html?category=Túi dự tiệc" class="d-block text-white-50 text-decoration-none py-1 hover-white">Túi dự tiệc</a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="dropdown">
                        <a href="#" class="text-dark text-decoration-none menu-link">THƯƠNG HIỆU <small>▾</small></a>
                        <div class="mega-menu p-4">
                            <div class="container">
                                <div class="row text-start g-3">
                                    <div class="col-4">
                                        <h6 class="fw-bold text-white border-bottom border-secondary pb-2">QUỐC TẾ</h6>
                                        <a href="catalog.html?brand=CHARLES%20%26%20KEITH" class="d-block text-white-50 text-decoration-none py-1 hover-white">CHARLES & KEITH</a>
                                        <a href="catalog.html?brand=Pedro" class="d-block text-white-50 text-decoration-none py-1 hover-white">Pedro</a>
                                    </div>
                                    <div class="col-4">
                                        <h6 class="fw-bold text-white border-bottom border-secondary pb-2">NỘI ĐỊA CAO CẤP</h6>
                                        <a href="catalog.html?brand=ELLY" class="d-block text-white-50 text-decoration-none py-1 hover-white">ELLY</a>
                                    </div>
                                    <div class="col-4">
                                        <h6 class="fw-bold text-white border-bottom border-secondary pb-2">TRẺ TRUNG & TIỆN ÍCH</h6>
                                        <a href="catalog.html?brand=Natoli" class="d-block text-white-50 text-decoration-none py-1 hover-white">Natoli</a>
                                        <a href="catalog.html?brand=Jamlos" class="d-block text-white-50 text-decoration-none py-1 hover-white">Jamlos</a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <a href="flashSale.html" class="text-danger text-decoration-none menu-link fw-bold">KHUYẾN MÃI</a>
                </nav>

                <div class="searchBar d-none d-lg-flex" style="flex: 1; justify-content: flex-end;">
                    <input type="text" class="form-control rounded-pill custom-search"
                        onkeyup="if(window.handleCatalogSearch) { handleCatalogSearch(event, this) } else { handleGeneralSearch(event, this) }" onsearch="if(window.handleCatalogSearchClear) handleCatalogSearchClear(this)" placeholder="Tìm kiếm túi xách, balo...">
                </div>

            </div>
        </div>
`;

function injectHeader() {
    const headerContainer = document.getElementById('main-header');
    if (headerContainer) {
        headerContainer.innerHTML = headerHTML;
    }
    checkAdminRoleHeader();
}

function checkAdminRoleHeader() {
    let userData = localStorage.getItem('currentUser');
    let qlBtn = document.getElementById('adminMenuWrap');
    if (qlBtn && userData) {
        try {
            let u = JSON.parse(userData);
            if (u.role !== 'Người bán') {
                // Keep visible or highlight seller mode
            }
        } catch(e) {}
    }
}

injectHeader();
