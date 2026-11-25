/*
 * Bộ xem sản phẩm xoay 360° (kiểu Apple): kéo chuột/vuốt để lật qua chuỗi ảnh chụp quanh sản phẩm.
 *
 * Hai chế độ:
 *  - "sequence": có từ 8 ảnh trở lên (thả ảnh vào wwwroot/images/bag360, đặt tên theo thứ tự frame_001, frame_002...).
 *    Kéo sang trái/phải để xoay vòng, thả tay có quán tính, rảnh thì tự xoay chậm.
 *  - "sway": chỉ có 1 ảnh tĩnh (chưa có chuỗi ảnh). Không giả vờ xoay 360°: ảnh chỉ nghiêng nhẹ theo tay kéo rồi tự về giữa.
 *
 * Danh sách ảnh do máy chủ truyền qua thuộc tính data-frames (JSON) của #bagViewport.
 * Đảo chiều xoay: đặt data-reverse="true" nếu thứ tự ảnh chụp ngược chiều tay kéo.
 */
(function () {
    'use strict';

    var viewport = document.getElementById('bagViewport');
    var canvas = document.getElementById('bag360Canvas');
    if (!viewport || !canvas || !canvas.getContext) return;

    var ctx = canvas.getContext('2d');
    var loadingEl = document.getElementById('bag360Loading');
    var scrubber = document.getElementById('bagScrubber');
    var btnToggle = document.getElementById('btnToggleSpin');
    var btnText = document.getElementById('btnSpinText');
    var hintEl = document.getElementById('bagDragHint');
    var shadowEl = document.getElementById('bagGroundShadow');

    var urls;
    try { urls = JSON.parse(viewport.getAttribute('data-frames') || '[]'); } catch (e) { urls = []; }
    if (!Array.isArray(urls) || urls.length === 0) return;

    var DIRECTION = viewport.getAttribute('data-reverse') === 'true' ? -1 : 1;
    var MIN_FRAMES_FOR_SEQUENCE = 8;
    var AUTO_FRAMES_PER_SEC = 7;     // tự xoay: 36 ảnh ≈ 5 giây một vòng
    var FRICTION = 3.2;              // quán tính tắt dần (càng lớn càng dừng nhanh)
    var MAX_VELOCITY = 60;           // khung hình/giây, chặn cú vuốt quá mạnh
    var RESUME_DELAY_MS = 2500;      // thả tay bao lâu thì tự xoay lại
    var SWAY_MAX_DEG = 22;           // góc nghiêng tối đa ở chế độ "sway"

    var reduceMotion = !!(window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches);

    var images = [];                 // Image đã tải
    var count = urls.length;
    var sequence = false;
    var ready = false;
    var pos = 0;                     // sequence: chỉ số khung (số thực, quay vòng); sway: góc nghiêng (độ)
    var vel = 0;                     // sequence: khung/giây; sway: độ/giây
    var dragging = false;
    var lastX = 0;
    var lastT = 0;
    var autoplay = !reduceMotion;
    var resumeAt = 0;
    var inView = true;
    var rafId = 0;
    var lastTs = 0;
    var drawnIndex = -1;
    var cssW = 0;
    var cssH = 0;

    function clamp(v, lo, hi) { return Math.max(lo, Math.min(hi, v)); }
    function wrap(v) { return ((v % count) + count) % count; }
    function canAuto() { return inView && !document.hidden; }

    // ---------- Vẽ ----------
    function resize() {
        var rect = canvas.getBoundingClientRect();
        var dpr = Math.min(window.devicePixelRatio || 1, 2);
        cssW = rect.width;
        cssH = rect.height;
        if (cssW === 0 || cssH === 0) return;
        canvas.width = Math.round(cssW * dpr);
        canvas.height = Math.round(cssH * dpr);
        ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        drawnIndex = -1;
        render(true);
    }

    function draw(img) {
        ctx.clearRect(0, 0, cssW, cssH);
        if (!img) return;
        var scale = Math.min(cssW / img.naturalWidth, cssH / img.naturalHeight) * 0.94;
        var w = img.naturalWidth * scale;
        var h = img.naturalHeight * scale;
        ctx.imageSmoothingQuality = 'high';
        ctx.drawImage(img, (cssW - w) / 2, (cssH - h) / 2, w, h);
    }

    function render(force) {
        if (cssW === 0) return;
        if (!ready) { draw(images[0]); return; }

        if (sequence) {
            var i = wrap(Math.round(pos));
            if (force || i !== drawnIndex) {
                drawnIndex = i;
                draw(images[i]);
                if (scrubber && Number(scrubber.value) !== i) scrubber.value = String(i);
                if (shadowEl) {
                    // bóng đổ dưới sàn co giãn nhẹ theo góc xoay
                    var a = (i / count) * Math.PI * 2;
                    shadowEl.style.transform = 'scale(' + (0.88 + 0.12 * Math.abs(Math.cos(a))).toFixed(3) + ')';
                }
            }
        } else {
            if (force) draw(images[0]);
            canvas.style.transform = 'perspective(900px) rotateY(' + pos.toFixed(2) + 'deg)';
            if (shadowEl) shadowEl.style.transform = 'scale(' + (1 - Math.abs(pos) / 120).toFixed(3) + ')';
        }
    }

    // ---------- Vòng lặp hoạt ảnh (chỉ chạy khi cần) ----------
    function schedule() {
        if (!rafId && ready && inView && !document.hidden) rafId = requestAnimationFrame(tick);
    }

    function tick(ts) {
        rafId = 0;
        var dt = lastTs ? Math.min((ts - lastTs) / 1000, 0.05) : 0.016;
        lastTs = ts;
        var keepGoing = false;

        if (!dragging) {
            var autoNow = autoplay && ts >= resumeAt && canAuto();
            if (sequence) {
                if (Math.abs(vel) > 0.25) {
                    pos = wrap(pos + vel * dt);
                    vel *= Math.exp(-FRICTION * dt);
                    keepGoing = true;
                } else {
                    vel = 0;
                    if (autoNow) { pos = wrap(pos + AUTO_FRAMES_PER_SEC * dt); }
                }
            } else {
                // lò xo kéo góc nghiêng về giữa; khi tự chạy thì đung đưa nhẹ
                var target = autoNow ? Math.sin(ts / 1400) * 10 : 0;
                vel += (target - pos) * 40 * dt;
                vel *= Math.exp(-6 * dt);
                pos += vel * dt;
                keepGoing = Math.abs(target - pos) > 0.05 || Math.abs(vel) > 0.05;
            }
            keepGoing = keepGoing || autoplay;   // đang chờ tới giờ tự xoay lại
        }

        render(false);
        if (keepGoing) {
            lastTs = ts;
            schedule();
        } else {
            lastTs = 0;
        }
    }

    function userTouched() {
        resumeAt = performance.now() + RESUME_DELAY_MS;
        if (hintEl) hintEl.classList.add('is-hidden');
    }

    // ---------- Kéo chuột / vuốt ----------
    viewport.addEventListener('pointerdown', function (e) {
        if (!ready || (e.pointerType === 'mouse' && e.button !== 0)) return;
        dragging = true;
        lastX = e.clientX;
        lastT = performance.now();
        vel = 0;
        try { viewport.setPointerCapture(e.pointerId); } catch (err) { /* trình duyệt cũ */ }
        viewport.classList.add('is-dragging');
        userTouched();
    });

    viewport.addEventListener('pointermove', function (e) {
        if (!dragging) return;
        var now = performance.now();
        var dx = e.clientX - lastX;
        var dt = Math.max((now - lastT) / 1000, 0.001);
        lastX = e.clientX;
        lastT = now;

        if (sequence) {
            // kéo hết bề ngang khung ≈ một vòng quay
            var frames = (DIRECTION * dx) / Math.max(3, cssW / count);
            pos = wrap(pos + frames);
            vel = vel * 0.7 + (frames / dt) * 0.3;
        } else {
            pos = clamp(pos + dx * 0.25, -SWAY_MAX_DEG, SWAY_MAX_DEG);
            vel = 0;
        }
        render(false);
    });

    function endDrag() {
        if (!dragging) return;
        dragging = false;
        viewport.classList.remove('is-dragging');
        vel = clamp(vel, -MAX_VELOCITY, MAX_VELOCITY);
        userTouched();
        lastTs = 0;
        schedule();
    }
    viewport.addEventListener('pointerup', endDrag);
    viewport.addEventListener('pointercancel', endDrag);
    viewport.addEventListener('lostpointercapture', endDrag);

    // ---------- Bàn phím ----------
    viewport.addEventListener('keydown', function (e) {
        if (!ready) return;
        var step = e.key === 'ArrowRight' ? 1 : (e.key === 'ArrowLeft' ? -1 : 0);
        if (!step) return;
        e.preventDefault();
        vel = 0;
        if (sequence) { pos = wrap(pos + step * DIRECTION); }
        else { pos = clamp(pos + step * 6, -SWAY_MAX_DEG, SWAY_MAX_DEG); }
        userTouched();
        render(false);
        schedule();
    });

    // ---------- Thanh trượt góc xoay ----------
    if (scrubber) {
        scrubber.addEventListener('input', function () {
            vel = 0;
            pos = Number(scrubber.value) || 0;
            userTouched();
            render(false);
        });
    }

    // ---------- Nút bật/tắt tự xoay ----------
    if (btnToggle) {
        btnToggle.setAttribute('aria-pressed', String(autoplay));
        btnToggle.classList.toggle('is-active', autoplay);
        btnToggle.addEventListener('click', function () {
            autoplay = !autoplay;
            btnToggle.classList.toggle('is-active', autoplay);
            btnToggle.setAttribute('aria-pressed', String(autoplay));
            resumeAt = 0;
            lastTs = 0;
            schedule();
        });
    }

    // ---------- Chỉ chạy khi nhìn thấy ----------
    if ('IntersectionObserver' in window) {
        new IntersectionObserver(function (entries) {
            inView = entries[0].isIntersecting;
            if (inView) { lastTs = 0; schedule(); }
        }, { threshold: 0.15 }).observe(viewport);
    }
    document.addEventListener('visibilitychange', function () { lastTs = 0; schedule(); });

    if ('ResizeObserver' in window) { new ResizeObserver(resize).observe(canvas); }
    else { window.addEventListener('resize', resize); }

    // ---------- Tải ảnh ----------
    function setProgress(done) {
        if (loadingEl && count > 1) loadingEl.textContent = 'Đang tải ' + Math.round((done / count) * 100) + '%';
    }

    function onLoaded() {
        images = images.filter(function (img) { return !!img; });
        if (images.length === 0) {
            if (loadingEl) loadingEl.textContent = 'Không tải được ảnh sản phẩm';
            return;
        }
        count = images.length;
        sequence = count >= MIN_FRAMES_FOR_SEQUENCE;
        ready = true;
        viewport.classList.add('is-ready');
        viewport.classList.toggle('is-sway', !sequence);

        if (sequence) {
            if (scrubber) { scrubber.max = String(count - 1); scrubber.value = '0'; }
        } else {
            // chưa có chuỗi ảnh xoay: nói đúng sự thật, không gọi là 360°
            if (scrubber) scrubber.parentNode.classList.add('is-no-scrubber');
            if (btnText) btnText.textContent = 'XEM 3D';
            if (hintEl) hintEl.textContent = 'Kéo để nghiêng';
            viewport.setAttribute('aria-label', 'Ảnh sản phẩm, kéo để nghiêng');
        }
        resize();
        schedule();
    }

    var done = 0;
    urls.forEach(function (url, i) {
        var img = new Image();
        img.decoding = 'async';
        img.onload = function () {
            images[i] = img;
            if (i === 0) { render(false); }
            setProgress(++done);
            if (done === urls.length) onLoaded();
        };
        img.onerror = function () {
            images[i] = null;
            setProgress(++done);
            if (done === urls.length) onLoaded();
        };
        img.src = url;
    });

    resize();
})();
