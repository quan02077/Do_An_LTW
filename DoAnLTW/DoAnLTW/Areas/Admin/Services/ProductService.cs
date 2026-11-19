using System.Text.RegularExpressions;
using DoAnLTW.Data;
using DoAnLTW.ViewModels;
using DoAnLTW.Models;
using DoAnLTW.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoAnLTW.Areas.Admin.Services
{
    public interface IProductService
    {
        Task<ProductIndexViewModel> GetAllAsync(string? keyword, CancellationToken cancellationToken = default);
        Task<ProductFormViewModel?> GetByIdAsync(int maTui, CancellationToken cancellationToken = default);
        Task<ServiceResult> AddAsync(ProductFormViewModel model, CancellationToken cancellationToken = default);
        Task<ServiceResult> UpdateAsync(int maTui, ProductFormViewModel model, CancellationToken cancellationToken = default);
        Task<ServiceResult> DeleteAsync(int maTui, CancellationToken cancellationToken = default);
        Task PopulateFormOptionsAsync(ProductFormViewModel model, CancellationToken cancellationToken = default);
    }

    public class ProductService(DbContext_TuiXach db, IWebHostEnvironment env) : IProductService
    {
        private const string ImageFolder = "HinhAnh";
        private const long MaxImageBytes = 2 * 1024 * 1024; // 2MB
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

        // ảnh do trang quản trị tải lên luôn có tên bắt đầu bằng Guid 32 ký tự hex.
        // chỉ những file này mới được xóa khỏi ổ đĩa, ảnh mẫu (tui1_1.png...) dùng chung trong repo được giữ lại.
        private static readonly Regex UploadedFileName = new("^[0-9a-f]{32}", RegexOptions.Compiled);

        // 1. danh sách sản phẩm (có tìm theo tên)
        public async Task<ProductIndexViewModel> GetAllAsync(string? keyword, CancellationToken cancellationToken = default)
        {
            var query = db.TuiXachs.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(p => p.tenTui.Contains(kw));
            }

            var items = await query
                .OrderBy(p => p.maTui)
                .Select(p => new ProductViewModel
            {
                MaTui = p.maTui,
                TenTui = p.tenTui,
                TenLoai = p.LoaiTui != null ? p.LoaiTui.tenLoai : "",
                TenThuongHieu = p.ThuongHieu != null ? p.ThuongHieu.tenThuongHieu : "",
                DonGia = p.donGia,
                SoLuong = p.soLuong,
                Badge = p.badge,
                HinhAnhChinh = p.HinhAnhTuis.Where(i => i.laAnhChinh).Select(i => i.tenHinhAnh).FirstOrDefault()
            }).ToListAsync(cancellationToken);

            return new ProductIndexViewModel
            {
                SanPhams = items,
                Keyword = keyword
            };
        }

        // 2. lấy thông tin sản phẩm để sửa
        public async Task<ProductFormViewModel?> GetByIdAsync(int maTui, CancellationToken cancellationToken = default)
        {
            return await db.TuiXachs
                .Where(p => p.maTui == maTui)
                .Select(p => new ProductFormViewModel
                {
                    MaTui = p.maTui,
                    TenTui = p.tenTui,
                    MaLoai = p.maLoai,
                    MaThuongHieu = p.maThuongHieu,
                    DonGia = p.donGia,
                    SoLuong = p.soLuong,
                    MoTa = p.moTa,
                    Badge = p.badge,
                    HinhAnhHienTai = p.HinhAnhTuis.Where(i => i.laAnhChinh).Select(i => i.tenHinhAnh).FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        // 3. thêm sản phẩm mới
        public async Task<ServiceResult> AddAsync(ProductFormViewModel model, CancellationToken cancellationToken = default)
        {
            // kiểm tra ảnh TRƯỚC khi ghi CSDL để không tạo sản phẩm "mồ côi" khi ảnh không hợp lệ.
            var imageError = ValidateImage(model.FileHinhAnh);
            if (imageError != null) return ServiceResult.Fail(imageError, nameof(model.FileHinhAnh));

            var p = new TuiXach
            {
                tenTui = model.TenTui.Trim(),
                maLoai = model.MaLoai,
                maThuongHieu = model.MaThuongHieu,
                donGia = model.DonGia,
                soLuong = model.SoLuong,
                moTa = model.MoTa,
                badge = model.Badge
            };

            string? savedFile = null;
            if (HasFile(model.FileHinhAnh))
            {
                savedFile = await SaveImageAsync(model.FileHinhAnh!, cancellationToken);
                p.HinhAnhTuis.Add(new HinhAnhTui { tenHinhAnh = savedFile, laAnhChinh = true, thuTu = 1 });
            }

            try
            {
                db.TuiXachs.Add(p);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // ghi CSDL thất bại thì dọn file vừa lưu để không để rác trên ổ đĩa.
                DeleteUploadedImage(savedFile);
                throw;
            }

            return ServiceResult.Ok();
        }

        // 4. cập nhật sản phẩm
        public async Task<ServiceResult> UpdateAsync(int maTui, ProductFormViewModel model, CancellationToken cancellationToken = default)
        {
            var imageError = ValidateImage(model.FileHinhAnh);
            if (imageError != null) return ServiceResult.Fail(imageError, nameof(model.FileHinhAnh));

            var p = await db.TuiXachs.Include(x => x.HinhAnhTuis).FirstOrDefaultAsync(x => x.maTui == maTui, cancellationToken);
            if (p == null) return ServiceResult.Fail("Không tìm thấy sản phẩm.");

            p.tenTui = model.TenTui.Trim();
            p.maLoai = model.MaLoai;
            p.maThuongHieu = model.MaThuongHieu;
            p.donGia = model.DonGia;
            p.soLuong = model.SoLuong;
            p.moTa = model.MoTa;
            p.badge = model.Badge;

            string? savedFile = null;
            string? oldFile = null;
            if (HasFile(model.FileHinhAnh))
            {
                savedFile = await SaveImageAsync(model.FileHinhAnh!, cancellationToken);

                var img = p.HinhAnhTuis.FirstOrDefault(i => i.laAnhChinh);
                if (img != null)
                {
                    oldFile = img.tenHinhAnh;
                    img.tenHinhAnh = savedFile;
                }
                else
                {
                    p.HinhAnhTuis.Add(new HinhAnhTui { maTui = p.maTui, tenHinhAnh = savedFile, laAnhChinh = true, thuTu = 1 });
                }
            }

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                DeleteUploadedImage(savedFile);
                throw;
            }

            // chỉ xóa ảnh cũ SAU khi CSDL đã lưu thành công tên ảnh mới.
            DeleteUploadedImage(oldFile);
            return ServiceResult.Ok();
        }

        // 5. xóa sản phẩm
        public async Task<ServiceResult> DeleteAsync(int maTui, CancellationToken cancellationToken = default)
        {
            var p = await db.TuiXachs.Include(x => x.HinhAnhTuis).FirstOrDefaultAsync(x => x.maTui == maTui, cancellationToken);
            if (p == null) return ServiceResult.Fail("Không tìm thấy sản phẩm.");

            // chi tiết đơn hàng là chứng từ bán hàng: không được xóa, nên sản phẩm cũng không được xóa
            // (nếu xóa sẽ vi phạm khóa ngoại ChiTietDonHang -> TuiXach).
            if (await db.ChiTietDonHangs.AnyAsync(x => x.maTui == maTui, cancellationToken))
            {
                return ServiceResult.Fail("Sản phẩm đã phát sinh đơn hàng, không thể xóa.");
            }

            var imageFiles = p.HinhAnhTuis.Select(i => i.tenHinhAnh).ToList();

            // giỏ hàng chỉ là dữ liệu tạm: gỡ sản phẩm khỏi mọi giỏ rồi mới xóa sản phẩm.
            var cartItems = await db.ChiTietGioHangs.Where(x => x.maTui == maTui).ToListAsync(cancellationToken);
            db.ChiTietGioHangs.RemoveRange(cartItems);
            db.HinhAnhTuis.RemoveRange(p.HinhAnhTuis);
            db.TuiXachs.Remove(p);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var file in imageFiles)
            {
                DeleteUploadedImage(file);
            }

            return ServiceResult.Ok();
        }

        // 6. nap danh sach loai tui va thuong hieu cho form them / sua
        public async Task PopulateFormOptionsAsync(ProductFormViewModel model, CancellationToken cancellationToken = default)
        {
            model.LoaiTuiOptions = await db.LoaiTuis
                .Select(lt => new SelectListItem { Value = lt.maLoai.ToString(), Text = lt.tenLoai })
                .ToListAsync(cancellationToken);

            model.ThuongHieuOptions = await db.ThuongHieus
                .Select(th => new SelectListItem { Value = th.maThuongHieu.ToString(), Text = th.tenThuongHieu })
                .ToListAsync(cancellationToken);
        }

        private static bool HasFile(IFormFile? file) => file != null && file.Length > 0;

        // trả về thông báo lỗi, hoặc null nếu ảnh hợp lệ (không chọn ảnh cũng là hợp lệ).
        private static string? ValidateImage(IFormFile? file)
        {
            if (!HasFile(file)) return null;

            var extension = Path.GetExtension(file!.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return "Chỉ chấp nhận ảnh định dạng .jpg, .jpeg, .png hoặc .webp.";
            }

            if (file.Length > MaxImageBytes)
            {
                return "Dung lượng ảnh tối đa là 2MB.";
            }

            return null;
        }

        // lưu ảnh với tên mới = Guid + đuôi file; không dùng FileName gốc của client
        // (tên gốc có thể chứa ký tự lạ hoặc "../" để ghi file ra ngoài thư mục).
        private async Task<string> SaveImageAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var folder = Path.Combine(env.WebRootPath, ImageFolder);
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
            var path = Path.Combine(folder, fileName);

            await using var stream = new FileStream(path, FileMode.CreateNew);
            await file.CopyToAsync(stream, cancellationToken);
            return fileName;
        }

        private void DeleteUploadedImage(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !UploadedFileName.IsMatch(fileName)) return;

            var folder = Path.GetFullPath(Path.Combine(env.WebRootPath, ImageFolder)) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(Path.Combine(folder, fileName));

            // chặn xóa file nằm ngoài wwwroot/HinhAnh
            if (fullPath.StartsWith(folder, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
