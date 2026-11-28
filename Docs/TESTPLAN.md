# KẾ HOẠCH VÀ KỊCH BẢN KIỂM THỬ (TEST PLAN & TEST CASES)
**Đề tài:** Hệ thống Website Thương Mại Điện Tử Bán Túi Xách Thời Trang BASAU.BAGS  
**Công nghệ:** ASP.NET Core 8 MVC, Entity Framework Core, SQL Server, xUnit, Moq  
**Phương pháp:** Kết hợp Kiểm thử tự động (Automated Unit Testing) & Kiểm thử thủ công (Manual Testing)  

---

## I. TỔNG QUAN VỀ KIỂM THỬ

### 1. Mục tiêu kiểm thử
- Đảm bảo toàn bộ các tầng nghiệp vụ (Business Services), máy trạng thái đơn hàng, giỏ hàng, phân hạng khách hàng VIP, thẻ bảo hành điện tử và các chức năng CRUD quản trị hoạt động chính xác 100%.
- Kiểm tra tính toàn vẹn dữ liệu, ràng buộc khóa ngoại, khả năng xử lý ngoại lệ và logic tính tiền / chiết khấu.
- Kiểm tra giao diện người dùng (UI/UX), tính năng động của các ViewComponent và khả năng tương tác của người dùng trên trình duyệt.

### 2. Môi trường & Công cụ
| Thành phần | Công nghệ / Môi trường |
| :--- | :--- |
| **Framework Unit Test** | `xUnit` (v2.8+) |
| **Thư viện Mocking** | `Moq` (v4.21+) giả lập các phụ thuộc nền (Email, Notification, Logger) |
| **Cơ sở dữ liệu kiểm thử** | `Microsoft.EntityFrameworkCore.InMemory` (v8.0.1) chạy siêu tốc trong RAM, cô lập hoàn toàn với CSDL thật |
| **Kiểm thử thủ công** | Trình duyệt Google Chrome, Microsoft Edge, Windows 11 |
| **Lệnh chạy kiểm thử** | `dotnet test` |

---

## II. PHẦN 1: BỘ KIỂM THỬ TỰ ĐỘNG (AUTOMATED UNIT TESTS - 35 TEST CASES)

Dự án test: `DoAnLTW.Tests` (Cấu trúc đơn giản, tuân thủ mô hình chuẩn **Arrange - Act - Assert**).

### 1. Phân hệ Tài khoản & Xác thực (`AccountTests.cs` - 4 Tests)
| STT | Tên bài test | Mục tiêu kiểm tra | Kết quả kỳ vọng | Trạng thái |
| :-: | :--- | :--- | :--- | :-: |
| 1 | `ValidateCredentials_CorrectPassword` | Đăng nhập đúng mật khẩu | Trả về thông tin tài khoản hợp lệ | **PASS** |
| 2 | `ValidateCredentials_WrongPassword` | Đăng nhập sai mật khẩu | Trả về `null`, từ chối đăng nhập | **PASS** |
| 3 | `Register_NewUser_CreatesAccount` | Đăng ký tài khoản mới hợp lệ | Tạo mới tài khoản và khách hàng thành công | **PASS** |
| 4 | `Register_DuplicateUsername_Failure` | Đăng ký trùng tên đăng nhập | Báo lỗi tên đăng nhập đã được sử dụng | **PASS** |

### 2. Phân hệ Giá bán & Khuyến mãi (`PricingTests.cs` - 2 Tests)
| STT | Tên bài test | Mục tiêu kiểm tra | Kết quả kỳ vọng | Trạng thái |
| :-: | :--- | :--- | :--- | :-: |
| 5 | `GetPrice_WithoutPromotion` | Sản phẩm không có khuyến mãi | Giữ nguyên giá gốc, số tiền giảm = 0 | **PASS** |
| 6 | `GetPrice_WithActivePromotion` | Sản phẩm có khuyến mãi giảm 20% | Tính đúng đơn giá áp dụng đã giảm 20% | **PASS** |

