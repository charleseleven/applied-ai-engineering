using System.Text.RegularExpressions;

namespace GmudAutomation.Core.Services;

/// <summary>
/// US 1.2 (Task 3): reconhece as tags padrão do time (API, WEB, Database) em nomes de projeto
/// mencionados no texto de um comentário, ex: "Contoso.PortalCliente.API".
/// </summary>
public sealed partial class ProjectTagExtractor : IProjectTagExtractor
{
    [GeneratedRegex(@"\b[A-Za-z0-9]+(?:\.[A-Za-z0-9]+)*\.(?:API|WEB|Database)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ProjectTagRegex();

    public IReadOnlyList<string> ExtractProjectTags(string? commentText)
    {
        if (string.IsNullOrWhiteSpace(commentText))
        {
            return Array.Empty<string>();
        }

        var matches = ProjectTagRegex().Matches(commentText);
        if (matches.Count == 0)
        {
            return Array.Empty<string>();
        }

        var tags = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            tags.Add(match.Value);
        }

        return tags;
    }
}
