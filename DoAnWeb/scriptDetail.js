let currentProduct = null;

//changebuyqty: tăng giảm số lượng mua
function changeBuyQty(delta) {
    let input = document.getElementById('buyQuantity');
    if (!input || !currentProduct) return;

    let currentVal = parseInt(input.value) || 1;
    let maxStock = currentProduct.soluong || 99;
    let newVal = currentVal + delta;

    if (newVal < 1) newVal = 1;
    if (newVal > maxStock) {
        newVal = maxStock;
        Swal.fire({
            title: 'Giới hạn số lượng',
            text: `Rất tiếc, sản phẩm này chỉ còn ${maxStock} cái trong kho!`,
            icon: 'info',
            timer: 1500,
            showConfirmButton: false
        });
    }

    input.value = newVal;
}

//switchmainimg: chuyển đổi ảnh chính từ thumbnail
function switchMainImg(src, thumbEl) {
    let mainImg = document.getElementById('mainProductImg');
    if (mainImg) {
        mainImg.src = src;
    }
    let allThumbs = document.querySelectorAll('.thumb-item');
    allThumbs.forEach(t => t.classList.remove('border-dark', 'shadow'));
    if (thumbEl) {
        thumbEl.classList.add('border-dark', 'shadow');
    }
}

//addtocart: thêm sản phẩm vào giỏ hàng (Yêu cầu đăng nhập)
function addToCart() {
    if (typeof checkLoginForCart === 'function') {
        if (!checkLoginForCart('thêm túi xách vào giỏ hàng')) {
            return;
        }
    } else {
        let userData = localStorage.getItem('currentUser');
        if (!userData || userData === "null") {
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
            return;
        }
    }

    if (!currentProduct) return;

    let qtyInput = document.getElementById('buyQuantity');
    let qtyToAdd = qtyInput ? (parseInt(qtyInput.value) || 1) : 1;
    let maxStock = currentProduct.soluong || 99;

    if (maxStock <= 0) {
        Swal.fire('Hết hàng', 'Sản phẩm này hiện đang tạm hết hàng!', 'warning');
        return;
    }

    let cart = getCart();
    let existingItem = cart.find(item => item.id === currentProduct.id);

    if (existingItem) {
        if (existingItem.quantity + qtyToAdd > maxStock) {
            Swal.fire({
                title: 'Vượt quá tồn kho',
                text: `Trong giỏ hàng đã có ${existingItem.quantity} sản phẩm. Kho chỉ còn ${maxStock} cái!`,
                icon: 'warning'
            });
            return;
        }
        existingItem.quantity += qtyToAdd;
    } else {
        cart.push({
            id: currentProduct.id,
            name: currentProduct.name,
            price: currentProduct.price,
            img: currentProduct.img,
            brand: currentProduct.brand,
            category: currentProduct.category,
            quantity: qtyToAdd,
            maxStock: maxStock
        });
    }

    saveCart(cart);

    Swal.fire({
        title: 'Đã thêm vào giỏ!',
        text: `Đã thêm ${qtyToAdd} x "${currentProduct.name}" vào giỏ hàng.`,
        icon: 'success',
        timer: 1200,
        showConfirmButton: false
    }).then(() => {
        if (typeof openCartPanel === 'function') {
            openCartPanel();
        }
    });
}

//updatefavbtnui: cập nhật giao diện nút yêu thích
function updateFavBtnUI() {
    let btn = document.getElementById('btnToggleFav');
    if (!btn || !currentProduct) return;
    let favList = (typeof getFavList === 'function') ? getFavList() : [];
    if (favList.includes(currentProduct.id)) {
        btn.innerHTML = '❤️ Đã thêm vào yêu thích';
        btn.classList.remove('btn-outline-dark');
        btn.classList.add('btn-danger');
    } else {
        btn.innerHTML = '♡ Thêm vào danh sách yêu thích';
        btn.classList.remove('btn-danger');
        btn.classList.add('btn-outline-dark');
    }
}

//togglecurrentfavorite: thêm hoặc bỏ yêu thích sản phẩm hiện tại (Yêu cầu đăng nhập)
function toggleCurrentFavorite() {
    let urlParams = new URLSearchParams(window.location.search);
    let productId = parseInt(urlParams.get('id'));

    if (productId) {
        if (typeof toggleFavorite === 'function') {
            toggleFavorite(productId);
            updateFavBtnUI();
        } else {
            alert("Lỗi: Không tìm thấy hệ thống Yêu thích.");
        }
    }
}

//renderrandomrelatedproducts: tải sản phẩm cùng thương hiệu hoặc danh mục
function renderRandomRelatedProducts(currentProd) {
    let container = document.getElementById('relatedProducts');
    if (!container || typeof productsDatabase === 'undefined') return;

    let allProds = (typeof getAllProducts === 'function') ? getAllProducts() : productsDatabase;
    let related = allProds.filter(p => p.id !== currentProd.id && (p.brand === currentProd.brand || p.category === currentProd.category));

    if (related.length < 3) {
        let others = allProds.filter(p => p.id !== currentProd.id && !related.includes(p));
        for (let i = 0; i < others.length && related.length < 3; i++) {
            related.push(others[i]);
        }
    }

    let htmlContent = "";
    related.slice(0, 3).forEach(p => {
        let priceFormat = p.price.toLocaleString('vi-VN') + ' ₫';
        htmlContent += `
            <div class="col-10 col-md-4 flex-shrink-0">
                <a href="productDetail.html?id=${p.id}" class="text-decoration-none text-dark related-card">
                    <div class="border rounded-4 p-3 bg-white shadow-sm h-100">
                        <div class="mb-3 bg-light rounded-3 d-flex align-items-center justify-content-center" style="aspect-ratio: 1/1; overflow: hidden;">
                            <img src="${p.img}" class="img-fluid w-100 h-100 object-fit-contain p-3">
                        </div>
                        <span class="text-secondary small fw-bold text-uppercase">${p.brand} · ${p.category}</span>
                        <h5 class="fw-bold my-1 text-truncate">${p.name}</h5>
                        <h5 class="fw-bold mt-2 text-danger">${priceFormat}</h5>
                    </div>
                </a>
            </div>`;
    });
    container.innerHTML = htmlContent;
}