### 3. Phân hệ Quản lý Giỏ hàng (`CartTests.cs` - 5 Tests)
| STT | Tên bài test | Mục tiêu kiểm tra | Kết quả kỳ vọng | Trạng thái |
| :-: | :--- | :--- | :--- | :-: |
| 7 | `AddAsync_NewItem` | Thêm sản phẩm mới vào giỏ | Giỏ có 1 mục, tính đúng tổng tiền & số lượng | **PASS** |
| 8 | `AddAsync_ExistingItem` | Thêm tiếp sản phẩm đã có trong giỏ | Cộng dồn số lượng chính xác | **PASS** |
| 9 | `AddAsync_ExceedsStock` | Thêm số lượng vượt quá tồn kho | Ném ngoại lệ `InvalidOperationException` | **PASS** |
| 10 | `UpdateAsync_ValidQuantity` | Cập nhật số lượng sản phẩm | Cập nhật số lượng và tính lại tổng tiền | **PASS** |
| 11 | `RemoveAsync_ExistingItem` | Xóa sản phẩm khỏi giỏ hàng | Xóa thành công, giỏ hàng về 0 | **PASS** |

### 4. Phân hệ Đặt hàng, Khách VIP & Bảo hành (`OrderTests.cs` - 7 Tests)
| STT | Tên bài test | Mục tiêu kiểm tra | Kết quả kỳ vọng | Trạng thái |
| :-: | :--- | :--- | :--- | :-: |
| 12 | `OrderStatus_CanTransition` | Kiểm tra máy trạng thái đơn hàng | ChoDuyet ➜ DangGiao ➜ HoanTat; chặn sửa đơn đã hủy/hoàn tất | **PASS** |
| 13 | `PaymentMethod_IsValid` | Kiểm tra phương thức thanh toán | Chấp nhận COD và ChuyenKhoan; từ chối chuỗi lạ | **PASS** |
| 14 | `IsVipAsync_CustomerIsVip` | Kiểm tra trạng thái VIP của khách VIP | Trả về `true` | **PASS** |
| 15 | `IsVipAsync_CustomerIsRegular` | Kiểm tra trạng thái VIP của khách thường | Trả về `false` | **PASS** |
| 16 | `CheckoutAsync_RegularCustomer` | Khách thường đặt hàng | Tính đúng giá gốc, không giảm giá | **PASS** |
| 17 | `CheckoutAsync_VipCustomer` | Khách VIP đặt hàng | Tự động áp dụng chiết khấu đặc quyền 5% | **PASS** |
| 18 | `EWarranty_Calculation` | Kiểm tra tính hạn Thẻ bảo hành điện tử 12 tháng | Tính đúng ngày hết hạn, xác định còn/hết hạn chuẩn xác | **PASS** |

### 5. Phân hệ CRUD Danh mục & Thương hiệu (`AdminCategoryTests.cs` - 6 Tests)
| STT | Tên bài test | Mục tiêu kiểm tra | Kết quả kỳ vọng | Trạng thái |
| :-: | :--- | :--- | :--- | :-: |
| 19 | `AddLoaiTuiAsync_ValidCategory` | Thêm mới loại túi xách | Lưu vào CSDL thành công | **PASS** |
| 20 | `UpdateLoaiTuiAsync_ExistingCategory` | Sửa tên loại túi xách | Cập nhật tên mới trong CSDL | **PASS** |
| 21 | `DeleteLoaiTuiAsync_NoProducts` | Xóa loại túi không có sản phẩm | Xóa thành công | **PASS** |
| 22 | `AddThuongHieuAsync_ValidBrand` | Thêm mới thương hiệu | Lưu vào CSDL thành công | **PASS** |
| 23 | `UpdateThuongHieuAsync_ExistingBrand` | Sửa tên thương hiệu | Cập nhật tên mới trong CSDL | **PASS** |
| 24 | `DeleteThuongHieuAsync` | Xóa thương hiệu | Xóa thành công | **PASS** |

