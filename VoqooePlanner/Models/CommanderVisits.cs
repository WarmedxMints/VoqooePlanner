namespace VoqooePlanner.Models
{
    public class CommanderVisits
    {
        public CommanderVisits(string name)
        {
            Name = name;
        }
        public string Name { get; }
        public int Visits {  get; }
    }
}