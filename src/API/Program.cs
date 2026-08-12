using API.Extensions;
using API.Hubs;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