//initproductdetail: tải thông tin chi tiết sản phẩm túi xách
function initProductDetail() {
    let urlParams = new URLSearchParams(window.location.search);
    let productId = parseInt(urlParams.get('id')) || 1;

    let allProds = (typeof getAllProducts === 'function') ? getAllProducts() : (typeof productsDatabase !== 'undefined' ? productsDatabase : []);
    let product = allProds.find(p => p.id === productId);

    if (product) {
        currentProduct = product;

        // Tiêu đề & Breadcrumb
        document.title = product.name + " - Basau Bags";
        let bc = document.getElementById('breadcrumbDetailName');
        if (bc) bc.innerText = product.name;

        // Tên, thương hiệu, loại túi
        let nameEl = document.getElementById('productName');
        let brandEl = document.getElementById('productBrand');
        let catEl = document.getElementById('productCategory');
        if (nameEl) nameEl.innerText = product.name;
        if (brandEl) brandEl.innerText = product.brand;
        if (catEl) catEl.innerText = product.category;

        // Giá & Khuyến mãi
        let priceEl = document.getElementById('productPrice');
        let oldPriceEl = document.getElementById('productOldPrice');
        let discountEl = document.getElementById('productDiscountPercent');
        let promoTag = document.getElementById('productPromoTag');
        let promoName = document.getElementById('productPromoName');

        if (priceEl) priceEl.innerText = product.price.toLocaleString('vi-VN') + " ₫";

        if (product.discountPercent > 0 && product.oldPrice > product.price) {
            if (oldPriceEl) {
                oldPriceEl.innerText = product.oldPrice.toLocaleString('vi-VN') + " ₫";
                oldPriceEl.classList.remove('d-none');
            }
            if (discountEl) {
                discountEl.innerText = `-${product.discountPercent}%`;
                discountEl.classList.remove('d-none');
            }
            if (promoTag && promoName) {
                promoName.innerText = product.promotionName || "Khuyến mãi đặc biệt";
                promoTag.classList.remove('d-none');
            }
        }

        // Tồn kho
        let stockEl = document.getElementById('productStock');
        let btnAddToCart = document.getElementById('btnAddToCart');
        let qtyInput = document.getElementById('buyQuantity');

        if (product.soluong > 0) {
            if (stockEl) {
                stockEl.innerText = `Còn ${product.soluong} sản phẩm trong kho`;
                stockEl.className = "badge bg-success-subtle text-success border border-success px-3 py-2 fw-bold";
            }
            if (qtyInput) qtyInput.max = product.soluong;
            if (btnAddToCart) btnAddToCart.disabled = false;
        } else {
            if (stockEl) {
                stockEl.innerText = "Tạm hết hàng";
                stockEl.className = "badge bg-danger-subtle text-danger border border-danger px-3 py-2 fw-bold";
            }
            if (btnAddToCart) {
                btnAddToCart.disabled = true;
                btnAddToCart.innerText = "HẾT HÀNG";
            }
        }

        // Mô tả chi tiết (bảng tuixach: mota)
        let descEl = document.getElementById('productDescription');
        if (descEl) {
            descEl.innerText = product.mota || "Chưa có mô tả chi tiết cho sản phẩm này.";
        }

        // Thông số kỹ thuật
        let specId = document.getElementById('specId');
        let specBrand = document.getElementById('specBrand');
        let specCat = document.getElementById('specCategory');
        let specPromo = document.getElementById('specPromo');
        let specStock = document.getElementById('specStock');

        if (specId) specId.innerText = `#TX${product.id}`;
        if (specBrand) specBrand.innerText = product.brand;
        if (specCat) specCat.innerText = product.category;
        if (specPromo) specPromo.innerText = product.promotionName || "Không áp dụng";
        if (specStock) specStock.innerText = `${product.soluong} cái`;

        // Ảnh chính
        let mainImg = document.getElementById('mainProductImg');
        if (mainImg) mainImg.src = product.img;

        // Thumbnails Gallery (bảng hinhanhtui)
        let thumbsContainer = document.getElementById('thumbnailsContainer');
        if (thumbsContainer && product.thumbnails && product.thumbnails.length > 0) {
            let thumbHtml = "";
            product.thumbnails.forEach((tSrc, idx) => {
                let borderClass = idx === 0 ? "border-dark shadow" : "";
                thumbHtml += `
                    <div class="thumb-item border rounded-3 p-1 bg-white ${borderClass}" style="width: 75px; height: 75px; cursor: pointer; transition: all 0.2s;" onclick="switchMainImg('${tSrc}', this)">
                        <img src="${tSrc}" class="w-100 h-100 object-fit-contain">
                    </div>
                `;
            });
            thumbsContainer.innerHTML = thumbHtml;
        }

        // Badge
        let badgeEl = document.getElementById('detailBadge');
        if (badgeEl && product.badge) {
            badgeEl.innerText = product.badge;
            badgeEl.classList.remove('d-none');
        }

        renderRandomRelatedProducts(product);
        updateFavBtnUI();
    }
}

window.onload = initProductDetail;