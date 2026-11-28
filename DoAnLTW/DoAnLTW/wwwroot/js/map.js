document.addEventListener('DOMContentLoaded', function () {
    const mapElement = document.getElementById('map');
    if (!mapElement) return;
    const oDiaChi = document.querySelector('textarea[name*="DiaChi"], input[name*="DiaChi"]');

    const saveLat = sessionStorage.getItem('lat');
    const saveLng = sessionStorage.getItem('lng');

    const toaDo = saveLat && saveLng ? [parseFloat(saveLat), parseFloat(saveLng)] : [10.7769, 106.7009];

    const map = L.map('map').setView(toaDo, 14);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);

    const marker = L.marker(toaDo, { draggable: true }).addTo(map);

    let geoTimer = null;
    let lastGeoAt = 0;

    async function capNhatViTri(lat, lng, zoom = null) {
        sessionStorage.setItem('lat', lat);   // chỉ nhớ trong phiên duyệt web, không lưu vị trí lâu dài trên máy
        sessionStorage.setItem('lng', lng);

        marker.setLatLng([lat, lng]);
        if (zoom) map.setView([lat, lng], zoom);

        if (oDiaChi) oDiaChi.placeholder = "Đang lấy địa chỉ...";
        // Nominatim (OpenStreetMap) chỉ cho tối đa 1 yêu cầu/giây: gom các lần bấm/kéo liên tiếp, chỉ hỏi lần cuối
        clearTimeout(geoTimer);
        const wait = Math.max(0, 1100 - (Date.now() - lastGeoAt));
        geoTimer = setTimeout(async () => {
            lastGeoAt = Date.now();
            try {
                const res = await fetch(`https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json&accept-language=vi`);
                const data = await res.json();
                if (data.display_name && oDiaChi) {
                    oDiaChi.value = data.display_name;
                }
            } catch (err) {
                console.error("Lỗi lấy địa chỉ:", err);
            } finally {
                if (oDiaChi) oDiaChi.placeholder = "";
            }
        }, wait);
    }

    map.on('click', e => capNhatViTri(e.latlng.lat, e.latlng.lng));

    marker.on('dragend', () => {
        const pos = marker.getLatLng();
        capNhatViTri(pos.lat, pos.lng);
    });

    const btnGPS = document.getElementById('btnViTriHienTai');
    if (btnGPS) {
        btnGPS.onclick = () => {
            if (!navigator.geolocation) return alert("Trình duyệt không hỗ trợ định vị!");

            navigator.geolocation.getCurrentPosition(
                pos => capNhatViTri(pos.coords.latitude, pos.coords.longitude, 16),
                () => alert("Vui lòng cho phép truy cập vị trí trên trình duyệt!")
            );
        };
    }
});