namespace K10.DebugSystem
{
    public interface IContentSummary
    {
        public string Summary { get; }
        public string Description => null;
        public EColor SummaryColor => EColor.Secondary;
    }

    public static class IContentFilterExtensions
    {
        public static string SafeSummary(this IContentSummary summarizable) => summarizable?.Summary ?? ConstsK10.NULL_STRING_COLORED;
    }
}