/*
 * Bộ xem sản phẩm xoay 360° (kiểu Apple): kéo chuột/vuốt để lật qua chuỗi ảnh chụp quanh sản phẩm.
 *
 * Ba chế độ (tự chọn theo số ảnh trong wwwroot/images/bag360, đặt tên theo thứ tự frame_001, frame_002...):
 *  - "sequence": từ 8 ảnh trở lên (chuỗi ảnh chụp quanh sản phẩm). Kéo sang trái/phải để xoay vòng, thả tay có quán tính,
 *    rảnh thì tự xoay chậm.
 *  - "views": 2-7 ảnh (vài góc nhìn khác nhau). Không đủ ảnh để xoay mượt nên kéo sẽ chuyển dần giữa các góc (hòa ảnh),
 *    thả tay thì dừng ở góc gần nhất; rảnh thì tự đổi góc sau vài giây. Nút ghi "NHIỀU GÓC" chứ không nói là 360°.
 *  - "sway": chỉ có 1 ảnh tĩnh. Không giả vờ xoay: ảnh chỉ nghiêng nhẹ theo tay kéo rồi tự về giữa.
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
    var VIEW_EASE = 9;               // chế độ views: độ nhanh khi ảnh dừng vào góc gần nhất
    var AUTO_VIEW_MS = 3200;         // chế độ views: bao lâu tự đổi sang góc kế tiếp
    var AUTO_FRAMES_PER_SEC = 7;     // tự xoay: 36 ảnh ≈ 5 giây một vòng
    var FRICTION = 3.2;              // quán tính tắt dần (càng lớn càng dừng nhanh)
    var MAX_VELOCITY = 60;           // khung hình/giây, chặn cú vuốt quá mạnh
    var RESUME_DELAY_MS = 2500;      // thả tay bao lâu thì tự xoay lại
    var SWAY_MAX_DEG = 22;           // góc nghiêng tối đa ở chế độ "sway"

    var reduceMotion = !!(window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches);

    var images = [];                 // Image đã tải
    var count = urls.length;
    var sequence = false;
    var views = false;
    var snapTarget = 0;              // chế độ views: chỉ số góc đang hướng tới
    var nextAutoAt = 0;
    var drawnKey = '';
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

    function drawFit(img, alpha) {
        if (!img || alpha <= 0) return;
        var scale = Math.min(cssW / img.naturalWidth, cssH / img.naturalHeight) * 0.94;
        var w = img.naturalWidth * scale;
        var h = img.naturalHeight * scale;
        ctx.globalAlpha = alpha;
        ctx.imageSmoothingQuality = 'high';
        ctx.drawImage(img, (cssW - w) / 2, (cssH - h) / 2, w, h);
        ctx.globalAlpha = 1;
    }

    function draw(img) {
        ctx.clearRect(0, 0, cssW, cssH);
        drawFit(img, 1);
    }

    function render(force) {
        if (cssW === 0) return;
        if (!ready) { draw(images[0]); return; }

        if (views) {
            // hòa hai góc liền kề theo phần lẻ của vị trí (ảnh nền trong suốt, túi tối nên hòa ảnh không lộ)
            var p = wrap(pos);
            var i0 = Math.floor(p);
            var t = p - i0;
            t = t * t * (3 - 2 * t);
            var key = i0 + ':' + t.toFixed(3);
            if (force || key !== drawnKey) {
                drawnKey = key;
                ctx.clearRect(0, 0, cssW, cssH);
                drawFit(images[i0], 1 - t);
                drawFit(images[(i0 + 1) % count], t);
                var idx = wrap(Math.round(pos));
                if (scrubber && Number(scrubber.value) !== idx) scrubber.value = String(idx);
            }
        } else if (sequence) {
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
            } else if (views) {
                var diff = snapTarget - pos;
                if (Math.abs(diff) > 0.002) {
                    pos += diff * (1 - Math.exp(-VIEW_EASE * dt));
                    keepGoing = true;
                } else {
                    pos = snapTarget;
                }
                if (autoNow && ts >= nextAutoAt && Math.abs(snapTarget - pos) < 0.01) {
                    snapTarget = Math.round(pos) + 1;   // tự đổi sang góc kế tiếp
                    nextAutoAt = ts + AUTO_VIEW_MS;
                    keepGoing = true;
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
        nextAutoAt = resumeAt + AUTO_VIEW_MS;
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
        } else if (views) {
            // kéo khoảng 70-100px thì sang góc kế bên
            var moved = (DIRECTION * dx) / Math.max(70, cssW / count * 1.4);
            pos += moved;
            vel = clamp(vel * 0.7 + (moved / dt) * 0.3, -6, 6);
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
        if (views) {
            snapTarget = Math.round(pos + vel * 0.12);   // cú vuốt nhanh thì đi thêm một góc
            vel = 0;
        }
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
        if (views) { snapTarget = Math.round(snapTarget) + step * DIRECTION; }   // cộng dồn khi nhấn liên tiếp
        else if (sequence) { pos = wrap(pos + step * DIRECTION); }
        else { pos = clamp(pos + step * 6, -SWAY_MAX_DEG, SWAY_MAX_DEG); }
        userTouched();
        render(false);
        schedule();
    });

    // ---------- Thanh trượt góc xoay ----------
    if (scrubber) {
        scrubber.addEventListener('input', function () {
            vel = 0;
            if (views) {
                // đi theo đường ngắn nhất quanh vòng các góc
                var cur = wrap(Math.round(pos));
                var d = (Number(scrubber.value) || 0) - cur;
                if (d > count / 2) d -= count;
                if (d < -count / 2) d += count;
                snapTarget = Math.round(pos) + d;
            } else {
                pos = Number(scrubber.value) || 0;
            }
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
        views = !sequence && count >= 2;
        ready = true;
        viewport.classList.add('is-ready');
        viewport.classList.toggle('is-sway', !sequence && !views);

        if (sequence || views) {
            if (scrubber) { scrubber.max = String(count - 1); scrubber.value = '0'; }
            if (views) {
                // vài góc nhìn khác nhau, chưa đủ để xoay mượt: nói đúng sự thật, không gọi là 360°
                nextAutoAt = performance.now() + 1800;
                if (btnText) btnText.textContent = 'NHIỀU GÓC';
                if (hintEl) hintEl.textContent = 'Kéo để đổi góc nhìn';
                viewport.setAttribute('aria-label', 'Túi xách nhiều góc nhìn. Kéo hoặc dùng phím mũi tên trái phải để đổi góc');
            }
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
