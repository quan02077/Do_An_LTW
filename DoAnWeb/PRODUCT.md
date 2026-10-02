# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users
- **Người dùng chính (Khách mua sắm)**: Học sinh, sinh viên, giới trẻ và nhân viên văn phòng tại Việt Nam yêu thích thời trang, có nhu cầu tìm kiếm và mua sắm các loại túi xách, balo, phụ kiện (túi đeo chéo, túi tote, túi dự tiệc, clutch...).
- **Người dùng quản trị (Admin)**: Người quản lý cửa hàng, giảng viên hoặc người đánh giá đồ án cần theo dõi, thao tác quản lý kho hàng, danh mục sản phẩm, chương trình khuyến mãi, đơn đặt hàng và tài khoản người dùng.

## Product Purpose
"Basau Bags" (Cửa Hàng Túi Xách & Balo Thời Trang) là đồ án môn học Web xây dựng hệ thống website thương mại điện tử chuyên kinh doanh túi xách và balo thời trang. Website cung cấp cho khách hàng trải nghiệm duyệt sản phẩm, lọc theo thuộc tính, xem chi tiết, giỏ hàng và thanh toán trực quan; đồng thời cung cấp bảng điều khiển quản trị (Admin) đầy đủ để kiểm soát dữ liệu bán hàng. Dự án hướng tới một trải nghiệm mượt mà, giao diện hiện đại, chuẩn thẩm mỹ mà không cần phụ thuộc vào backend phức tạp.

## Positioning
Cửa hàng túi xách thời trang đa thương hiệu (CHARLES & KEITH, Natoli, Pedro, ELLY, Jamlos) với trải nghiệm mua sắm trực tuyến sống động, kết hợp lưu trữ và đồng bộ trạng thái thời gian thực ở client (giỏ hàng, danh sách yêu thích, đơn hàng và quản lý kho).

## Operating Context
- Hoạt động trên mọi trình duyệt web desktop và di động phổ biến tại Việt Nam.
- Chạy trực tiếp trên môi trường tĩnh client-side (thông qua Live Server hoặc mở trực tiếp file HTML).
- Mô hình dữ liệu khớp với thiết kế cơ sở dữ liệu quan hệ (`quan_ly_tui_xach.sql`), được mô phỏng linh hoạt thông qua JavaScript (`data.js`) và lưu trữ bền vững với `localStorage`.

## Capabilities and Constraints
- **Tính năng đã hoàn thiện**:
  - Giao diện khách hàng: Trang chủ với slider bộ sưu tập nổi bật, Trang danh mục (Catalog) hỗ trợ lọc nâng cao (thương hiệu, loại túi, tầm giá, sắp xếp), Chi tiết sản phẩm với thư viện ảnh nhiều góc độ, Trang Flash Sale đếm ngược thời gian thực, Giỏ hàng dạng slide-out / modal & Trang thanh toán (Checkout) đầy đủ, Đăng nhập / Đăng ký tài khoản và lịch sử đơn hàng.
  - Giao diện quản trị (Admin): Quản lý sản phẩm (CRUD), quản lý danh mục, quản lý khuyến mãi, theo dõi đơn hàng và tài khoản.
- **Ràng buộc kỹ thuật**:
  - Kiến trúc Multi-Page Application (MPA) thuần với HTML, CSS, JavaScript và Bootstrap 5.
  - Cấu trúc dữ liệu trong `data.js` đồng bộ với `localStorage`, bám sát thiết kế cơ sở dữ liệu `quan_ly_tui_xach.sql`.
  - Không yêu cầu backend server, chạy độc lập trên trình duyệt.

## Brand Commitments
- **Tên thương hiệu**: Basau Bags - Cửa Hàng Túi Xách & Balo Thời Trang.
- **Ngôn ngữ**: Tiếng Việt (vi-VN) cho toàn bộ nhãn, nội dung, tiền tệ (VNĐ), định dạng ngày tháng và trang quản trị.
- **Danh mục thương hiệu**: Các thương hiệu thời trang uy tín (CHARLES & KEITH, Natoli, Pedro, ELLY, Jamlos).
- **Nhận diện & Thẩm mỹ**: Phong cách thiết kế hiện đại, typography thanh lịch (Josefin Sans, Oswald), hình ảnh sản phẩm chỉn chu cùng các huy hiệu khuyến mãi bắt mắt.

## Evidence on Hand
- File script định nghĩa cơ sở dữ liệu SQL chi tiết: `quan_ly_tui_xach.sql`.
- Bộ dữ liệu mẫu JavaScript phong phú với 15 mẫu sản phẩm, 5 thương hiệu, 6 loại túi, 5 chương trình khuyến mại: `data.js`.
- Thư mục tài nguyên hình ảnh sản phẩm và banner có sẵn: `hinhAnh/` (`banner*.jpg`, `tui*_*.png`).
- Hệ thống các trang hoàn chỉnh: `homePage.html`, `catalog.html`, `productDetail.html`, `flashSale.html`, `checkout.html`, `login.html`, `admin.html`.

## Product Principles
1. **Khám phá sản phẩm trực quan**: Giúp khách hàng tìm đúng chiếc túi yêu thích nhanh nhất thông qua bộ lọc thông minh (thương hiệu, danh mục, giá) và hình ảnh chi tiết.
2. **Quy trình mua hàng liền mạch**: Tạo trải nghiệm mượt mà từ lúc xem banner quảng bá, lướt chi tiết, thêm giỏ hàng đến khi hoàn tất đặt hàng.
3. **Đồng bộ và toàn vẹn dữ liệu**: Đảm bảo giỏ hàng, danh sách yêu thích, đơn hàng và các cập nhật từ trang quản trị luôn được lưu trữ nhất quán giữa các phiên làm việc.
4. **Chuẩn mực trải nghiệm thương mại điện tử Việt Nam**: Thể hiện chính xác cách định dạng giá tiền (VNĐ), thông điệp ưu đãi và ngữ cảnh mua sắm quen thuộc của người dùng Việt.
