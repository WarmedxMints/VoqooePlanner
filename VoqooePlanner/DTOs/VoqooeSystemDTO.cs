using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace VoqooePlanner.DTOs
{
    public sealed class VoqooeSystemDTO : IEqualityComparer<VoqooeSystemDTO>, IEquatable<VoqooeSystemDTO>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Address { get; set; }
        public string Name { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public bool Visited { get; set; }
        public bool ContainsELW { get; set; }
        public int StarType { get; set; }
        public int Value { get; set; }

        public ICollection<JournalCommanderDTO> CommanderVisits { get; set; } = [];

        public bool Equals(VoqooeSystemDTO? other)
        {
            if (other is null) return false;

            if (other.Address != Address) return false;

            return Visited == other.Visited && ContainsELW == other.ContainsELW && Value == other.Value;
        }

        public bool Equals(VoqooeSystemDTO? x, VoqooeSystemDTO? y)
        {
            if(x == null) return false;
           
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] VoqooeSystemDTO obj)
        {
            return obj.Address.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as VoqooeSystemDTO);
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }
    }
}
