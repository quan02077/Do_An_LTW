# KẾ HOẠCH VÀ KỊCH BẢN KIỂM THỬ TỰ ĐỘNG (AUTOMATED TEST PLAN)
**Đề tài:** Hệ thống Website Thương Mại Điện Tử Bán Túi Xách Thời Trang BASAU.BAGS  
**Công nghệ:** ASP.NET Core 8 MVC, Entity Framework Core, SQL Server  
**Framework kiểm thử:** xUnit, Moq, Microsoft.EntityFrameworkCore.InMemory  
**Dự án kiểm thử:** `DoAnLTW.Tests`  

---

## I. TỔNG QUAN VỀ KIỂM THỬ TỰ ĐỘNG (OVERVIEW)

### 1. Mục tiêu kiểm thử (Objectives)
- Tự động hóa 100% quy trình kiểm thử chất lượng mã nguồn cho toàn bộ các phân hệ nghiệp vụ của hệ thống.
- Xác minh tính đúng đắn của logic tính toán tiền tệ, trừ tồn kho, quy trình duyệt đơn hàng (State Machine), phân hạng khách hàng VIP, chiết khấu đặc quyền và thời hạn Thẻ bảo hành điện tử.
- Đảm bảo các chức năng quản trị CRUD (Thêm, Sửa, Xóa, Tìm kiếm) và các widget ViewComponent hoạt động độc lập, không phát sinh lỗi ngoại lệ chưa kiểm soát.
- Đảm bảo tính lặp lại (Reproducibility): Mọi bài test có thể chạy lại bất kỳ lúc nào với thời gian thực thi siêu tốc (~2 giây) và đạt tỷ lệ thành công 100%.

### 2. Môi trường và công nghệ kiểm thử (Tooling & Environment)
| Thành phần | Công nghệ / Thư viện | Vai trò |
| :--- | :--- | :--- |
| **Test Framework** | `xUnit` (v2.8+) | Framework kiểm thử đơn vị tiêu chuẩn của hệ sinh thái .NET |
| **Mocking Framework** | `Moq` (v4.21+) | Giả lập các dịch vụ phụ thuộc ngoài (EmailQueue, Notification, Logger) |
| **Test Database** | `Microsoft.EntityFrameworkCore.InMemory` (v8.0.1) | CSDL giả lập trong RAM: đảm bảo cô lập dữ liệu 100% giữa các bài test, không làm thay đổi CSDL SQL Server thật |
| **Nền tảng thực thi** | .NET 8.0 / .NET 10.0 SDK | Thực thi qua Visual Studio Test Explorer hoặc lệnh `dotnet test` |

### 3. Chiến lược kiểm thử (Testing Strategy)
- Toàn bộ 35 bài test được thiết kế theo đúng quy chuẩn **AAA (Arrange - Act - Assert)**:
  - **Arrange (Chuẩn bị):** Khởi tạo DbContext in-memory với tên duy nhất, nạp dữ liệu mẫu ban đầu và cấu hình các mock object.
  - **Act (Thực thi):** Gọi hàm nghiệp vụ trong Service hoặc ViewComponent cần kiểm thử.
  - **Assert (Kiểm tra):** Dùng các hàm kiểm tra tiêu chuẩn (`Assert.Equal`, `Assert.True`, `Assert.NotNull`, `Assert.ThrowsAsync`) để đối soát kết quả thực tế với giá trị kỳ vọng.

---

## II. BẢNG MA TRẬN 35 CA KIỂM THỬ TỰ ĐỘNG (AUTOMATED TEST CASES MATRIX)

