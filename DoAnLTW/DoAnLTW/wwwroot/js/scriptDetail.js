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

//addtocart: thêm sản phẩm vào giỏ hàng (yêu cầu đăng nhập)
function addToCart() {
    let isServerAuth = (typeof window.serverIsAuthenticated !== 'undefined' && window.serverIsAuthenticated === true) || 
                       (typeof window.serverUsername !== 'undefined' && window.serverUsername && window.serverUsername.length > 0);
    let userData = localStorage.getItem('currentUser');
    let hasLocalUser = userData && userData !== "null";

    if (typeof checkLoginForCart === 'function') {
        if (!checkLoginForCart('thêm túi xách vào giỏ hàng')) {
            return;
        }
    } else if (!isServerAuth && !hasLocalUser) {
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
                    window.location.href = "/Account/Login";
                }
            });
        } else {
            if (confirm('Vui lòng đăng nhập tài khoản để thêm túi xách vào giỏ hàng!')) {
                window.location.href = "/Account/Login";
            }
        }
        return;
    }

    if (!currentProduct) return;

    let qtyInput = document.getElementById('buyQuantity');
    let qtyToAdd = qtyInput ? (parseInt(qtyInput.value) || 1) : 1;
    let maxStock = currentProduct.soluong || 99;

    if (maxStock <= 0) {
        Swal.fire('Hết hàng', 'Sản phẩm này hiện đang tạm hết hàng!', 'warning');
        return;
    }

    let cart = (typeof getCart === 'function') ? getCart() : [];
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

    if (typeof saveCart === 'function') {
        saveCart(cart);
    }

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
        btn.innerHTML = 'ĐÃ LƯU VÀO DANH SÁCH YÊU THÍCH';
        btn.classList.remove('btn-outline-dark');
        btn.classList.add('btn-dark');
    } else {
        btn.innerHTML = 'LƯU VÀO DANH SÁCH YÊU THÍCH';
        btn.classList.remove('btn-dark');
        btn.classList.add('btn-outline-dark');
    }
}

//togglecurrentfavorite: thêm hoặc bỏ yêu thích sản phẩm hiện tại (yêu cầu đăng nhập)
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

//lay duong dan trang chi tiet tuy theo moi truong (mvc / static html)
function getProductDetailUrl(id) {
    let pathname = window.location.pathname;
    if (pathname.toLowerCase().includes('/catalog/detail')) {
        return `/Catalog/Detail?id=${id}`;
    }
    return `productDetail.html?id=${id}`;
}

