namespace RAXY.Utility.Editor.Hub
{
    /// <summary>
    /// Contract for pluggable tabs inside the RAXY Project Hub window.
    /// Implement in each package's Editor assembly; the Hub discovers modules via TypeCache.
    /// </summary>
    public interface IRaxyHubModule
    {
        string Id { get; }
        string DisplayName { get; }
        int Order { get; }

        void OnEnable();
        void OnDisable();
        void OnGUI();
    }
}
