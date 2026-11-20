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
        // trả về số sản phẩm thực sự được áp dụng (mã túi không tồn tại bị bỏ qua)
        Task<int> AddAsync(PromotionFormViewModel model, CancellationToken cancellationToken = default);
        // null = không tìm thấy khuyến mãi
        Task<int?> UpdateAsync(int maKM, PromotionFormViewModel model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int maKM, CancellationToken cancellationToken = default);
        Task PopulateProductOptionsAsync(PromotionFormViewModel model, CancellationToken cancellationToken = default);
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
                    TrangThai = km.trangThai ? "Hoạt động" : "Ngừng hoạt động",
                    SoSanPham = km.TuiXachs.Count
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<PromotionFormViewModel?> GetByIdAsync(int maKM, CancellationToken cancellationToken = default)
        {
            var model = await db.KhuyenMais
                .Where(km => km.maKM == maKM)
                .Select(km => new PromotionFormViewModel
                {
                    MaKM = km.maKM,
                    TenKM = km.tenKM,
                    PhanTramGiam = (int)km.phanTramGiam,
                    NgayBatDau = km.ngayBatDau,
                    NgayKetThuc = km.ngayKetThuc,
                    TrangThai = km.trangThai,
                    // tick sẵn những túi đang thuộc khuyến mãi này
                    SelectedMaTui = km.TuiXachs.Select(t => t.maTui).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (model != null)
            {
                await PopulateProductOptionsAsync(model, cancellationToken);
            }
            return model;
        }

        public async Task<int> AddAsync(PromotionFormViewModel model, CancellationToken cancellationToken = default)
        {
            var promotion = new KhuyenMai
            {
                tenKM = model.TenKM,
                phanTramGiam = model.PhanTramGiam,
                ngayBatDau = model.NgayBatDau,
                ngayKetThuc = model.NgayKetThuc,
                trangThai = model.TrangThai
            };

            // khuyến mãi mới chưa có maKM -> gán qua navigation, EF tự điền maKM sau khi INSERT;
            // khuyến mãi và việc gán sản phẩm được lưu trong cùng một lần SaveChanges (một transaction).
            var selectedBags = await LoadSelectedBagsAsync(model.SelectedMaTui, cancellationToken);
            foreach (var bag in selectedBags)
            {
                bag.KhuyenMai = promotion;
            }

            db.KhuyenMais.Add(promotion);
            await db.SaveChangesAsync(cancellationToken);
            return selectedBags.Count;
        }

        public async Task<int?> UpdateAsync(int maKM, PromotionFormViewModel model, CancellationToken cancellationToken = default)
        {
            var promotion = await db.KhuyenMais.FirstOrDefaultAsync(km => km.maKM == maKM, cancellationToken);
            if (promotion == null)
            {
                return null;
            }

            promotion.tenKM = model.TenKM;
            promotion.phanTramGiam = model.PhanTramGiam;
            promotion.ngayBatDau = model.NgayBatDau;
            promotion.ngayKetThuc = model.NgayKetThuc;
            promotion.trangThai = model.TrangThai;

            var selectedIds = model.SelectedMaTui.Distinct().ToList();

            // túi đang thuộc khuyến mãi này nhưng bị bỏ tick -> gỡ khuyến mãi
            var removedBags = await db.TuiXachs
                .Where(t => t.maKM == maKM && !selectedIds.Contains(t.maTui))
                .ToListAsync(cancellationToken);
            foreach (var bag in removedBags)
            {
                bag.maKM = null;
            }

            // túi được tick -> gán khuyến mãi này (kể cả túi đang thuộc khuyến mãi khác: chuyển sang)
            var selectedBags = await LoadSelectedBagsAsync(selectedIds, cancellationToken);
            foreach (var bag in selectedBags)
            {
                bag.maKM = maKM;
            }

            // một lần SaveChanges = một transaction: thông tin khuyến mãi và danh sách sản phẩm cùng thành công hoặc cùng thất bại
            await db.SaveChangesAsync(cancellationToken);
            return selectedBags.Count;
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

        // nạp danh sách túi xách cho checkbox; gọi lại mỗi khi hiển thị form (kể cả khi validate lỗi)
        public async Task PopulateProductOptionsAsync(PromotionFormViewModel model, CancellationToken cancellationToken = default)
        {
            model.SanPhamOptions = await db.TuiXachs
                .AsNoTracking()
                .OrderBy(t => t.maTui)
                .Select(t => new PromotionProductOption
                {
                    MaTui = t.maTui,
                    TenTui = t.tenTui,
                    DonGia = t.donGia,
                    HinhAnh = t.HinhAnhTuis.Where(h => h.laAnhChinh).Select(h => h.tenHinhAnh).FirstOrDefault(),
                    MaKMHienTai = t.maKM,
                    TenKMHienTai = t.KhuyenMai != null ? t.KhuyenMai.tenKM : null
                })
                .ToListAsync(cancellationToken);
        }

        // chỉ lấy những mã túi thật sự tồn tại; mã lạ gửi từ form bị bỏ qua
        private async Task<List<TuiXach>> LoadSelectedBagsAsync(IEnumerable<int> selectedMaTui, CancellationToken cancellationToken)
        {
            var ids = selectedMaTui.Distinct().ToList();
            if (ids.Count == 0) return new List<TuiXach>();

            return await db.TuiXachs.Where(t => ids.Contains(t.maTui)).ToListAsync(cancellationToken);
        }
    }
}
