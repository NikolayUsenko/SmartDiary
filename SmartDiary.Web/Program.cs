using Microsoft.EntityFrameworkCore;
using SmartDiary.Web.Data;
using SmartDiary.Web.Models;
using Task = SmartDiary.Web.Models.Task;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

SeedData(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

static void SeedData(IServiceProvider serviceProvider)
{
    using (var scope = serviceProvider.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Применяем миграции, если БД не создана
            context.Database.Migrate();

            // Проверяем, есть ли уже данные
            if (context.Users.Any())
            {
                return; // Данные уже есть
            }

            // НАЧИНАЕМ ТРАНЗАКЦИЮ
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // Создаем тестового пользователя
                    var user = new User
                    {
                        Username = "testuser",
                        Email = "test@example.com",
                        PasswordHash = "AQAAAAIAAYagAAAAE..." // Здесь будет хэш пароля, пока заглушка 
                    };
                    context.Users.Add(user);
                    context.SaveChanges(); // Сохраняем, чтобы получить Id пользователя

                    // Создаем проекты
                    var projects = new[]
                    {
                        new Project { Name = "Личные дела", Description = "Личные задачи", Color = "FF5733", OwnerId = user.Id },
                        new Project { Name = "Работа", Description = "Рабочие задачи", Color = "33FF57", OwnerId = user.Id },
                        new Project { Name = "Учеба", Description = "Учебные задачи", Color = "3357FF", OwnerId = user.Id }
                    };
                    context.Projects.AddRange(projects);
                    context.SaveChanges();

                    // Создаем теги
                    var tags = new[]
                    {
                        new Tag { Name = "Важное", OwnerId = user.Id },
                        new Tag { Name = "Срочное", OwnerId = user.Id },
                        new Tag { Name = "Идея", OwnerId = user.Id }
                    };
                    context.Tags.AddRange(tags);
                    context.SaveChanges();

                    // Создаем задачи
                    var tasks = new[]
                    {
                        new Task
                        {
                            Title = "Купить продукты",
                            Description = "Молоко, хлеб, яйца",
                            Status = "New",
                            Priority = "Medium",
                            UserId = user.Id,
                            ProjectId = projects[0].Id,
                            Deadline = DateTime.UtcNow.AddDays(1)
                        },
                        new Task
                        {
                            Title = "Сдать отчет",
                            Description = "Подготовить квартальный отчет",
                            Status = "InProgress",
                            Priority = "High",
                            UserId = user.Id,
                            ProjectId = projects[1].Id,
                            Deadline = DateTime.UtcNow.AddHours(5)
                        },
                        new Task
                        {
                            Title = "Прочитать книгу",
                            Description = "Глава 3",
                            Status = "New",
                            Priority = "Low",
                            UserId = user.Id,
                            ProjectId = projects[2].Id,
                            Deadline = null
                        }
                    };
                    context.Tasks.AddRange(tasks);
                    context.SaveChanges();

                    // Связываем задачи с тегами
                    var taskTags = new[]
                    {
                        new TaskTag { TaskId = tasks[0].Id, TagId = tags[1].Id }, // Продукты - Срочное
                        new TaskTag { TaskId = tasks[1].Id, TagId = tags[0].Id }, // Отчет - Важное
                        new TaskTag { TaskId = tasks[1].Id, TagId = tags[1].Id }, // Отчет - Срочное
                        new TaskTag { TaskId = tasks[2].Id, TagId = tags[2].Id }  // Книга - Идея
                    };
                    context.TaskTags.AddRange(taskTags);
                    context.SaveChanges();

                    // ЕСЛИ ВСЕ УСПЕШНО - ПОДТВЕРЖДАЕМ ТРАНЗАКЦИЮ
                    transaction.Commit();

                    Console.WriteLine("Seed data successfully added with transaction!");
                }
                catch (Exception)
                {
                    // ЕСЛИ ОШИБКА - ОТКАТЫВАЕМ ТРАНЗАКЦИЮ
                    transaction.Rollback();
                    Console.WriteLine("Transaction rolled back due to error!");
                    throw; // Пробрасываем исключение дальше
                }
            }
        }
        catch (Exception ex)
        {
            // Логируем ошибку
            Console.WriteLine($"Error during database seeding: {ex.Message}");
            throw;
        }
    }
}

app.Run();
