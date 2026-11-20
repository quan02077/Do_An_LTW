using DoAnLTW.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.ViewComponents;

// Mục hiển thị loại túi trên mega menu
public record MenuLoaiTuiItem(int MaLoai, string TenLoai);

// Mục hiển thị thương hiệu trên mega menu
public record MenuThuongHieuItem(int MaThuongHieu, string TenThuongHieu);

// Dữ liệu hiển thị cho menu 2 cấp động
public record MenuViewModel(
    IReadOnlyList<MenuLoaiTuiItem> LoaiTuis,
    IReadOnlyList<MenuThuongHieuItem> ThuongHieus);

// ViewComponent đọc danh sách loại túi và thương hiệu từ CSDL cho header
public class MenuViewComponent(DbContext_TuiXach db) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var ct = HttpContext?.RequestAborted ?? CancellationToken.None;

        var loaiTuis = await db.LoaiTuis
            .AsNoTracking()
            .OrderBy(l => l.tenLoai)
            .Select(l => new MenuLoaiTuiItem(l.maLoai, l.tenLoai))
            .ToListAsync(ct);

        var thuongHieus = await db.ThuongHieus
            .AsNoTracking()
            .OrderBy(t => t.tenThuongHieu)
            .Select(t => new MenuThuongHieuItem(t.maThuongHieu, t.tenThuongHieu))
            .ToListAsync(ct);

        return View(new MenuViewModel(loaiTuis, thuongHieus));
    }
}
