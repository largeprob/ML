using LargeProb.Core;

namespace LargeProb.ML.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.AddControllers()
                .AppSettingsConfig()
                .AddGlobalFilter()
                .AddSwaggerGen("LargeProb.ML.Api.xml", false);


            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DisplayRequestDuration();
                c.SwaggerEndpoint($"/swagger/��̨�ӿ�/swagger.json", $"��̨�ӿ�");
            });

            app.UseHttpsRedirection();
            app.MapControllers();
            app.Run();
        }
    }
}
