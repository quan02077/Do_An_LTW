# ĐỒ ÁN LẬP TRÌNH WEB - BASAU BAGS STORE 👜

Website thương mại điện tử chuyên kinh doanh các sản phẩm túi xách thời trang cao cấp (**BaSau Bags Store**), được phát triển trên nền tảng **ASP.NET Core 8 MVC** và **Entity Framework Core**.

---

## 📁 Cấu Trúc Thư Mục Dự Án

```text
Do_An_LTW/
├── .gitignore
├── README.md                        # Hướng dẫn chạy project, tài khoản Admin/User mẫu
├── Docs/                            # Gom toàn bộ tài liệu báo cáo vào đây
│   ├── Dac_Ta_Co_So_Du_Lieu_QL_TuiXach.docx
│   └── quan_ly_tui_xach.sql
└── DoAnLTW/                         # Solution chính
    ├── DoAnLTW.sln
    ├── run.bat
    └── DoAnLTW/
        ├── Areas/                   # Phân hệ quản trị (tích hợp từ admin4)
        │   └── Admin/
        │       ├── Controllers/     # Controller quản trị (Product, Category, Order, Promotion, Home)
        │       ├── Services/        # Logic nghiệp vụ quản trị
        │       ├── ViewModels/      # ViewModel cho trang Admin
        │       └── Views/           # Giao diện quản trị (layout, dashboard, crud...)
        ├── Controllers/             # Khu vực khách hàng
        │   ├── AccountController.cs
        │   ├── CartApiController.cs
        │   ├── CartController.cs
        │   ├── CatalogController.cs
        │   ├── FlashSaleController.cs
        │   ├── HomeController.cs
        │   └── OrdersController.cs
        ├── Data/                    # AppDbContext, DbInitializer (Tự động migrate & seed data)
        ├── Migrations/              # InitialCreate, ModelSnapshot (EF Core Migrations)
        ├── Models/                  # Entities (TuiXach, KhachHang, DonHang, TaiKhoan, KhuyenMai...)
        ├── Services/                # Business Logic (Cart, Catalog, Order, Pricing, Customer...)
        ├── ViewModels/              # Dữ liệu truyền giữa Controller và View
        ├── Views/                   # Giao diện khách hàng
        │   ├── Account/
        │   ├── Cart/
        │   ├── Catalog/
        │   ├── FlashSale/
        │   ├── Home/
        │   ├── Orders/
        │   └── Shared/
        ├── wwwroot/                 # Tài nguyên tĩnh (bao gồm cả asset của admin4: css, js, images, font...)
        ├── appsettings.json         # Chuỗi kết nối Database và cấu hình hệ thống
        ├── Program.cs               # Cấu hình DI, Cookie Authentication, Routing
        └── DoAnLTW.csproj           # Cấu hình project .NET 8
```

---

## 🛠️ Công Nghệ Sử Dụng

- **Backend:** C# / .NET 8 (ASP.NET Core MVC)
- **ORM:** Entity Framework Core 8 (Code-First với Migrations & Auto-Seeder)
- **Database:** Microsoft SQL Server
- **Frontend:** Razor Pages (View Engine), Bootstrap 5, HTML5, CSS3, JavaScript, ApexCharts, SweetAlert
- **Xác thực:** Cookie Authentication (`[Authorize]`, phân quyền Role: `Người bán`, `Khách hàng`)

---

## ⚙️ Cấu Hình Kết Nối Cơ Sở Dữ Liệu (Database Connection)

File cấu hình chuỗi kết nối nằm tại:  
`DoAnLTW/DoAnLTW/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost;Initial Catalog=QL_TuiXach;Integrated Security=True;Trust Server Certificate=True"
  }
}
```

> [!NOTE]
> Tuỳ thuộc vào phiên bản và cấu hình SQL Server trên máy của bạn, hãy cập nhật giá trị `Data Source`:
> - **SQL Server mặc định (Localhost):**  
>   `Data Source=localhost;Initial Catalog=QL_TuiXach;Integrated Security=True;Trust Server Certificate=True`
> - **SQL Server Express:**  
>   `Data Source=.\\SQLEXPRESS;Initial Catalog=QL_TuiXach;Integrated Security=True;Trust Server Certificate=True`
> - **SQL Server với tài khoản sa:**  
>   `Server=localhost;Database=QL_TuiXach;User Id=sa;Password=MậtKhẩuCủaBạn;TrustServerCertificate=True`
> - **LocalDB:**  
>   `Server=(localdb)\\mssqllocaldb;Database=QL_TuiXach;Trusted_Connection=True;MultipleActiveResultSets=true`

### ⚡ Cơ Chế Tự Động Tạo Bảng & Nạp Dữ Liệu (Auto Migration & Seed Data)
Khi ứng dụng khởi chạy ở môi trường **Development**, `DbInitializer.InitializeAsync()` sẽ:
1. Tự động áp dụng EF Core Migrations để tạo CSDL `QL_TuiXach` và toàn bộ các bảng nếu chưa có.
2. Tự động nạp sẵn dữ liệu mẫu (Seeder):
   - 1 tài khoản Quản trị viên (`admin`)
   - 5 tài khoản Khách hàng mẫu
   - 5 Thương hiệu túi xách (`CHARLES & KEITH`, `Pedro`, `ELLY`, `Natoli`, `Jamlos`)
   - 6 Loại túi (`Balo`, `Túi đeo chéo`, `Túi tote`, `Túi dự tiệc`, `Túi đeo vai`, `Clutch`)
   - 5 Chương trình khuyến mãi
   - 15 Sản phẩm túi xách hoàn chỉnh kèm 45 góc ảnh sản phẩm

