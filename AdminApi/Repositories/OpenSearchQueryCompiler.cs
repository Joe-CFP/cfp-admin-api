using AdminApi.Services;
using OpenSearch.Client;

namespace AdminApi.Repositories;

public interface IOpenSearchQueryCompiler
{
    QueryContainer Compile(QueryExpression expressionTree, IReadOnlyList<string> defaultFields);
}

public class OpenSearchQueryCompiler : IOpenSearchQueryCompiler
{
    public QueryContainer Compile(QueryExpression expressionTree, IReadOnlyList<string> defaultFields)
    {
        defaultFields ??= Array.Empty<string>();
        return CompileExpression(expressionTree, defaultFields);
    }

    private static QueryContainer CompileExpression(QueryExpression expression, IReadOnlyList<string> defaultFields)
    {
        return expression switch {
            QueryTermExpression term => CompileTerm(term, defaultFields),
            QueryNotExpression not => new BoolQuery { MustNot = new[] { CompileExpression(not.Operand, defaultFields) } },
            QueryAndExpression and => new BoolQuery { Must = new[] { CompileExpression(and.Left, defaultFields), CompileExpression(and.Right, defaultFields) } },
            QueryOrExpression or => new BoolQuery { Should = new[] { CompileExpression(or.Left, defaultFields), CompileExpression(or.Right, defaultFields) } },
            _ => new BoolQuery()
        };
    }

    private static QueryContainer CompileTerm(QueryTermExpression term, IReadOnlyList<string> defaultFields)
    {
        string[] fields = ResolveFields(term.FieldSelectors, defaultFields);

        var query = new MultiMatchQuery
        {
            Query = term.Text,
            Analyzer = "my_analyzer2",
            Type = term.IsQuoted ? TextQueryType.Phrase : TextQueryType.PhrasePrefix,
            Fields = fields
        };

        if (!term.IsQuoted)
            query.MaxExpansions = 200;

        return query;
    }

    private static string[] ResolveFields(IReadOnlyList<string>? selectors, IReadOnlyList<string> defaultFields)
    {
        if (selectors == null || selectors.Count == 0)
            return defaultFields.ToArray();

        var mapped = new List<string>();

        foreach (string selector in selectors)
        {
            string field = MapShortField(selector);
            if (!string.IsNullOrWhiteSpace(field))
                mapped.Add(field);
        }

        string[] distinct = mapped.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (distinct.Length == 0)
            return defaultFields.ToArray();

        return distinct;
    }

    private static string MapShortField(string code)
    {
        string key = (code ?? "").Trim().ToLowerInvariant();

        return key switch {
            "pu" => "publisher",
            "lo" => "location",
            "su" => "summary",
            "cp" => "cpvdesc",
            "ti" => "reftitleshort",
            "aw" => "awardedtofirstlines",
            "rl" => "procedurelanguagescodes",
            _ => ""
        };
    }
}
