// ================================================================
// QUẢN LÝ DANH MỤC TÚI XÁCH & BALO (catalog.js)
// ================================================================

// Biến trạng thái toàn cục
let currentBrand = "all";
let currentSearch = "";
let displayedProducts = [];
let currentPage = 1;
let itemsPerPage = 6;
let isSearchMode = false;

// Hàm bỏ dấu tiếng Việt để tìm kiếm thông minh hơn
function removeVietnameseTones(str) {
    if (!str) return '';
    str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
    str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
    str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
    str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
    str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
    str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
    str = str.replace(/đ/g, "d");
    str = str.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ/g, "A");
    str = str.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
    str = str.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
    str = str.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ/g, "O");
    str = str.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
    str = str.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
    str = str.replace(/Đ/g, "D");
    return str.toLowerCase().trim();
}

//selectbrand: chọn thương hiệu sản phẩm từ tab
function selectBrand(element) {
    let tabs = document.querySelectorAll('.brand-tab');
    tabs.forEach(t => t.classList.remove('active'));
    element.classList.add('active');
    currentBrand = element.getAttribute('data-brand') || "all";

    if (!isSearchMode) {
        let url = currentBrand === 'all' ? 'catalog.html' : '?brand=' + encodeURIComponent(currentBrand);
        window.history.replaceState(null, null, url);
    }

    applyFilters();
}

//applyfilters: áp dụng các bộ lọc túi xách
function applyFilters() {
    let checkboxes = document.querySelectorAll('.filter-check input[type="checkbox"]');
    let filterCategories = [], filterBadges = [], filterPrices = [];

    for (let i = 0; i < checkboxes.length; i++) {
        if (checkboxes[i].checked) {
            let val = checkboxes[i].value;
            if (["Balo", "Túi đeo chéo", "Túi tote", "Túi dự tiệc", "Túi đeo vai", "Clutch"].includes(val)) {
                filterCategories.push(val);
            } else if (["under700k", "700k-1.2m", "over1.2m"].includes(val)) {
                filterPrices.push(val);
            } else {
                filterBadges.push(val);
            }
        }
    }

    // Cập nhật tiêu đề trang & thanh breadcrumb
    let pageTitle = document.getElementById('pageTitle');
    let breadcrumbCurrent = document.getElementById('breadcrumbCurrent');

    let titleText = "Tất cả túi xách";
    if (isSearchMode && currentSearch) {
        titleText = `Tìm kiếm: "${currentSearch}"`;
    } else if (currentBrand !== 'all' && filterCategories.length === 1) {
        titleText = `${currentBrand} - ${filterCategories[0]}`;
    } else if (currentBrand !== 'all') {
        titleText = currentBrand;
    } else if (filterCategories.length === 1) {
        titleText = filterCategories[0];
    } else if (filterBadges.length === 1) {
        titleText = filterBadges[0];
    }

    if (pageTitle) pageTitle.innerText = titleText;
    if (breadcrumbCurrent) breadcrumbCurrent.innerText = titleText;

    displayedProducts = [];
    let allProducts = (typeof getAllProducts === 'function') ? getAllProducts() : (typeof productsDatabase !== 'undefined' ? productsDatabase : []);

    for (let i = 0; i < allProducts.length; i++) {
        let product = allProducts[i];
        let isValid = true;

        // 1. Lọc theo thương hiệu
        if (currentBrand !== "all" && product.brand !== currentBrand) {
            isValid = false;
        }

        // 2. Lọc theo loại túi
        if (filterCategories.length > 0 && !filterCategories.includes(product.category)) {
            isValid = false;
        }

        // 3. Lọc theo khuyến mãi & badge
        if (filterBadges.length > 0) {
            let hasBadgeMatch = false;
            for (let b = 0; b < filterBadges.length; b++) {
                let badgeVal = filterBadges[b];
                if (badgeVal === "Sale Off" && (product.badge === "Sale Off" || product.discountPercent > 0)) hasBadgeMatch = true;
                if (badgeVal === "Best Seller" && product.badge === "Best Seller") hasBadgeMatch = true;
                if (badgeVal === "New Arrival" && product.badge === "New Arrival") hasBadgeMatch = true;
                if (product.promotionName && product.promotionName.toLowerCase().includes(badgeVal.toLowerCase())) hasBadgeMatch = true;
            }
            if (!hasBadgeMatch) isValid = false;
        }

        // 4. Lọc theo khoảng giá
        if (filterPrices.length > 0) {
            let matchPrice = false;
            for (let j = 0; j < filterPrices.length; j++) {
                let p = filterPrices[j];
                if (p === "under700k" && product.price < 700000) matchPrice = true;
                if (p === "700k-1.2m" && product.price >= 700000 && product.price <= 1200000) matchPrice = true;
                if (p === "over1.2m" && product.price > 1200000) matchPrice = true;
            }
            if (!matchPrice) isValid = false;
        }

        // 5. Lọc theo từ khóa tìm kiếm (hỗ trợ cả có dấu và không dấu)
        if (currentSearch !== "") {
            let sNorm = removeVietnameseTones(currentSearch);
            let nameNorm = removeVietnameseTones(product.name || "");
            let brandNorm = removeVietnameseTones(product.brand || "");
            let categoryNorm = removeVietnameseTones(product.category || "");
            let motaNorm = removeVietnameseTones(product.mota || "");

            if (!nameNorm.includes(sNorm) && !brandNorm.includes(sNorm) && !categoryNorm.includes(sNorm) && !motaNorm.includes(sNorm)) {
                isValid = false;
            }
        }

        if (isValid) {
            displayedProducts.push(product);
        }
    }

    applySort();
}

