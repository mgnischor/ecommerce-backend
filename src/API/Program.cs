using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(
        "/docs",
        (options) =>
        {
            options.HideSearch = false;
            options.ShowSidebar = true;
            options.Theme = ScalarTheme.DeepSpace;
            options.Title = "E-Commerce API";
            options.WithDirectDocumentDownload();
            options.WithJsonDocumentDownload();
            options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
            options.WithYamlDocumentDownload();
        }
    );
}

app.UseAuthorization();
app.MapControllers();
app.Run();