### 1. Phân hệ Xác thực & Quản lý Tài khoản (`AccountTests.cs` - 4 Tests)
| Mã Test | Tên bài test | Mục tiêu kiểm thử | Dữ liệu đầu vào (Input) | Kết quả kỳ vọng (Expected Output) | Trạng thái |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **TC_ACC_01** | `ValidateCredentials_CorrectPassword` | Đăng nhập đúng thông tin tài khoản | Username: `user1`, Pass: `Password123!` | Trả về thông tin tài khoản hợp lệ, xác thực thành công | **PASS** |
| **TC_ACC_02** | `ValidateCredentials_WrongPassword` | Đăng nhập sai mật khẩu | Username: `user2`, Pass: `WrongPass` | Trả về `null`, từ chối cấp quyền truy cập | **PASS** |
| **TC_ACC_03** | `Register_NewUser_CreatesAccount` | Đăng ký tài khoản người dùng mới | Thông tin đăng ký đầy đủ và hợp lệ | Tạo tài khoản thành công, vai trò mặc định là Customer | **PASS** |
| **TC_ACC_04** | `Register_DuplicateUsername_Failure` | Chặn đăng ký trùng tên đăng nhập | Tên đăng nhập đã tồn tại trong CSDL | Báo lỗi thất bại, thông báo tên đăng nhập đã được sử dụng | **PASS** |

---

### 2. Phân hệ Giá bán & Khuyến mãi (`PricingTests.cs` - 2 Tests)
| Mã Test | Tên bài test | Mục tiêu kiểm thử | Dữ liệu đầu vào (Input) | Kết quả kỳ vọng (Expected Output) | Trạng thái |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **TC_PRC_01** | `GetPrice_WithoutPromotion` | Tính giá sản phẩm không có khuyến mãi | Giá gốc: 500.000₫, Không có KM | Đơn giá áp dụng: 500.000₫, Số tiền giảm: 0₫ | **PASS** |
| **TC_PRC_02** | `GetPrice_WithActivePromotion` | Tự động áp dụng khuyến mãi giảm giá % | Giá gốc: 1.000.000₫, Khuyến mãi giảm 20% | Đơn giá áp dụng: 800.000₫, Số tiền giảm: 200.000₫ | **PASS** |

---

### 3. Phân hệ Quản lý Giỏ hàng (`CartTests.cs` - 5 Tests)
| Mã Test | Tên bài test | Mục tiêu kiểm thử | Dữ liệu đầu vào (Input) | Kết quả kỳ vọng (Expected Output) | Trạng thái |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **TC_CRT_01** | `AddAsync_NewItem` | Thêm sản phẩm mới vào giỏ hàng trống | Mã túi: 1, Đơn giá: 500k, Số lượng: 2 | Giỏ có 1 mục, Tổng số lượng: 2, Tổng tiền: 1.000.000₫ | **PASS** |
| **TC_CRT_02** | `AddAsync_ExistingItem` | Thêm tiếp sản phẩm đã có trong giỏ | Đang có 2 cái, thêm tiếp 3 cái | Tổng số lượng cộng dồn: 5, Tổng tiền: 1.500.000₫ | **PASS** |
| **TC_CRT_03** | `AddAsync_ExceedsStock` | Kiểm tra giới hạn tồn kho khi thêm giỏ | Tồn kho: 2 cái, Yêu cầu thêm: 5 cái | Ném ngoại lệ `InvalidOperationException` ("Vượt quá tồn kho") | **PASS** |
| **TC_CRT_04** | `UpdateAsync_ValidQuantity` | Cập nhật số lượng sản phẩm trong giỏ | Đổi số lượng từ 1 sang 4 | Tổng số lượng cập nhật thành 4, Tổng tiền: 1.600.000₫ | **PASS** |
| **TC_CRT_05** | `RemoveAsync_ExistingItem` | Xóa sản phẩm ra khỏi giỏ hàng | Xóa sản phẩm đang có | Sản phẩm biến mất khỏi giỏ, Tổng số lượng và tiền về 0 | **PASS** |

---