### 6. Phân hệ CRUD Quản trị Sản phẩm (`AdminProductTests.cs` - 4 Tests)
| STT | Tên bài test | Mục tiêu kiểm tra | Kết quả kỳ vọng | Trạng thái |
| :-: | :--- | :--- | :--- | :-: |
| 25 | `GetAllAsync_FilterByKeyword` | Lấy danh sách sản phẩm và lọc từ khóa | Trả về đúng sản phẩm khớp tên | **PASS** |
| 26 | `AddAsync_ValidProduct` | Thêm mới sản phẩm đầy đủ thông tin | Lưu đúng giá, tồn kho, loại túi, thương hiệu | **PASS** |
| 27 | `UpdateAsync_ExistingProduct` | Sửa giá bán và tồn kho sản phẩm | Dữ liệu cập nhật chính xác | **PASS** |
| 28 | `DeleteAsync_ExistingProduct` | Xóa sản phẩm an toàn | Xóa thành công khỏi CSDL | **PASS** |

### 7. Phân hệ Quản trị Đơn hàng & Dashboard (`AdminOrderAndDashboardTests.cs` - 2 Tests)
| STT | Tên bài test | Mục tiêu kiểm tra | Kết quả kỳ vọng | Trạng thái |
| :-: | :--- | :--- | :--- | :-: |
| 29 | `AdminOrder_UpdateStatus` | Duyệt đơn hàng sang trạng thái mới | Trạng thái đơn được cập nhật | **PASS** |
| 30 | `AdminDashboard_GetStats` | Thống kê số lượng đơn và doanh thu | Tính đúng tổng đơn và doanh thu các đơn hoàn tất | **PASS** |

### 8. Phân hệ Widget ViewComponent (`ViewComponentTests.cs` - 5 Tests)
| STT | Tên bài test | Mục tiêu kiểm tra | Kết quả kỳ vọng | Trạng thái |
| :-: | :--- | :--- | :--- | :-: |
| 31 | `CartSummary_AuthenticatedUser` | Số đếm giỏ hàng khi đã đăng nhập | Trả về số lượng sản phẩm trong giỏ | **PASS** |
| 32 | `CartSummary_UnauthenticatedUser` | Số đếm giỏ hàng khi chưa đăng nhập | Trả về số lượng bằng 0 | **PASS** |
| 33 | `VipBadge_VipCustomer` | Huy hiệu VIP khi khách hàng là VIP | Model nhận `true` (render badge VIP vàng) | **PASS** |
| 34 | `VipBadge_RegularCustomer` | Huy hiệu VIP khi là khách thường | Model nhận `false` (ẩn huy hiệu) | **PASS** |
| 35 | `VipBadge_UnauthenticatedUser` | Huy hiệu VIP khi chưa đăng nhập | Trả về chuỗi rỗng (`EmptyContent`) | **PASS** |

---

## III. PHẦN 2: BẢNG KỊCH BẢN KIỂM THỬ THỦ CÔNG TRÊN GIAO DIỆN (MANUAL TEST CASES)

