namespace VoqooePlanner.Models
{
    [Flags]
    public enum NearBySystemsOptions
    {
        None = 0,
        ExcludeVisited = 1 << 0,
        ExcludeUserVisited = 1 << 1,
        IncludeUnvisitedELWs = 1 << 2,
        IncludeUnvisitedValue = 1 << 3,
    }
}
