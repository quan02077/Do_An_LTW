using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Areas.Admin.Controllers;

// trang hộp thư hỗ trợ khách hàng. Kế thừa AdminBaseController nên chỉ vai trò Người bán vào được;
// dữ liệu và tin nhắn thời gian thực do SupportChatHub và SupportChatApiController phục vụ.
public class SupportChatController : AdminBaseController
{
    // GET: /Admin/SupportChat
    public IActionResult Index() => View();
}
