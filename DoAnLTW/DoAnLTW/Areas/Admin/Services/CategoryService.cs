using DoAnLTW.Data;
using DoAnLTW.ViewModels;
using DoAnLTW.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Areas.Admin.Services
{
    public interface ICategoryService
    {
        Task<CategoryIndexViewModel> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TypeEditViewModel?> GetLoaiTuiByIdAsync(int maLoai, CancellationToken cancellationToken = default);
        Task<BrandEditViewModel?> GetThuongHieuByIdAsync(int maThuongHieu, CancellationToken cancellationToken = default);
        Task<TypeViewModel> AddLoaiTuiAsync(TypeCreateViewModel loaiTui, CancellationToken cancellationToken = default);
        Task<BrandViewModel> AddThuongHieuAsync(BrandCreateViewModel thuongHieu, CancellationToken cancellationToken = default);
        Task<TypeViewModel> UpdateLoaiTuiAsync(int maLoai, TypeEditViewModel loaiTui, CancellationToken cancellationToken = default);
        Task<BrandViewModel> UpdateThuongHieuAsync(int maThuongHieu, BrandEditViewModel thuongHieu, CancellationToken cancellationToken = default);
        Task<bool> DeleteLoaiTuiAsync(int maLoai, CancellationToken cancellationToken = default);
        Task<bool> DeleteThuongHieuAsync(int maThuongHieu, CancellationToken cancellationToken = default);
    }
    public class CategoryService(DbContext_TuiXach db) : ICategoryService
    {
        public async Task<TypeViewModel> AddLoaiTuiAsync(TypeCreateViewModel loaiTui, CancellationToken cancellationToken = default)
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

        public async Task<BrandViewModel> AddThuongHieuAsync(BrandCreateViewModel thuongHieu, CancellationToken cancellationToken = default)
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

        public async Task<bool> DeleteLoaiTuiAsync(int maLoai, CancellationToken cancellationToken = default)
        {
            var loaiTui = await db.LoaiTuis
                .Include(lt => lt.TuiXachs)
                .FirstOrDefaultAsync(lt => lt.maLoai == maLoai, cancellationToken);

            if (loaiTui == null)
            {
                return false;
            }

            if (loaiTui.TuiXachs.Any())
            {
                return false;
            }

            db.LoaiTuis.Remove(loaiTui);
            await db.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> DeleteThuongHieuAsync(int maThuongHieu, CancellationToken cancellationToken = default)
        {
            var thuongHieu = await db.ThuongHieus
                .Include(th => th.TuiXachs)
                .FirstOrDefaultAsync(th => th.maThuongHieu == maThuongHieu, cancellationToken);

            if (thuongHieu == null)
            {
                return false;
            }
            if (thuongHieu.TuiXachs.Any())
            {
                return false;
            }

            db.ThuongHieus.Remove(thuongHieu);
            await db.SaveChangesAsync(cancellationToken);

            return true;
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

        public async Task<TypeEditViewModel?> GetLoaiTuiByIdAsync(int maLoai, CancellationToken cancellationToken = default)
        {
            var loaiTui = await db.LoaiTuis
                .Include(lt => lt.TuiXachs)
                .FirstOrDefaultAsync(lt => lt.maLoai == maLoai, cancellationToken);

            if (loaiTui == null)
            {
                return null;
            }

            return new TypeEditViewModel
            {
                MaLoai = loaiTui.maLoai,
                TenLoai = loaiTui.tenLoai,
                SoLuongSanPham = loaiTui.TuiXachs.Count
            };
        }

        public async Task<BrandEditViewModel?> GetThuongHieuByIdAsync(int maThuongHieu, CancellationToken cancellationToken = default)
        {
            var thuongHieu = await db.ThuongHieus
                .Include(th => th.TuiXachs)
                .FirstOrDefaultAsync(th => th.maThuongHieu == maThuongHieu, cancellationToken);

            if (thuongHieu == null)
            {
                return null;
            }

            return new BrandEditViewModel
            {
                MaThuongHieu = thuongHieu.maThuongHieu,
                TenThuongHieu = thuongHieu.tenThuongHieu,
                SoLuongSanPham = thuongHieu.TuiXachs.Count
            };
        }

        public async Task<TypeViewModel> UpdateLoaiTuiAsync(int maLoai, TypeEditViewModel loaiTui, CancellationToken cancellationToken = default)
        {
            var existingLoaiTui = await db.LoaiTuis
                .Include(lt => lt.TuiXachs)
                .FirstOrDefaultAsync(lt => lt.maLoai == maLoai, cancellationToken);

            if (existingLoaiTui == null)
            {
                throw new Exception($"Không tìm thấy loại túi với mã {maLoai}");
            }
            existingLoaiTui.tenLoai = loaiTui.TenLoai;

            await db.SaveChangesAsync(cancellationToken);

            return new TypeViewModel
            {
                MaLoai = existingLoaiTui.maLoai,
                TenLoai = existingLoaiTui.tenLoai,
                SoLuongSanPham = existingLoaiTui.TuiXachs.Count
            };
        }

        public async Task<BrandViewModel> UpdateThuongHieuAsync(int maThuongHieu, BrandEditViewModel thuongHieu, CancellationToken cancellationToken = default)
        {
            var existingThuongHieu = await db.ThuongHieus
                .Include(th => th.TuiXachs)
                .FirstOrDefaultAsync(th => th.maThuongHieu == maThuongHieu, cancellationToken);

            if (existingThuongHieu == null)
            {
                throw new Exception($"Không tìm thấy thương hiệu với mã {maThuongHieu}");
            }

            existingThuongHieu.tenThuongHieu = thuongHieu.TenThuongHieu;

            await db.SaveChangesAsync(cancellationToken);

            return new BrandViewModel
            {
                MaThuongHieu = existingThuongHieu.maThuongHieu,
                TenThuongHieu = existingThuongHieu.tenThuongHieu,
                SoLuongSanPham = existingThuongHieu.TuiXachs.Count
            };
        }
    }
}
