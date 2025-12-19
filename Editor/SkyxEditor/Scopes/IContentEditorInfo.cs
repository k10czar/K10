namespace Skyx.SkyxEditor
{
    public interface IContentEditorInfo
    {
        public string ContentName { get; }
        public string ContentSummary { get; }
        public string ContentDescription => null;
        public EColor ContentColor => EColor.Infer;
    }

    public static class IContentFilterExtensions
    {
        public static string SafeSummary(this IContentEditorInfo summarizable) => summarizable?.ContentSummary ?? ConstsK10.NULL_STRING_COLORED;
    }
}