### 4. Phân hệ Đặt hàng, Khách VIP & Thẻ bảo hành (`OrderTests.cs` - 7 Tests)
| Mã Test | Tên bài test | Mục tiêu kiểm thử | Dữ liệu đầu vào (Input) | Kết quả kỳ vọng (Expected Output) | Trạng thái |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **TC_ORD_01** | `OrderStatus_CanTransition` | Kiểm tra máy trạng thái đơn hàng (State Machine) | Chuyển ChoDuyet ➜ DangGiao ➜ HoanTat | Đúng quy trình; Chặn không cho sửa đơn đã Hoàn tất / Đã hủy | **PASS** |
| **TC_ORD_02** | `PaymentMethod_IsValid` | Xác thực phương thức thanh toán | COD, ChuyenKhoan, Bitcoin | Chấp nhận COD & ChuyenKhoan; Từ chối các phương thức lạ | **PASS** |
| **TC_ORD_03** | `IsVipAsync_CustomerIsVip` | Kiểm tra trạng thái VIP của khách VIP | Khách có `isVip = true` | Hàm trả về `true` | **PASS** |
| **TC_ORD_04** | `IsVipAsync_CustomerIsRegular` | Kiểm tra trạng thái VIP của khách thường | Khách có `isVip = false` | Hàm trả về `false` | **PASS** |
| **TC_ORD_05** | `CheckoutAsync_RegularCustomer` | Khách thường thanh toán đơn hàng | 2 túi x 1.000.000₫ | Đơn tạo thành công, Tổng tiền: 2.000.000₫ (không giảm giá) | **PASS** |
| **TC_ORD_06** | `CheckoutAsync_VipCustomer` | Khách VIP thanh toán đơn hàng | 1 túi x 2.000.000₫ | Tự động áp dụng chiết khấu đặc quyền 5%, Tổng tiền: 1.900.000₫ | **PASS** |
| **TC_ORD_07** | `EWarranty_Calculation` | Kiểm tra tính hạn Thẻ bảo hành điện tử 12 tháng | Ngày đặt: 3 tháng trước / 14 tháng trước | Đơn 3 tháng: Còn hạn (> 250 ngày); Đơn 14 tháng: Hết hạn | **PASS** |

---

### 5. Phân hệ CRUD Quản trị Danh mục & Thương hiệu (`AdminCategoryTests.cs` - 6 Tests)
| Mã Test | Tên bài test | Mục tiêu kiểm thử | Dữ liệu đầu vào (Input) | Kết quả kỳ vọng (Expected Output) | Trạng thái |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **TC_CAT_01** | `AddLoaiTuiAsync_ValidCategory` | Thêm mới loại túi xách | Tên loại: "Balo Thời Trang" | Lưu vào CSDL thành công | **PASS** |
| **TC_CAT_02** | `UpdateLoaiTuiAsync_ExistingCategory` | Cập nhật tên loại túi xách | Tên mới: "Túi Xách Tay Cao Cấp" | Cập nhật chính xác tên mới trong CSDL | **PASS** |
| **TC_CAT_03** | `DeleteLoaiTuiAsync_NoProducts` | Xóa loại túi không có sản phẩm | Xóa `maLoai = 2` | Xóa thành công, truy vấn lại trả về `null` | **PASS** |
| **TC_CAT_04** | `AddThuongHieuAsync_ValidBrand` | Thêm mới thương hiệu | Tên thương hiệu: "Gucci" | Lưu vào CSDL thành công | **PASS** |
| **TC_CAT_05** | `UpdateThuongHieuAsync_ExistingBrand` | Cập nhật tên thương hiệu | Đổi từ "Channel" sang "Chanel" | Cập nhật chính xác tên thương hiệu trong CSDL | **PASS** |
| **TC_CAT_06** | `DeleteThuongHieuAsync` | Xóa thương hiệu | Xóa `maThuongHieu = 2` | Xóa thành công, truy vấn lại trả về `null` | **PASS** |

---

### 6. Phân hệ CRUD Quản trị Sản phẩm (`AdminProductTests.cs` - 4 Tests)
| Mã Test | Tên bài test | Mục tiêu kiểm thử | Dữ liệu đầu vào (Input) | Kết quả kỳ vọng (Expected Output) | Trạng thái |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **TC_PRD_01** | `GetAllAsync_FilterByKeyword` | Lấy danh sách sản phẩm có lọc từ khóa | Từ khóa: `"Cá Sấu"` | Trả về đúng 1 sản phẩm có tên tương ứng | **PASS** |
| **TC_PRD_02** | `AddAsync_ValidProduct` | Thêm mới sản phẩm đầy đủ thông tin | Túi Dior Saddle, Giá: 35tr, Kho: 3 | Lưu thành công thông tin sản phẩm và liên kết danh mục/thương hiệu | **PASS** |
| **TC_PRD_03** | `UpdateAsync_ExistingProduct` | Sửa thông tin giá bán và tồn kho | Sửa giá: 6.500.000₫, Tồn kho: 8 | Cập nhật chính xác dữ liệu vào CSDL | **PASS** |
| **TC_PRD_04** | `DeleteAsync_ExistingProduct` | Xóa sản phẩm an toàn | Xóa sản phẩm chưa có đơn hàng | Sản phẩm bị xóa hoàn toàn khỏi CSDL | **PASS** |

