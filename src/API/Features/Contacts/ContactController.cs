using System.Security.Claims;
using Application.Features.Contacts;
using Application.Features.Contacts.DataTransferObjects.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Contacts;

[ApiController]
[Authorize]
[Route("api/contacts")]
public sealed class ContactController(IContactService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(await service.GetContactsAsync(UserId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Add(AddContactRequest request, CancellationToken cancellationToken)
    {
        var contact = await service.AddContactAsync(UserId, request, cancellationToken);
        return contact is null ? BadRequest(new { error = "User topilmadi, o'zingizni yoki mavjud kontaktni qo'shib bo'lmaydi." }) : Created($"api/contacts/{contact.Id}", contact);
    }

    [HttpDelete("{contactId:int}")]
    public async Task<IActionResult> Delete(int contactId, CancellationToken cancellationToken) =>
        await service.DeleteContactAsync(UserId, contactId, cancellationToken) ? NoContent() : NotFound();

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
