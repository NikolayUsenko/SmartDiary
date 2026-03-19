using Microsoft.EntityFrameworkCore;

namespace SmartDiary.Web.Models
{
    [PrimaryKey(nameof(TaskId), nameof(TagId))] // Составной первичный ключ
    public class TaskTag
    {
        public int TaskId { get; set; }
        public int TagId { get; set; }
        // Навигационные свойства
        public Task Task { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}