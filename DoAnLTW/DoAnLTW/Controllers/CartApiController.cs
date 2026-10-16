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

    [HttpPut("{maTui:int}")]
    public async Task<CartViewModel> Update(int maTui, UpdateCartItemViewModel request, CancellationToken ct) =>
        await cartService.UpdateAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), maTui, request, ct);

    [HttpDelete("{maTui:int}")]
    public async Task<CartViewModel> Remove(int maTui, CancellationToken ct) =>
        await cartService.RemoveAsync(await currentCustomer.GetRequiredCustomerIdAsync(ct), maTui, ct);
}
