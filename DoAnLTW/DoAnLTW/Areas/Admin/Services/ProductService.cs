using DoAnLTW.Data;
using DoAnLTW.ViewModels;
using DoAnLTW.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Areas.Admin.Services
{
    public interface IProductService
    {
        Task<List<ProductViewModel>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ProductFormViewModel?> GetByIdAsync(int maTui, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(ProductFormViewModel model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(int maTui, ProductFormViewModel model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int maTui, CancellationToken cancellationToken = default);
    }

    public class ProductService(DbContext_TuiXach db, IWebHostEnvironment env) : IProductService
    {
        // 1. lấy toàn bộ danh sách sản phẩm
        public async Task<List<ProductViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await db.TuiXachs.Select(p => new ProductViewModel
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
        public async Task<bool> AddAsync(ProductFormViewModel model, CancellationToken cancellationToken = default)
        {
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

            db.TuiXachs.Add(p);
            await db.SaveChangesAsync(cancellationToken);

            // lưu ảnh nếu có tải lên
            if (model.FileHinhAnh != null && model.FileHinhAnh.Length > 0)
            {
                var fileName = $"{Guid.NewGuid():N}_{model.FileHinhAnh.FileName}";
                var path = Path.Combine(env.WebRootPath, "HinhAnh", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await model.FileHinhAnh.CopyToAsync(stream);

                db.HinhAnhTuis.Add(new HinhAnhTui { maTui = p.maTui, tenHinhAnh = fileName, laAnhChinh = true });
                await db.SaveChangesAsync(cancellationToken);
            }

            return true;
        }

        // 4. cập nhật sản phẩm
        public async Task<bool> UpdateAsync(int maTui, ProductFormViewModel model, CancellationToken cancellationToken = default)
        {
            var p = await db.TuiXachs.Include(x => x.HinhAnhTuis).FirstOrDefaultAsync(x => x.maTui == maTui, cancellationToken);
            if (p == null) return false;

            p.tenTui = model.TenTui.Trim();
            p.maLoai = model.MaLoai;
            p.maThuongHieu = model.MaThuongHieu;
            p.donGia = model.DonGia;
            p.soLuong = model.SoLuong;
            p.moTa = model.MoTa;
            p.badge = model.Badge;

            // cập nhật ảnh nếu có tải ảnh mới
            if (model.FileHinhAnh != null && model.FileHinhAnh.Length > 0)
            {
                var fileName = $"{Guid.NewGuid():N}_{model.FileHinhAnh.FileName}";
                var path = Path.Combine(env.WebRootPath, "HinhAnh", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await model.FileHinhAnh.CopyToAsync(stream);

                var img = p.HinhAnhTuis.FirstOrDefault(i => i.laAnhChinh);
                if (img != null)
                {
                    img.tenHinhAnh = fileName;
                }
                else
                {
                    p.HinhAnhTuis.Add(new HinhAnhTui { maTui = p.maTui, tenHinhAnh = fileName, laAnhChinh = true });
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        // 5. xóa sản phẩm
        public async Task<bool> DeleteAsync(int maTui, CancellationToken cancellationToken = default)
        {
            var p = await db.TuiXachs.Include(x => x.HinhAnhTuis).FirstOrDefaultAsync(x => x.maTui == maTui, cancellationToken);
            if (p == null) return false;

            // xóa hình ảnh trước rồi xóa túi xách
            db.HinhAnhTuis.RemoveRange(p.HinhAnhTuis);
            db.TuiXachs.Remove(p);
            await db.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
