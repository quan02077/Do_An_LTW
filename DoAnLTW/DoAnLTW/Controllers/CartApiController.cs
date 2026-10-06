using DoAnLTW.Services;
using DoAnLTW.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartApiController(ICartService cartService, ICurrentCustomerService currentCustomer) : ControllerBase
{
    [HttpGet]
    public async Task<CartViewModel> Get(CancellationToken ct) =>
        await cartService.GetAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), ct);

    [HttpPost]
    public async Task<CartViewModel> Add(AddCartItemViewModel request, CancellationToken ct) =>
        await cartService.AddAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), request, ct);

    [HttpPut("{maTui:guid}")]
    public async Task<CartViewModel> Update(Guid maTui, UpdateCartItemViewModel request, CancellationToken ct) =>
        await cartService.UpdateAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), maTui, request, ct);

    [HttpDelete("{maTui:guid}")]
    public async Task<CartViewModel> Remove(Guid maTui, CancellationToken ct) =>
        await cartService.RemoveAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), maTui, ct);
}
