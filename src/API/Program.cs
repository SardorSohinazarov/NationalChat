using API.Extensions;
using API.Hubs;
using API.Middleware;

// Ba'zi konteyner muhitlarida (masalan Render) IPv6 yo'nalishi yo'q, shu sabab tashqi
// xostlarga (masalan smtp.gmail.com) IPv6 orqali ulanish "Network is unreachable" bilan
// yiqiladi. IPv6'ni o'chirib, faqat IPv4 orqali ulanishga majburlaymiz.
AppContext.SetSwitch("System.Net.DisableIPv6", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
