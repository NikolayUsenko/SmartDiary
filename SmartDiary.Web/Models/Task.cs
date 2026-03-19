using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDiary.Web.Models
{
    public class Task
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Заголовок задачи обязателен")]
        [StringLength(200, ErrorMessage = "Заголовок не может превышать 200 символов")]
        public string? Title { get; set; }
        public string? Description { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [DataType(DataType.DateTime)]
        [CustomValidation(typeof(Task), nameof(ValidateDeadline))]
        public DateTime? Deadline { get; set; }
        [Required]
        public string Status { get; set; } = "New"; // New, InProgress, Completed
        [Required]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High
                                                         // Внешние ключи
        public int? ProjectId { get; set; }
        public int UserId { get; set; }
        // Навигационные свойства
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        // Связь многие-ко-многим с Tag

        public DateTime UpdatedAt { get; set; }

        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
        // Кастомная валидация для дедлайна
        public static ValidationResult? ValidateDeadline(DateTime? deadline, ValidationContext context)
        {
            // Получаем текущий экземпляр задачи, чтобы иметь доступ к полю CreatedAt
            var taskInstance = (Task)context.ObjectInstance;

            // 1. Проверка: если дедлайн указан
            if (deadline.HasValue)
            {
                // Проверка, что дедлайн не в прошлом (относительно текущего времени)
                if (deadline.Value < DateTime.UtcNow)
                {
                    return new ValidationResult("Дедлайн не может быть в прошлом (относительно текущей даты).");
                }

                // 2. РАСШИРЕННАЯ ПРОВЕРКА: Дедлайн не может быть раньше даты создания задачи.
                // Сравниваем только по дате (без времени) или полностью? Лучше сравнивать полностью,
                // но для наглядности сравниваем даты, чтобы нельзя было создать задачу вчера с дедлайном позавчера.
                if (deadline.Value.Date < taskInstance.CreatedAt.Date)
                {
                    return new ValidationResult("Дедлайн не может быть раньше даты создания задачи.");
                }
            }

            return ValidationResult.Success;
        }
    }
}