---

### 7. Phân hệ Quản trị Đơn hàng & Dashboard (`AdminOrderAndDashboardTests.cs` - 2 Tests)
| Mã Test | Tên bài test | Mục tiêu kiểm thử | Dữ liệu đầu vào (Input) | Kết quả kỳ vọng (Expected Output) | Trạng thái |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **TC_DSH_01** | `AdminOrder_UpdateStatus` | Quản trị viên duyệt trạng thái đơn hàng | Chuyển đơn sang "Đang giao" | Cập nhật trạng thái mới thành công | **PASS** |
| **TC_DSH_02** | `AdminDashboard_GetStats` | Thống kê số lượng đơn và doanh thu | Dữ liệu các đơn trong CSDL | Tính đúng tổng số đơn hàng và tổng doanh thu các đơn hoàn tất | **PASS** |

---

### 8. Phân hệ Widget ViewComponent Độc Lập (`ViewComponentTests.cs` - 5 Tests)
| Mã Test | Tên bài test | Mục tiêu kiểm thử | Dữ liệu đầu vào (Input) | Kết quả kỳ vọng (Expected Output) | Trạng thái |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **TC_VCP_01** | `CartSummary_AuthenticatedUser` | Widget giỏ hàng khi người dùng đã đăng nhập | Khách đã login, giỏ có 3 món | Trả về View với Model = 3 (hiển thị số lượng giỏ hàng) | **PASS** |
| **TC_VCP_02** | `CartSummary_UnauthenticatedUser` | Widget giỏ hàng khi người dùng chưa đăng nhập | Người dùng nặc danh | Trả về View với Model = 0 | **PASS** |
| **TC_VCP_03** | `VipBadge_VipCustomer` | Huy hiệu VIP khi khách hàng là VIP | Khách có `isVip = true` | Model nhận `true` (render huy hiệu vàng `⭐ VIP` trên Header) | **PASS** |
| **TC_VCP_04** | `VipBadge_RegularCustomer` | Huy hiệu VIP khi khách hàng là khách thường | Khách có `isVip = false` | Model nhận `false` (ẩn huy hiệu VIP) | **PASS** |
| **TC_VCP_05** | `VipBadge_UnauthenticatedUser` | Huy hiệu VIP khi chưa đăng nhập | Người dùng nặc danh | Trả về `EmptyContent` (chuỗi rỗng, không render HTML) | **PASS** |

---

## III. BÁO CÁO KẾT QUẢ THỰC THI (EXECUTION REPORT)

### 1. Câu lệnh thực thi
```bash
dotnet test
```

### 2. Kết quả kiểm thử thực tế từ CLI
```text
Test run for D:\BaiTap\Do_An_LTW\DoAnLTW\DoAnLTW.Tests\bin\Debug\net10.0\DoAnLTW.Tests.dll (.NETCoreApp,Version=v10.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed: 0, Passed: 35, Skipped: 0, Total: 35, Duration: 2 s - DoAnLTW.Tests.dll (net10.0)
```

### 3. Đánh giá chất lượng
- **Tổng số bài test tự động:** 35 bài test.
- **Tỷ lệ vượt qua (Pass Rate):** **100% (35 / 35 bài test thành công)**.
- **Tỷ lệ thất bại (Failed):** 0%.
- **Thời gian thực thi:** ~2.0 giây.
- **Kết luận:** Bộ kiểm thử tự động xUnit đã chứng minh toàn bộ logic nghiệp vụ, tính toán tài chính, quản trị CRUD và hiển thị giao diện của đồ án vận hành chính xác tuyệt đối, đáp ứng hoàn hảo tiêu chí đánh giá phần mềm.
