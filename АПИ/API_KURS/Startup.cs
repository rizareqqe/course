using API_KURS.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using System;
using System.Data.Common;
using Пример_1._1.Contexts;

namespace API_KURS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<DatabaseContext>(options =>
                options.UseMySql(
                    connectionString,
                    mySqlOptions => mySqlOptions.ServerVersion(ServerVersion.AutoDetect(connectionString))));

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerFeature>();
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Message = "Внутренняя ошибка сервера",
                        Details = env.IsDevelopment() ? feature?.Error.Message : "Проверьте доступность базы данных и корректность данных."
                    });
                });
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            SeedDatabase(app);
        }

        private static void SeedDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            context.Database.ExecuteSqlRaw(@"
CREATE TABLE IF NOT EXISTS user_accounts (
    user_id INT PRIMARY KEY AUTO_INCREMENT,
    login VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(100) NOT NULL,
    full_name VARCHAR(150) NOT NULL,
    role VARCHAR(30) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1
);");

            EnsureCuratorColumnExists(context);
            EnsureCuratorConstraintExists(context);

            context.Database.ExecuteSqlRaw(@"
INSERT INTO user_accounts (login, password, full_name, role, is_active)
SELECT 'admin', 'admin123', 'Системный администратор', 'Administrator', 1
WHERE NOT EXISTS (SELECT 1 FROM user_accounts WHERE login = 'admin');");

            context.Database.ExecuteSqlRaw(@"
INSERT INTO user_accounts (login, password, full_name, role, is_active)
SELECT 'editor', 'editor123', 'Контент-менеджер фильмотеки', 'CatalogEditor', 1
WHERE NOT EXISTS (SELECT 1 FROM user_accounts WHERE login = 'editor');");

            context.Database.ExecuteSqlRaw(@"
UPDATE movies
SET curator_user_id = (SELECT user_id FROM user_accounts WHERE login = 'editor' LIMIT 1)
WHERE curator_user_id IS NULL;");
        }

        private static void EnsureCuratorColumnExists(DatabaseContext context)
        {
            const string checkSql = @"
SELECT COUNT(*)
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'movies'
  AND COLUMN_NAME = 'curator_user_id';";

            if (ExecuteScalar(context, checkSql) == 0)
            {
                context.Database.ExecuteSqlRaw("ALTER TABLE movies ADD COLUMN curator_user_id INT NULL;");
            }
        }

        private static void EnsureCuratorConstraintExists(DatabaseContext context)
        {
            const string checkSql = @"
SELECT COUNT(*)
FROM information_schema.TABLE_CONSTRAINTS
WHERE CONSTRAINT_SCHEMA = DATABASE()
  AND TABLE_NAME = 'movies'
  AND CONSTRAINT_NAME = 'fk_movies_curator_user';";

            if (ExecuteScalar(context, checkSql) == 0)
            {
                context.Database.ExecuteSqlRaw(@"
ALTER TABLE movies
ADD CONSTRAINT fk_movies_curator_user
FOREIGN KEY (curator_user_id) REFERENCES user_accounts(user_id)
ON DELETE SET NULL
ON UPDATE CASCADE;");
            }
        }

        private static long ExecuteScalar(DatabaseContext context, string sql)
        {
            using DbCommand command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            if (command.Connection!.State != System.Data.ConnectionState.Open)
            {
                command.Connection.Open();
            }

            var result = command.ExecuteScalar();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt64(result);
        }
    }
}