| Mã TC | Phân hệ | Tên ca kiểm thử | Thao tác thực hiện | Dữ liệu kiểm thử | Kết quả mong đợi | Trạng thái |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| **TC_M_01** | Xác thực | Đăng ký tài khoản mới | Nhập form đăng ký tại `/Account/Register` | Username: `testuser`, Pass: `Test@123` | Đăng ký thành công, tự động đăng nhập | **PASS** |
| **TC_M_02** | Xác thực | Đăng nhập sai mật khẩu | Nhập sai pass tại `/Account/Login` | Pass: `SaiPass999` | Báo lỗi đỏ không cho đăng nhập | **PASS** |
| **TC_M_03** | Khách hàng | Xem hồ sơ và tiến độ VIP | Bấm vào tên người dùng trên Header | Khách thường | Hiển thị hồ sơ cá nhân và số tiền cần mua thêm để lên VIP | **PASS** |
| **TC_M_04** | Trang chủ | Tương tác banner Flash Sale 3D | Dùng chuột xoay mô hình 3D trên banner | Thao tác rê/xoay chuột | Mô hình 3D xoay mượt mà, đồng hồ đếm ngược chạy đúng | **PASS** |
| **TC_M_05** | Danh mục | Lọc theo loại và tìm kiếm | Chọn loại túi bên menu trái và gõ từ khóa | Loại: Túi Đeo Chéo, Từ khóa: "Canvas" | Danh sách sản phẩm lọc chuẩn xác theo điều kiện | **PASS** |
| **TC_M_06** | Chi tiết | Xem chi tiết sản phẩm | Click vào sản phẩm bất kỳ | Túi Tote Da | Hiển thị đủ ảnh lớn, giá gốc, giá KM, tồn kho và mô tả | **PASS** |
| **TC_M_07** | Giỏ hàng | Thêm vào giỏ và đếm realtime | Bấm nút "Thêm vào giỏ" ở trang chi tiết | Số lượng: 2 | Header ViewComponent tự động nhảy số `(2)` không cần tải lại trang | **PASS** |
| **TC_M_08** | Giỏ hàng | Đổi số lượng và xóa món | Bấm nút `+`/`-` và nút xóa trong giỏ | Thao tác trên giỏ hàng | Số lượng tăng giảm, tiền tính lại tức thì | **PASS** |
| **TC_M_09** | Thanh toán | Khách thường đặt hàng COD | Vào trang thanh toán, chọn COD | Địa chỉ: 123 Lê Lợi | Đặt hàng thành công, hiển thị trang hoàn tất, trừ tồn kho | **PASS** |
| **TC_M_10** | Khách VIP | Hưởng chiết khấu đặc quyền VIP | Khách VIP tiến hành thanh toán | Giỏ hàng: 2.000.000₫ | Màn hình hiển thị giảm giá VIP 5% (-100.000₫), tổng còn 1.900.000₫ | **PASS** |
| **TC_M_11** | Bảo hành | Mở Thẻ bảo hành điện tử | Bấm nút "Thẻ bảo hành" ở đơn Hoàn tất | Đơn hoàn tất 1 tháng trước | Mở Modal hiển thị mã BH, thời hạn 12 tháng, badge "Còn hạn (335 ngày)" | **PASS** |
| **TC_M_12** | Admin | Đăng nhập quản trị và Dashboard | Đăng nhập tài khoản Admin | TK Admin | Mở giao diện Admin Dashboard với biểu đồ và số liệu thống kê | **PASS** |
| **TC_M_13** | Admin | CRUD Sản phẩm (Thêm kèm ảnh) | Nhập form thêm sản phẩm, upload ảnh từ máy | Tên: "Túi Dior", Giá: 35tr | Sản phẩm mới lưu vào CSDL, ảnh hiển thị đúng trên trang chủ | **PASS** |
| **TC_M_14** | Admin | Duyệt đơn hàng | Chọn đơn hàng mới, duyệt sang "Đang giao" | Đơn hàng chờ duyệt | Đơn chuyển trạng thái tuần tự; không thể sửa khi đã Hoàn tất | **PASS** |
| **TC_M_15** | Chatbot | Tư vấn túi xách tự động bằng AI | Bấm icon chat ở góc màn hình, gõ câu hỏi | "Gợi ý túi đi làm thanh lịch" | AI phản hồi câu tư vấn tự nhiên kèm link sản phẩm phù hợp | **PASS** |

---

## IV. TỔNG KẾT VÀ KẾT QUẢ THỰC NGHIỆM

1. **Tổng số ca kiểm thử:**
   - **35 Ca kiểm thử tự động (Unit Tests)** trên toàn bộ tầng Service, Admin và ViewComponent.
   - **15 Ca kiểm thử thủ công (Manual Tests)** bao phủ toàn bộ luồng người dùng trên giao diện.
2. **Tỷ lệ thành công:** **100% (50 / 50 ca kiểm thử ĐẠT - PASS)**.
3. **Thời gian chạy Unit Test:** ~1.5 - 2.0 giây cho toàn bộ 35 bài test.
4. **Đánh giá chung:** Hệ thống hoạt động ổn định, chính xác, đáp ứng toàn diện tiêu chí đánh giá đồ án công nghệ phần mềm.
