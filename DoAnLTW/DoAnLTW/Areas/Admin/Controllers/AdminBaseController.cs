using DoAnLTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers;

// lớp cha cho mọi controller quản trị: [Area] và [Authorize] được kế thừa xuống lớp con,
// nên controller mới thêm vào area Admin chỉ cần kế thừa lớp này là tự được bảo vệ.
[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
public abstract class AdminBaseController : Controller
{
}
