using DoAnLTW.Areas.Admin.ViewModels;
using DoAnLTW.Data;
using DoAnLTW.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Areas.Admin.Services
{
    public interface IPromotionService
    {
        Task<List<PromotionViewModel>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PromotionFormViewModel?> GetByIdAsync(int maKM, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(PromotionFormViewModel model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(int maKM, PromotionFormViewModel model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int maKM, CancellationToken cancellationToken = default);
    }

    public class PromotionService(DbContext_TuiXach db) : IPromotionService
    {
        public async Task<List<PromotionViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await db.KhuyenMais
                .OrderByDescending(km => km.maKM)
                .Select(km => new PromotionViewModel
                {
                    MaKM = km.maKM,
                    TenKM = km.tenKM,
                    PhanTramGiam = (int)km.phanTramGiam,
                    NgayBatDau = km.ngayBatDau,
                    NgayKetThuc = km.ngayKetThuc,
                    TrangThai = km.trangThai ? "Hoạt động" : "Ngừng hoạt động"
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<PromotionFormViewModel?> GetByIdAsync(int maKM, CancellationToken cancellationToken = default)
        {
            return await db.KhuyenMais
                .Where(km => km.maKM == maKM)
                .Select(km => new PromotionFormViewModel
                {
                    MaKM = km.maKM,
                    TenKM = km.tenKM,
                    PhanTramGiam = (int)km.phanTramGiam,
                    NgayBatDau = km.ngayBatDau,
                    NgayKetThuc = km.ngayKetThuc,
                    TrangThai = km.trangThai
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> AddAsync(PromotionFormViewModel model, CancellationToken cancellationToken = default)
        {
            var promotion = new KhuyenMai
            {
                tenKM = model.TenKM,
                phanTramGiam = model.PhanTramGiam,
                ngayBatDau = model.NgayBatDau,
                ngayKetThuc = model.NgayKetThuc,
                trangThai = model.TrangThai
            };

            db.KhuyenMais.Add(promotion);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(int maKM, PromotionFormViewModel model, CancellationToken cancellationToken = default)
        {
            var promotion = await db.KhuyenMais.FirstOrDefaultAsync(km => km.maKM == maKM, cancellationToken);
            if (promotion == null)
            {
                return false;
            }

            promotion.tenKM = model.TenKM;
            promotion.phanTramGiam = model.PhanTramGiam;
            promotion.ngayBatDau = model.NgayBatDau;
            promotion.ngayKetThuc = model.NgayKetThuc;
            promotion.trangThai = model.TrangThai;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int maKM, CancellationToken cancellationToken = default)
        {
            var promotion = await db.KhuyenMais.FirstOrDefaultAsync(km => km.maKM == maKM, cancellationToken);
            if (promotion == null)
            {
                return false;
            }

            // Gỡ liên kết mã khuyến mãi ở các túi xách đang dùng mã này để tránh lỗi khóa ngoại
            var relatedBags = await db.TuiXachs.Where(t => t.maKM == maKM).ToListAsync(cancellationToken);
            foreach (var bag in relatedBags)
            {
                bag.maKM = null;
            }

            db.KhuyenMais.Remove(promotion);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
