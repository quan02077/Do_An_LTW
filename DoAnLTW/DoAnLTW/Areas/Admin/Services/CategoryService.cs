using DoAnLTW.Data;
using DoAnLTW.ViewModels;
using DoAnLTW.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Areas.Admin.Services
{
    public interface ICategoryService
    {
        Task<CategoryIndexViewModel> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TypeViewModel> CreateLoaiTuiAsync(TypeCreateViewModel loaiTui, CancellationToken cancellationToken = default);
        Task<BrandViewModel> CreateThuongHieuAsync(BrandCreateViewModel thuongHieu, CancellationToken cancellationToken = default);
        Task<TypeViewModel> UpdateLoaiTuiAsync(int maLoai, TypeEditViewModel loaiTui, CancellationToken cancellationToken = default);
        Task<BrandViewModel> UpdateThuongHieuAsync(int maThuongHieu, BrandEditViewModel thuongHieu, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int maLoai, CancellationToken cancellationToken = default);
    }
    public class CategoryService(DbContext_TuiXach db) : ICategoryService
    {
        public async Task<TypeViewModel> CreateLoaiTuiAsync(TypeCreateViewModel loaiTui, CancellationToken cancellationToken = default)
        {
            var newLoaiTui = new LoaiTui
            {
                tenLoai = loaiTui.TenLoai
            };

            db.LoaiTuis.Add(newLoaiTui);

            await db.SaveChangesAsync(cancellationToken);

            var loaiTuiDTO = new TypeViewModel
            {
                MaLoai = newLoaiTui.maLoai,
                TenLoai = newLoaiTui.tenLoai,
                SoLuongSanPham = 0 
            };

            return loaiTuiDTO;
        }

        public async Task<BrandViewModel> CreateThuongHieuAsync(BrandCreateViewModel thuongHieu, CancellationToken cancellationToken = default)
        {
            var newThuongHieu = new ThuongHieu
            {
                tenThuongHieu = thuongHieu.TenThuongHieu
            };

            db.ThuongHieus.Add(newThuongHieu);

            await db.SaveChangesAsync(cancellationToken);

            var thuongHieuDTO = new BrandViewModel
            {
                MaThuongHieu = newThuongHieu.maThuongHieu,
                TenThuongHieu = newThuongHieu.tenThuongHieu,
                SoLuongSanPham = 0
            };

            return thuongHieuDTO;
        }

        public async Task<bool> DeleteAsync(int maLoai, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<CategoryIndexViewModel> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cate = new CategoryIndexViewModel
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
            return cate;
        }

        public async Task<TypeViewModel> UpdateLoaiTuiAsync(int maLoai, TypeEditViewModel loaiTui, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<BrandViewModel> UpdateThuongHieuAsync(int maThuongHieu, BrandEditViewModel thuongHieu, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
