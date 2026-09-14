# Hướng Dẫn Sử Dụng Thư Mục Website (Cần Thiết)

Thư mục **`website/`** này chứa toàn bộ mã nguồn cần thiết để chạy ứng dụng quản lý sự kiện **EventVN** độc lập, không cần cài đặt Node.js hay chạy lệnh npm.

---

## 1. Danh sách các file trong thư mục

| Tên file / thư mục | Mô tả chức năng |
| :--- | :--- |
| **`index.html`** | **Trang chủ**: Tìm kiếm sự kiện, lọc theo danh mục, bookmark trái tim, phân quyền Admin |
| **`event-detail.html`** | **Chi tiết sự kiện**: Xem thông tin, lịch trình và popup đăng ký vé |
| **`auth.html`** | **Đăng nhập & Đăng ký**: Tab chuyển đổi, nút ẩn/hiện mật khẩu, 2 nút Demo 1-chạm (Admin & Thành viên) |
| **`dashboard.html`** | **Vé của tôi & Yêu thích**: Danh sách vé điện tử đã đăng ký, mã vé EVN-2026, hủy vé, xem sự kiện yêu thích |
| **`profile.html`** | **Hồ sơ cá nhân**: Banner cover, sửa thông tin, đổi mật khẩu và nút đăng xuất |
| **`admin.html`** | **Bảng quản trị Admin**: Quản lý sự kiện (thêm/xóa), duyệt vé đăng ký |
| **`ConnectionDB.php`** | **Kết nối MySQL CSDL**: Cấu hình kết nối Aiven Cloud MySQL sẵn sàng cho PHP |
| **`css/style.css`** | Định dạng giao diện, font chữ Outfit, hiệu ứng animation trái tim |
| **`js/`** | Toàn bộ mã nguồn JavaScript xử lý tương tác, đăng nhập, phân quyền, lưu dữ liệu |

---

## 2. Cách chạy ứng dụng

### Cách 1: Mở trực tiếp trên máy tính
- Bạn chỉ cần **nhấp đúp chuột vào file `index.html`** để mở trên trình duyệt Chrome, Edge, Cốc Cốc hoặc Firefox.

### Cách 2: Chạy trong môi trường PHP (XAMPP / WampServer)
1. Sao chép toàn bộ thư mục `website/` này vào thư mục `htdocs` của XAMPP (ví dụ: `C:/xampp/htdocs/quan_ly_su_kien/`).
2. Mở trình duyệt và truy cập `http://localhost/quan_ly_su_kien/index.html` (hoặc đổi đuôi thành `.php` để nhúng code PHP từ `ConnectionDB.php`).

