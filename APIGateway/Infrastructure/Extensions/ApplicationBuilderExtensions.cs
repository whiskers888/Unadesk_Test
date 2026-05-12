namespace Unadesk_Test.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{

    public static void ConfigureSwagger(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}