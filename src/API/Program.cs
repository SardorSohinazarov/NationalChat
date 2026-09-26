using API.Extensions;
using API.Hubs;
using API.Middleware;
using API.Options;

// Ba'zi konteyner muhitlarida (masalan Render) IPv6 yo'nalishi yo'q, shu sabab tashqi
// xostlarga (masalan smtp.gmail.com) IPv6 orqali ulanish "Network is unreachable" bilan
// yiqiladi. IPv6'ni o'chirib, faqat IPv4 orqali ulanishga majburlaymiz.
AppContext.SetSwitch("System.Net.DisableIPv6", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices(builder.Configuration, builder.Environment);

var app = builder.Build();

if (!app.Environment.IsDevelopment() && !app.Services.GetRequiredService<ClientOriginOptions>().IsRestricted)
{
    app.Logger.LogWarning(
        "Cors:AllowedOrigins sozlanmagan: har qanday sayt API'ni chaqira oladi va refresh cookie boshqa saytdan ishlamaydi " +
        "(foydalanuvchi access token muddati tugashi bilan qayta kirishi kerak bo'ladi). Web klient manzilini sozlang.");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
