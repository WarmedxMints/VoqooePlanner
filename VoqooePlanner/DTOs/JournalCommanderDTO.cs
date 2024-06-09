using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace VoqooePlanner.DTOs
{
    [Index(nameof(Name), IsUnique = true)]
    public sealed class JournalCommanderDTO
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string JournalDir { get; set; } = string.Empty;
        public string LastFile { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
    }
}
