// Dãy sản phẩm nổi bật ở trang chủ tự chạy ngang liên tục.
// Nhân đôi dãy để vòng lặp liền mạch; bản sao ẩn với trình đọc màn hình và không focus được.
(function () {
    'use strict';

    var SPEED = 55;            // px mỗi giây
    var marquee = document.querySelector('.hp-marquee');
    if (!marquee) return;
    var track = marquee.querySelector('.hp-track');
    if (!track || !track.children.length) return;

    var cloned = false;

    function setWidth() {
        // độ dài một dãy = nửa tổng khi đã nhân đôi; trước khi nhân đôi = toàn bộ track
        var gap = parseFloat(getComputedStyle(track).columnGap) || 0;
        var total = track.scrollWidth;
        return cloned ? (total + gap) / 2 : total;
    }

    function clone() {
        if (cloned) return;
        Array.prototype.slice.call(track.children).forEach(function (item) {
            var c = item.cloneNode(true);
            c.setAttribute('aria-hidden', 'true');
            c.setAttribute('inert', '');
            track.appendChild(c);
        });
        cloned = true;
    }

    function unclone() {
        if (!cloned) return;
        var half = track.children.length / 2;
        while (track.children.length > half) track.removeChild(track.lastChild);
        cloned = false;
    }

    function update() {
        if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            unclone();
            marquee.classList.remove('is-running');
            return;
        }
        unclone();
        var one = setWidth();
        // dãy ngắn hơn khung nhìn thì không cần chạy (tránh hở trống)
        if (one <= marquee.clientWidth) {
            marquee.classList.remove('is-running');
            return;
        }
        clone();
        marquee.style.setProperty('--hp-duration', (one / SPEED).toFixed(1) + 's');
        marquee.classList.add('is-running');
    }

    var timer;
    window.addEventListener('resize', function () {
        clearTimeout(timer);
        timer = setTimeout(update, 200);
    });
    // chờ ảnh tải xong để đo đúng (ảnh có chiều cao cố định nhưng đo chiều ngang không phụ thuộc, vẫn đợi cho chắc)
    if (document.readyState === 'complete') update();
    else window.addEventListener('load', update);
})();
