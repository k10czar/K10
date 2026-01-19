using System.Collections.Generic;

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

        public static string SafeSummary(this IReadOnlyList<IContentEditorInfo> summarizables, string plural)
        {
            return summarizables.Count switch
            {
                0 => ConstsK10.NULL_STRING_COLORED,
                1 => summarizables[0].SafeSummary(),
                2 => $"{summarizables[0].SafeSummary()} & {summarizables[1].SafeSummary()}",
                _ => $"{summarizables.Count} {plural}"
            };
        }
    }
}