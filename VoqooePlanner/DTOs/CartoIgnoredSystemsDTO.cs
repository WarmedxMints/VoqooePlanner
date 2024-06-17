using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoqooePlanner.DTOs
{
    public sealed class CartoIgnoredSystemsDTO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Address { get; set; }
        public required string Name { get; set; }
        public List<JournalCommanderDTO> Commanders { get; set; } = [];
    }
}
