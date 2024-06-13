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

    public enum OrganicScanState
    {
        None = 0,
        Analysed = 1,
        Sold = 2
    }

    public enum ExoBiologyViewState
    {
        Loading = -1,
        CheckList = 0,
        UnSoldList = 1
    }
}
