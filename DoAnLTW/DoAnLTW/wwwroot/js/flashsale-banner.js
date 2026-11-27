/**
 * flashsale-banner.js
 * Quản lý tương tác mô hình 3D (Google Model Viewer) và đồng hồ đếm ngược Flash Sale.
 */
(function () {
    'use strict';

    // 1. QUẢN LÝ MÔ HÌNH 3D (GOOGLE MODEL-VIEWER)
    var model = document.getElementById('bagModel');
    var btn = document.getElementById('btnToggleSpin');
    var hint = document.getElementById('bagDragHint');
    var load = document.getElementById('bagLoad');

    if (model) {
        // Tôn trọng thiết lập "giảm chuyển động" của hệ điều hành
        if (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            model.removeAttribute('auto-rotate');
        }

        function syncButtonState() {
            var isRotating = model.hasAttribute('auto-rotate');
            if (btn) {
                btn.classList.toggle('is-active', isRotating);
                btn.setAttribute('aria-pressed', String(isRotating));
            }
        }

        // Bật/tắt tự động xoay
        if (btn) {
            btn.addEventListener('click', function () {
                if (model.hasAttribute('auto-rotate')) {
                    model.removeAttribute('auto-rotate');
                } else {
                    model.setAttribute('auto-rotate', '');
                }
                syncButtonState();
            });
        }
        syncButtonState();

        // Tiến trình tải mô hình 3D
        var bar = load ? load.firstElementChild : null;
        model.addEventListener('progress', function (e) {
            var progress = e.detail && typeof e.detail.totalProgress === 'number' ? e.detail.totalProgress : 0;
            if (bar) bar.style.transform = 'scaleX(' + progress.toFixed(3) + ')';
            if (load) load.setAttribute('aria-valuenow', String(Math.round(progress * 100)));
            if (progress >= 1 && load) load.classList.add('is-done');
        });

        model.addEventListener('load', function () {
            if (load) load.classList.add('is-done');
        });

        // Khi người dùng chạm/kéo chuột xoay túi lần đầu, ẩn dòng chữ gợi ý
        model.addEventListener('camera-change', function (e) {
            if (e.detail && e.detail.source === 'user-interaction' && hint) {
                hint.classList.add('is-hidden');
            }
        });

        // Xử lý lỗi tải file .glb: giữ lại ảnh poster tĩnh
        model.addEventListener('error', function () {
            var poster = model.querySelector('img[slot="poster"]');
            if (poster && poster.src) {
                model.setAttribute('poster', poster.src);
            }
            if (load) load.classList.add('is-done');
            if (hint) hint.textContent = 'Không tải được mô hình 3D';
        });
    }

    // 2. ĐỒNG HỒ ĐẾM NGƯỢC (LIVE COUNTDOWN TIMER)
    var elDays = document.getElementById('fs-days');
    var elHours = document.getElementById('fs-hours');
    var elMinutes = document.getElementById('fs-minutes');
    var elSeconds = document.getElementById('fs-seconds');

    if (elDays && elHours && elMinutes && elSeconds) {
        var days = 2, hours = 14, minutes = 45, seconds = 30;

        function updateCountdown() {
            if (seconds > 0) {
                seconds--;
            } else {
                seconds = 59;
                if (minutes > 0) {
                    minutes--;
                } else {
                    minutes = 59;
                    if (hours > 0) {
                        hours--;
                    } else {
                        hours = 23;
                        if (days > 0) days--;
                    }
                }
            }
            elDays.textContent = String(days).padStart(2, '0');
            elHours.textContent = String(hours).padStart(2, '0');
            elMinutes.textContent = String(minutes).padStart(2, '0');
            elSeconds.textContent = String(seconds).padStart(2, '0');
        }

        setInterval(updateCountdown, 1000);
    }
})();
