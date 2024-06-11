using VoqooePlanner.DTOs;
using VoqooePlanner.ViewModels.ModelViews;

namespace VoqooePlanner.Models
{
    public sealed class VoqooeSystem
    {
        public VoqooeSystem(VoqooeSystemDTO dto)
        {
            Address = dto.Address;
            Name = dto.Name;
            X = dto.X;
            Y = dto.Y;
            Z = dto.Z;
            Visited = dto.Visited;
            ContainsELW = dto.ContainsELW;
            StarType = dto.StarType;
            Value = dto.Value;
        }

        public VoqooeSystem(JournalSystemViewModel journalSystem)
        {
            Name = journalSystem.Name;
            X = journalSystem.Pos.X;
            Y = journalSystem.Pos.Y;
            Z = journalSystem.Pos.Z;
        }

        public VoqooeSystem(long address, string name, double x, double y, double z, bool visited, bool containsELW, int starType, int value)
        {
            Address = address;
            Name = name;
            X = x;
            Y = y;
            Z = z;
            Visited = visited;
            ContainsELW = containsELW;
            StarType = starType;
            Value = value;
        }

        public long Address { get; }
        public string Name { get; }
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public bool Visited { get; }
        public bool ContainsELW { get; }
        public int StarType { get; }
        public int Value { get; }
        public bool UserVisited { get; set; }
        public double Distance { get; set; }
    }
}