//selectrelatedproduct: chuyen doi sang san pham lien quan truc tiep tren trang
function selectRelatedProduct(id, e) {
    if (e) {
        if (e.ctrlKey || e.metaKey || e.shiftKey || e.which === 2) {
            return;
        }
        e.preventDefault();
    }
    let targetUrl = getProductDetailUrl(id);
    if (window.history && window.history.pushState) {
        window.history.pushState({ id: id }, '', targetUrl);
    }
    initProductDetail(id);
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// lang nghe su kien back / forward cua trinh duyet
window.addEventListener('popstate', function(event) {
    let idFromState = (event && event.state && event.state.id) ? event.state.id : null;
    initProductDetail(idFromState);
});

//renderrandomrelatedproducts: tải sản phẩm cùng thương hiệu hoặc danh mục
function renderRandomRelatedProducts(currentProd) {
    let container = document.getElementById('relatedProducts');
    if (!container || typeof productsDatabase === 'undefined') return;

    let allProds = (typeof getAllProducts === 'function') ? getAllProducts() : productsDatabase;
    let currentId = currentProd.id || currentProd.matui;
    let related = allProds.filter(p => (p.id !== currentId && p.matui !== currentId) && (p.brand === currentProd.brand || p.category === currentProd.category));

    if (related.length < 4) {
        let others = allProds.filter(p => (p.id !== currentId && p.matui !== currentId) && !related.some(r => r.id === p.id));
        for (let i = 0; i < others.length && related.length < 4; i++) {
            related.push(others[i]);
        }
    }

    let htmlContent = "";
    related.slice(0, 4).forEach(p => {
        let priceFormat = p.price.toLocaleString('vi-VN') + ' ₫';
        let detailUrl = getProductDetailUrl(p.id);
        htmlContent += `
            <div class="col-10 col-md-3 flex-shrink-0">
                <a href="${detailUrl}" onclick="selectRelatedProduct(${p.id}, event)" class="text-decoration-none text-dark related-card d-block h-100" style="cursor: pointer;">
                    <div class="border rounded-4 p-3 bg-white shadow-sm h-100 d-flex flex-column transition-hover">
                        <div class="mb-3 bg-light rounded-3 d-flex align-items-center justify-content-center" style="aspect-ratio: 1/1; overflow: hidden;">
                            <img src="${p.img}" class="img-fluid w-100 h-100 object-fit-contain p-3" alt="${p.name}">
                        </div>
                        <span class="text-secondary small fw-bold text-uppercase">${p.brand} · ${p.category}</span>
                        <h5 class="fw-bold my-1 text-truncate" title="${p.name}">${p.name}</h5>
                        <h5 class="fw-bold mt-auto pt-2 text-danger">${priceFormat}</h5>
                    </div>
                </a>
            </div>`;
    });
    container.innerHTML = htmlContent;
}

//initproductdetail: tải thông tin chi tiết sản phẩm túi xách
function initProductDetail(targetId) {
    let numId = targetId;
    if (!numId) {
        let urlParams = new URLSearchParams(window.location.search);
        let idParam = urlParams.get('id');
        if (idParam) {
            numId = parseInt(idParam, 10);
        }
    }

    let allProds = (typeof getAllProducts === 'function') ? getAllProducts() : (typeof productsDatabase !== 'undefined' ? productsDatabase : []);
    let product = null;

    if (numId && !isNaN(numId) && numId > 0) {
        product = allProds.find(p => p.id === numId || p.matui === numId);
    }

    // nếu không tìm thấy theo id, lấy sản phẩm đầu tiên làm mặc định để không bị trắng giao diện
    if (!product && allProds.length > 0) {
        product = allProds[0];
    }

    if (product) {
        currentProduct = product;

        // tiêu đề & breadcrumb
        document.title = product.name + " - Basau Bags";
        let bc = document.getElementById('breadcrumbDetailName');
        if (bc) bc.innerText = product.name;

        // tên, thương hiệu, loại túi
        let nameEl = document.getElementById('productName');
        let brandEl = document.getElementById('productBrand');
        let catEl = document.getElementById('productCategory');
        if (nameEl) nameEl.innerText = product.name;
        if (brandEl) brandEl.innerText = product.brand;
        if (catEl) catEl.innerText = product.category;

        // giá & khuyến mãi
        let priceEl = document.getElementById('productPrice');
        let oldPriceEl = document.getElementById('productOldPrice');
        let discountEl = document.getElementById('productDiscountPercent');
        let promoTag = document.getElementById('productPromoTag');
        let promoName = document.getElementById('productPromoName');

        if (priceEl) priceEl.innerText = product.price.toLocaleString('vi-VN') + " ₫";

        if (product.discountPercent > 0 && product.oldPrice && product.oldPrice > product.price) {
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
        } else {
            if (oldPriceEl) oldPriceEl.classList.add('d-none');
            if (discountEl) discountEl.classList.add('d-none');
            if (promoTag) promoTag.classList.add('d-none');
        }

        // tồn kho & số lượng
        let stockEl = document.getElementById('productStock');
        let btnAddToCart = document.getElementById('btnAddToCart');
        let qtyInput = document.getElementById('buyQuantity');

        if (qtyInput) {
            qtyInput.value = 1;
        }

        if (product.soluong > 0) {
            if (stockEl) {
                stockEl.innerText = `Còn ${product.soluong} sản phẩm trong kho`;
                stockEl.className = "badge bg-success-subtle text-success border border-success px-3 py-2 fw-bold";
            }
            if (qtyInput) qtyInput.max = product.soluong;
            if (btnAddToCart) {
                btnAddToCart.disabled = false;
                btnAddToCart.innerText = "THÊM VÀO GIỎ HÀNG";
            }
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

        // mô tả chi tiết (bảng tuixach: mota)
        let descEl = document.getElementById('productDescription');
        if (descEl) {
            descEl.innerText = product.mota || "Chưa có mô tả chi tiết cho sản phẩm này.";
        }

        // thông số kỹ thuật
        let specId = document.getElementById('specId');
        let specBrand = document.getElementById('specBrand');
        let specCat = document.getElementById('specCategory');
        let specPromo = document.getElementById('specPromo');
        let specStock = document.getElementById('specStock');

        if (specId) specId.innerText = `#TX${product.id}`;
        if (specBrand) specBrand.innerText = product.brand;
        if (specCat) specCat.innerText = product.category;
        if (specPromo) specPromo.innerText = product.promotionName || (product.discountPercent > 0 ? `Giảm ${product.discountPercent}%` : "Không áp dụng");
        if (specStock) specStock.innerText = `${product.soluong} cái`;

        // ảnh chính
        let mainImg = document.getElementById('mainProductImg');
        if (mainImg) mainImg.src = product.img;

        // thumbnails gallery (bảng hinhanhtui)
        let thumbsContainer = document.getElementById('thumbnailsContainer');
        if (thumbsContainer) {
            let thumbs = (product.thumbnails && product.thumbnails.length > 0) ? product.thumbnails : [product.img];
            let thumbHtml = "";
            thumbs.forEach((tSrc, idx) => {
                let borderClass = idx === 0 ? "border-dark shadow" : "";
                thumbHtml += `
                    <div class="thumb-item border rounded-3 p-1 bg-white ${borderClass}" style="width: 75px; height: 75px; cursor: pointer; transition: all 0.2s;" onclick="switchMainImg('${tSrc}', this)">
                        <img src="${tSrc}" class="w-100 h-100 object-fit-contain" alt="thumb">
                    </div>
                `;
            });
            thumbsContainer.innerHTML = thumbHtml;
        }

        // badge
        let badgeEl = document.getElementById('detailBadge');
        if (badgeEl) {
            if (product.badge) {
                badgeEl.innerText = product.badge;
                badgeEl.classList.remove('d-none');
            } else {
                badgeEl.innerText = "";
                badgeEl.classList.add('d-none');
            }
        }

        renderRandomRelatedProducts(product);
        updateFavBtnUI();
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => initProductDetail());
} else {
    initProductDetail();
}
window.onload = function() {
    initProductDetail();
};