//applysort: sắp xếp danh sách sản phẩm
function applySort() {
    let sortSelect = document.getElementById('sortSelect');
    let sortVal = sortSelect ? sortSelect.value : 'featured';

    if (sortVal === 'price-asc') {
        displayedProducts.sort((a, b) => a.price - b.price);
    } else if (sortVal === 'price-desc') {
        displayedProducts.sort((a, b) => b.price - a.price);
    } else if (sortVal === 'stock-desc') {
        displayedProducts.sort((a, b) => (b.soluong || 0) - (a.soluong || 0));
    }

    currentPage = 1;
    renderCatalog();
}

//rendercatalog: hiển thị sản phẩm ra giao diện
function renderCatalog() {
    let grid = document.getElementById('productGrid');
    let countEl = document.getElementById('resultCount');
    let noResults = document.getElementById('noResults');
    if (!grid) return;

    if (countEl) countEl.innerText = `(${displayedProducts.length} sản phẩm)`;

    if (displayedProducts.length === 0) {
        grid.innerHTML = "";
        if (noResults) noResults.classList.remove('d-none');
        renderPagination();
        return;
    }

    if (noResults) noResults.classList.add('d-none');

    let start = (currentPage - 1) * itemsPerPage;
    let end = start + itemsPerPage;
    let pageItems = displayedProducts.slice(start, end);

    let favList = (typeof getFavList === 'function') ? getFavList() : [];

    let html = "";
    pageItems.forEach(p => {
        let priceSale = p.price.toLocaleString('vi-VN') + ' ₫';
        let oldPriceHtml = (p.oldPrice && p.oldPrice > p.price)
            ? `<span class="text-muted text-decoration-line-through small me-2">${p.oldPrice.toLocaleString('vi-VN')} ₫</span>`
            : '';
        let badgeHtml = "";
        if (p.discountPercent > 0) {
            badgeHtml = `<span class="product-discount-badge">-${p.discountPercent}%</span>`;
        } else if (p.badge) {
            badgeHtml = `<span class="product-status-badge">${p.badge}</span>`;
        }

        let statusTagHtml = "";
        if (p.badge === "Best Seller") {
            statusTagHtml = `<span class="badge bg-light text-dark border small fw-bold me-1">Bán chạy</span>`;
        } else if (p.badge === "New Arrival") {
            statusTagHtml = `<span class="badge bg-light text-dark border small fw-bold me-1">Mới về</span>`;
        }

        let stockText = (p.soluong !== undefined && p.soluong > 0) 
            ? `<span class="badge bg-light text-success border">Còn ${p.soluong}</span>` 
            : `<span class="badge bg-secondary">Hết hàng</span>`;

        let isFav = favList.includes(p.id);
        let favText = isFav ? 'Đã lưu' : 'Lưu';

        html += `
            <div class="col-12 col-sm-6 col-lg-4 mb-2">
                <div class="product-card border p-3 shadow-sm h-100 d-flex flex-column position-relative">
                    
                    <!-- Khung ảnh túi xách -->
                    <a href="productDetail.html?id=${p.id}" class="text-decoration-none text-dark d-block">
                        <div class="product-img-wrap mb-3">
                            <img src="${p.img}" alt="${p.name}" class="img-fluid">
                        </div>
                    </a>

                    <!-- Nút yêu thích (yêu cầu đăng nhập, không dùng icon) -->
                    <button type="button" class="fav-card-btn ${isFav ? 'active' : ''}" 
                        onclick="toggleCatalogFav(${p.id})" 
                        title="${isFav ? 'Bỏ lưu' : 'Lưu vào danh sách yêu thích'}">
                        ${favText}
                    </button>

                    <!-- Huy hiệu % giảm giá (nổi trên ảnh với z-index cao) -->
                    ${badgeHtml}

                    <div class="d-flex justify-content-between align-items-center mb-1">
                        <span class="text-secondary small fw-bold text-uppercase">${p.brand} · ${p.category}</span>
                        <div class="d-flex align-items-center gap-1">${statusTagHtml}${stockText}</div>
                    </div>

                    <a href="productDetail.html?id=${p.id}" class="text-decoration-none text-dark">
                        <h5 class="fw-bold mb-1" style="font-size: 15px; min-height: 44px; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; text-overflow: ellipsis; line-height: 1.45;" title="${p.name}">${p.name}</h5>
                    </a>

                    <p class="text-muted small mb-3" style="display: -webkit-box; -webkit-line-clamp: 1; -webkit-box-orient: vertical; overflow: hidden; font-size: 13px;">${p.mota || ''}</p>

                    <div class="mt-auto pt-2 border-top">
                        <div class="d-flex align-items-baseline mb-3">
                            ${oldPriceHtml}
                            <span class="text-danger fw-bold fs-5">${priceSale}</span>
                        </div>
                        <div class="d-flex gap-2">
                            <a href="productDetail.html?id=${p.id}" class="btn btn-outline-dark rounded-pill fw-bold py-2 flex-grow-1 text-center" style="font-size: 13px;">
                                Chi tiết
                            </a>
                            <button type="button" class="btn btn-dark rounded-pill fw-bold py-2 px-3 btn-quick-add" onclick="quickAddToCart(${p.id})" title="Thêm nhanh vào giỏ hàng" style="font-size: 13px;">
                                Thêm
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    });

    // Inject trực tiếp vào hàng #productGrid (không lồng thêm .row thừa)
    grid.innerHTML = html;
    renderPagination();
}

//togglecatalogfav: chuyển trạng thái yêu thích ngay trên card catalog
function toggleCatalogFav(productId) {
    if (typeof toggleFavorite === 'function') {
        toggleFavorite(productId);
        // Cập nhật lại UI sau khi thao tác yêu thích
        renderCatalog();
    }
}

//quickaddtocart: thêm nhanh sản phẩm vào giỏ hàng (Yêu cầu đăng nhập)
function quickAddToCart(productId) {
    if (typeof checkLoginForCart === 'function') {
        if (!checkLoginForCart('thêm túi xách vào giỏ hàng')) {
            return;
        }
    } else {
        let userData = localStorage.getItem('currentUser');
        if (!userData || userData === "null") {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Yêu cầu đăng nhập',
                    text: 'Vui lòng đăng nhập tài khoản để thêm túi xách vào giỏ hàng!',
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
                if (confirm('Vui lòng đăng nhập tài khoản để thêm túi xách vào giỏ hàng!')) {
                    window.location.href = "login.html";
                }
            }
            return;
        }
    }

    let allProducts = (typeof getAllProducts === 'function') ? getAllProducts() : (typeof productsDatabase !== 'undefined' ? productsDatabase : []);
    let product = allProducts.find(p => p.id === productId);
    if (!product) return;

    let maxStock = product.soluong || 99;
    if (maxStock <= 0) {
        if (typeof Swal !== 'undefined') {
            Swal.fire('Hết hàng', 'Sản phẩm này hiện đang tạm hết hàng!', 'warning');
        } else {
            alert('Sản phẩm này hiện đang tạm hết hàng!');
        }
        return;
    }

    let cart = (typeof getCart === 'function') ? getCart() : [];
    let existingItem = cart.find(item => item.id === product.id);

    if (existingItem) {
        if (existingItem.quantity + 1 > maxStock) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Vượt quá tồn kho',
                    text: `Trong giỏ hàng đã có ${existingItem.quantity} sản phẩm. Kho chỉ còn ${maxStock} cái!`,
                    icon: 'warning'
                });
            } else {
                alert(`Trong giỏ hàng đã có ${existingItem.quantity} sản phẩm. Kho chỉ còn ${maxStock} cái!`);
            }
            return;
        }
        existingItem.quantity += 1;
    } else {
        cart.push({
            id: product.id,
            name: product.name,
            price: product.price,
            img: product.img,
            brand: product.brand,
            category: product.category,
            quantity: 1,
            maxStock: maxStock
        });
    }

    if (typeof saveCart === 'function') {
        saveCart(cart);
    } else {
        localStorage.setItem('tuixach_cart_guest', JSON.stringify(cart));
    }

    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'Đã thêm vào giỏ!',
            text: `Đã thêm 1 x "${product.name}" vào giỏ hàng.`,
            icon: 'success',
            timer: 1200,
            showConfirmButton: false
        }).then(() => {
            if (typeof openCartPanel === 'function') {
                openCartPanel();
            }
        });
    } else {
        if (typeof openCartPanel === 'function') {
            openCartPanel();
        }
    }
}

//renderpagination: hiển thị phân trang với nút Trước và Sau
function renderPagination() {
    let container = document.getElementById('phantrang');
    if (!container) return;

    let totalPages = Math.ceil(displayedProducts.length / itemsPerPage);
    if (totalPages <= 1) {
        container.innerHTML = "";
        return;
    }

    let html = "";
    if (currentPage > 1) {
        html += `<button class="btn btn-outline-dark px-3 py-1 fw-bold me-1" onclick="goToPage(${currentPage - 1})" title="Trang trước">&laquo;</button>`;
    }

    for (let i = 1; i <= totalPages; i++) {
        let activeClass = (i === currentPage) ? "btn-dark shadow" : "btn-outline-dark";
        html += `<button class="btn ${activeClass} px-3 py-1 fw-bold mx-1" onclick="goToPage(${i})">${i}</button>`;
    }

    if (currentPage < totalPages) {
        html += `<button class="btn btn-outline-dark px-3 py-1 fw-bold ms-1" onclick="goToPage(${currentPage + 1})" title="Trang tiếp theo">&raquo;</button>`;
    }

    container.innerHTML = html;
}

function goToPage(page) {
    currentPage = page;
    renderCatalog();
    let grid = document.getElementById('productGrid');
    if (grid) {
        grid.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}

//clearallfilters: xóa toàn bộ bộ lọc
function clearAllFilters() {
    let checkboxes = document.querySelectorAll('.filter-check input[type="checkbox"]');
    checkboxes.forEach(cb => cb.checked = false);

    let tabs = document.querySelectorAll('.brand-tab');
    tabs.forEach(t => t.classList.remove('active'));
    if (tabs[0]) tabs[0].classList.add('active');

    currentBrand = "all";
    currentSearch = "";
    isSearchMode = false;
    window.history.replaceState(null, null, "catalog.html");

    let searchInput = document.querySelector('.custom-search');
    if (searchInput) searchInput.value = "";

    applyFilters();
}

//handlecatalogsearch: tìm kiếm trong trang danh mục
function handleCatalogSearch(e, input) {
    if (e.key === 'Enter') {
        e.preventDefault();
        currentSearch = input.value.trim();
        isSearchMode = (currentSearch !== "");
        applyFilters();
    } else if (input.value === "" && isSearchMode) {
        currentSearch = "";
        isSearchMode = false;
        applyFilters();
    }
}

function handleCatalogSearchClear(input) {
    if (input.value === "") {
        currentSearch = "";
        isSearchMode = false;
        applyFilters();
    }
}

//initcatalogpage: đọc url params và khởi tạo trang catalog
function initCatalogPage() {
    let params = new URLSearchParams(window.location.search);
    let brandParam = params.get('brand');
    let categoryParam = params.get('category');
    let searchParam = params.get('search');
    let badgeParam = params.get('badge');

    if (searchParam) {
        currentSearch = searchParam;
        isSearchMode = true;
        let searchInput = document.querySelector('.custom-search');
        if (searchInput) searchInput.value = searchParam;
    }

    if (brandParam) {
        currentBrand = brandParam;
        let tabs = document.querySelectorAll('.brand-tab');
        tabs.forEach(t => {
            if (t.getAttribute('data-brand') === brandParam) {
                t.classList.add('active');
            } else {
                t.classList.remove('active');
            }
        });
    }

    if (categoryParam) {
        let checkboxes = document.querySelectorAll('.filter-check input[type="checkbox"]');
        checkboxes.forEach(cb => {
            if (cb.value === categoryParam) cb.checked = true;
        });
    }

    if (badgeParam) {
        let checkboxes = document.querySelectorAll('.filter-check input[type="checkbox"]');
        checkboxes.forEach(cb => {
            if (cb.value === badgeParam) cb.checked = true;
        });
    }

    applyFilters();
}

document.addEventListener('DOMContentLoaded', initCatalogPage);
