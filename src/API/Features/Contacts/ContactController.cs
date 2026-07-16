using System.Security.Claims;
using Application.DataTransferObjects.Pagination;
using Application.Features.Contacts;
using Application.Features.Contacts.DataTransferObjects.Requests;
using API.DataTransferObjects.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Contacts;

[ApiController]
[Authorize]
[Route("api/contacts")]
public sealed class ContactController(IContactService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] CursorPaginationRequest pagination, CancellationToken cancellationToken) =>
        Ok(Result.Success(await service.GetContactsAsync(UserId, pagination, cancellationToken)));

    [HttpPost]
    public async Task<IActionResult> Add(AddContactRequest request, CancellationToken cancellationToken)
    {
        var contact = await service.AddContactAsync(UserId, request, cancellationToken);
        return contact is null
            ? BadRequest(Result.Fail("User topilmadi, o'zingizni yoki mavjud kontaktni qo'shib bo'lmaydi."))
            : Created($"api/contacts/{contact.Id}", Result.Success(contact));
    }

    [HttpDelete("{contactId:int}")]
    public async Task<IActionResult> Delete(int contactId, CancellationToken cancellationToken) =>
        await service.DeleteContactAsync(UserId, contactId, cancellationToken)
            ? Ok(Result.Success())
            : NotFound(Result.Fail("Kontakt topilmadi"));

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
