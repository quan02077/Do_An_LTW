using DoAnLTW.Data;
using DoAnLTW.ViewModels;
using DoAnLTW.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Areas.Admin.Services
{
    public interface ICategoryService
    {
        Task<CategoryIndexViewModel> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TypeViewModel?> GetLoaiTuiByIdAsync(int maLoai, CancellationToken cancellationToken = default);
        Task<BrandViewModel?> GetThuongHieuByIdAsync(int maThuongHieu, CancellationToken cancellationToken = default);
        Task<bool> AddLoaiTuiAsync(TypeViewModel loaiTui, CancellationToken cancellationToken = default);
        Task<bool> AddThuongHieuAsync(BrandViewModel thuongHieu, CancellationToken cancellationToken = default);
        Task<bool> UpdateLoaiTuiAsync(int maLoai, TypeViewModel loaiTui, CancellationToken cancellationToken = default);
        Task<bool> UpdateThuongHieuAsync(int maThuongHieu, BrandViewModel thuongHieu, CancellationToken cancellationToken = default);
        Task<bool> DeleteLoaiTuiAsync(int maLoai, CancellationToken cancellationToken = default);
        Task<bool> DeleteThuongHieuAsync(int maThuongHieu, CancellationToken cancellationToken = default);
    }

    public class CategoryService(DbContext_TuiXach db) : ICategoryService
    {
        // 1. lấy toàn bộ danh mục cho trang index
        public async Task<CategoryIndexViewModel> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return new CategoryIndexViewModel
            {
                LoaiTuis = await db.LoaiTuis.Select(lt => new TypeViewModel
                {
                    MaLoai = lt.maLoai,
                    TenLoai = lt.tenLoai,
                    SoLuongSanPham = lt.TuiXachs.Count
                }).ToListAsync(cancellationToken),

                ThuongHieus = await db.ThuongHieus.Select(th => new BrandViewModel
                {
                    MaThuongHieu = th.maThuongHieu,
                    TenThuongHieu = th.tenThuongHieu,
                    SoLuongSanPham = th.TuiXachs.Count
                }).ToListAsync(cancellationToken)
            };
        }

        // 2. lấy chi tiết theo id để đưa lên form sửa
        public async Task<TypeViewModel?> GetLoaiTuiByIdAsync(int maLoai, CancellationToken cancellationToken = default)
        {
            return await db.LoaiTuis
                .Where(lt => lt.maLoai == maLoai)
                .Select(lt => new TypeViewModel
                {
                    MaLoai = lt.maLoai,
                    TenLoai = lt.tenLoai,
                    SoLuongSanPham = lt.TuiXachs.Count
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<BrandViewModel?> GetThuongHieuByIdAsync(int maThuongHieu, CancellationToken cancellationToken = default)
        {
            return await db.ThuongHieus
                .Where(th => th.maThuongHieu == maThuongHieu)
                .Select(th => new BrandViewModel
                {
                    MaThuongHieu = th.maThuongHieu,
                    TenThuongHieu = th.tenThuongHieu,
                    SoLuongSanPham = th.TuiXachs.Count
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        // 3. thêm mới
        public async Task<bool> AddLoaiTuiAsync(TypeViewModel loaiTui, CancellationToken cancellationToken = default)
        {
            db.LoaiTuis.Add(new LoaiTui { tenLoai = loaiTui.TenLoai.Trim() });
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AddThuongHieuAsync(BrandViewModel thuongHieu, CancellationToken cancellationToken = default)
        {
            db.ThuongHieus.Add(new ThuongHieu { tenThuongHieu = thuongHieu.TenThuongHieu.Trim() });
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        // 4. cập nhật
        public async Task<bool> UpdateLoaiTuiAsync(int maLoai, TypeViewModel loaiTui, CancellationToken cancellationToken = default)
        {
            var entity = await db.LoaiTuis.FindAsync([maLoai], cancellationToken);
            if (entity == null) return false;

            entity.tenLoai = loaiTui.TenLoai.Trim();
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateThuongHieuAsync(int maThuongHieu, BrandViewModel thuongHieu, CancellationToken cancellationToken = default)
        {
            var entity = await db.ThuongHieus.FindAsync([maThuongHieu], cancellationToken);
            if (entity == null) return false;

            entity.tenThuongHieu = thuongHieu.TenThuongHieu.Trim();
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        // 5. xóa (kiểm tra an toàn khóa ngoại)
        public async Task<bool> DeleteLoaiTuiAsync(int maLoai, CancellationToken cancellationToken = default)
        {
            var loaiTui = await db.LoaiTuis.FindAsync([maLoai], cancellationToken);
            if (loaiTui == null) return false;

            var coSanPham = await db.TuiXachs.AnyAsync(tx => tx.maLoai == maLoai, cancellationToken);
            if (coSanPham) return false;

            db.LoaiTuis.Remove(loaiTui);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteThuongHieuAsync(int maThuongHieu, CancellationToken cancellationToken = default)
        {
            var thuongHieu = await db.ThuongHieus.FindAsync([maThuongHieu], cancellationToken);
            if (thuongHieu == null) return false;

            var coSanPham = await db.TuiXachs.AnyAsync(tx => tx.maThuongHieu == maThuongHieu, cancellationToken);
            if (coSanPham) return false;

            db.ThuongHieus.Remove(thuongHieu);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
