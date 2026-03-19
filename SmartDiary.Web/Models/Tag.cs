using System.ComponentModel.DataAnnotations;

namespace SmartDiary.Web.Models
{
    public class Tag
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Название тега обязательно")]
        [StringLength(50, ErrorMessage = "Название тега не может превышать 50 символов")]
        public string? Name { get; set; }
        // Внешний ключ
        public int OwnerId { get; set; }
        // Навигационные свойства
        public User? Owner { get; set; }
        // Связь многие-ко-многим с Task
        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }
}