*(Bạn cũng có thể tham khảo file tài liệu báo cáo tại `Docs/Dac_Ta_Co_So_Du_Lieu_QL_TuiXach.docx` và file script SQL tại `Docs/quan_ly_tui_xach.sql`).*

---

## 🚀 Hướng Dẫn Chạy Dự Án

### Cách 1: Sử dụng file `run.bat` (Nhanh nhất)
1. Mở thư mục `DoAnLTW/` (hoặc thư mục gốc).
2. Nhấn đúp chuột vào file **`run.bat`**.
3. Hệ thống sẽ tự động build, khởi chạy Web Server và tự động mở trình duyệt tại:  
   👉 **`http://localhost:5226`**

### Cách 2: Sử dụng .NET CLI
Mở terminal/PowerShell tại thư mục dự án và chạy:
```bash
cd DoAnLTW/DoAnLTW
dotnet run
```
Sau đó truy cập: `http://localhost:5226`

### Cách 3: Sử dụng Visual Studio 2022
1. Mở file solution: `DoAnLTW/DoAnLTW.sln`.
2. Kiểm tra chuỗi kết nối trong `appsettings.json`.
3. Nhấn **F5** (hoặc **Ctrl + F5**) để khởi chạy.

---

## 👥 Danh Sách Tài Khoản Mẫu (Tài Khoản Test)

Hệ thống đã nạp sẵn các tài khoản demo sau để thuận tiện cho việc chấm bài và kiểm thử:

### 1. Phân Hệ Quản Trị (Admin)
- **Đường dẫn đăng nhập:** `/Account/Login` (sau khi đăng nhập có thể vào `/Admin`)
- **Tên đăng nhập:** `admin`
- **Mật khẩu:** `123456`
- **Vai trò:** `Người bán` (Admin)
- **Quyền hạn:** Truy cập trang Dashboard quản trị, quản lý sản phẩm, thương hiệu, danh mục, khuyến mãi và xử lý đơn hàng.

### 2. Phân Hệ Khách Hàng (Customer)
| STT | Tên đăng nhập | Mật khẩu | Họ và tên | Email | Địa chỉ |
|:---:|:---|:---:|:---|:---|:---|
| 1 | `minhquan` | `123456` | Nguyễn Nhật Minh Quân | minhquan@gmail.com | TP. Hồ Chí Minh |
| 2 | `ngocanh` | `123456` | Trần Ngọc Anh | ngocanh@gmail.com | Bình Dương |
| 3 | `thanhhoa` | `123456` | Lê Thanh Hòa | thanhhoa@gmail.com | Đồng Nai |
| 4 | `quanghuy` | `123456` | Phạm Quang Huy | quanghuy@gmail.com | Long An |
| 5 | `thimai` | `123456` | Võ Thị Mai | thimai@gmail.com | TP. Hồ Chí Minh |

---

## 🌟 Các Phân Hệ & Chức Năng Chính

### 1. Khu Vực Khách Hàng (Client)
- **Trang chủ (`/`):** Hero banner slider, Flash Sale đếm ngược, sản phẩm bán chạy, thương hiệu nổi bật.
- **Danh mục & Tìm kiếm (`/Catalog`):** Lọc theo thương hiệu, loại túi, mức giá; sắp xếp theo giá tăng/giảm, tên; phân trang; xem chi tiết sản phẩm và ảnh phóng to.
- **Flash Sale (`/FlashSale`):** Các sản phẩm đang được áp dụng khuyến mãi lớn.
- **Giỏ hàng (`/Cart`):** Thêm vào giỏ, cập nhật số lượng (kiểm tra tồn kho và validate số lượng > 0), xóa sản phẩm, tính tổng tiền tự động.
- **Đặt hàng (`/Orders/Checkout`):** Điền thông tin giao hàng, chọn phương thức thanh toán, hoàn tất đơn hàng.
- **Lịch sử đơn hàng (`/Orders/History`):** Xem danh sách đơn đã đặt và chi tiết trạng thái từng đơn hàng.
- **Tài khoản cá nhân (`/Account`):** Đăng nhập, đăng ký tài khoản mới, cập nhật hồ sơ cá nhân, đổi mật khẩu, quên mật khẩu.

### 2. Phân Hệ Quản Trị (Admin Area - `/Admin`)
*(Tích hợp giao diện và tài nguyên từ template admin4)*
- **Dashboard (`/Admin/Home`):** Thống kê tổng doanh thu, số đơn hàng, tổng sản phẩm và biểu đồ hoạt động.
- **Quản lý sản phẩm (`/Admin/Product`):** Xem danh sách, thêm sản phẩm mới (kèm upload hình ảnh), chỉnh sửa thông tin, cập nhật khuyến mãi cho sản phẩm, xóa sản phẩm.
- **Quản lý danh mục & thương hiệu (`/Admin/Category`):** Thêm, sửa danh mục loại túi và danh sách thương hiệu đối tác.
- **Quản lý khuyến mãi (`/Admin/Promotion`):** Tạo và quản lý các chương trình ưu đãi, phần trăm giảm giá và thời gian áp dụng.
- **Quản lý đơn hàng (`/Admin/AdminOrder`):** Danh sách đơn đặt hàng, chi tiết đơn hàng, cập nhật trạng thái đơn (Chờ xử lý, Đang giao, Đã giao, Đã hủy).
