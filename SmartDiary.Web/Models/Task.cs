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
        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
        // Кастомная валидация для дедлайна
        public static ValidationResult? ValidateDeadline(DateTime? deadline,
        ValidationContext context)
        {
            if (deadline.HasValue && deadline.Value < DateTime.UtcNow.Date)
            {
                return new ValidationResult("Дедлайн не может быть в прошлом");
            }
            return ValidationResult.Success;
        }
    }
}