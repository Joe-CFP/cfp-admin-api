namespace AdminApi.Services;

public enum QueryTokenKind
{
    Term = 1,
    And = 2,
    Or = 3,
    Not = 4,
    LeftParen = 5,
    RightParen = 6
}

public readonly record struct QueryToken(QueryTokenKind Kind, string Text, bool IsQuoted);

public abstract record QueryExpression;

public sealed record QueryTermExpression(string Text, bool IsQuoted, IReadOnlyList<string>? FieldSelectors) : QueryExpression;

public sealed record QueryNotExpression(QueryExpression Operand) : QueryExpression;

public sealed record QueryAndExpression(QueryExpression Left, QueryExpression Right) : QueryExpression;

public sealed record QueryOrExpression(QueryExpression Left, QueryExpression Right) : QueryExpression;

public sealed record ParsedQuery(string OriginalText, IReadOnlyList<QueryToken> Tokens, QueryExpression? ExpressionTree);

public interface IQueryParsingService
{
    ParsedQuery Parse(string queryText);
}

public class QueryParsingService : IQueryParsingService
{
    public ParsedQuery Parse(string queryText)
    {
        queryText ??= "";
        List<QueryToken> tokens = InsertImplicitOrs(Tokenize(queryText));
        QueryExpression? expressionTree = ParseExpressionTree(tokens);
        return new ParsedQuery(queryText, tokens, expressionTree);
    }

    private static List<QueryToken> Tokenize(string input)
    {
        var tokens = new List<QueryToken>();
        int index = 0;

        while (index < input.Length)
        {
            char c = input[index];

            if (char.IsWhiteSpace(c))
            {
                index++;
                continue;
            }

            if (c == '(')
            {
                tokens.Add(new QueryToken(QueryTokenKind.LeftParen, "(", false));
                index++;
                continue;
            }

            if (c == ')')
            {
                tokens.Add(new QueryToken(QueryTokenKind.RightParen, ")", false));
                index++;
                continue;
            }

            if (c == '"')
            {
                index++;
                int start = index;

                while (index < input.Length && input[index] != '"')
                    index++;

                string phrase = input.Substring(start, index - start);
                tokens.Add(new QueryToken(QueryTokenKind.Term, phrase, true));

                if (index < input.Length && input[index] == '"')
                    index++;

                continue;
            }

            int wordStart = index;

            while (index < input.Length && !char.IsWhiteSpace(input[index]) && input[index] != '(' && input[index] != ')')
                index++;

            string word = input.Substring(wordStart, index - wordStart);

            if (TryGetOperatorKind(word, out QueryTokenKind operatorKind))
                tokens.Add(new QueryToken(operatorKind, operatorKind.ToString().ToUpperInvariant(), false));
            else
                tokens.Add(new QueryToken(QueryTokenKind.Term, word, false));
        }

        return tokens;
    }

    private static bool TryGetOperatorKind(string text, out QueryTokenKind kind)
    {
        kind = QueryTokenKind.Term;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (text.Equals("AND", StringComparison.OrdinalIgnoreCase))
        {
            kind = QueryTokenKind.And;
            return true;
        }

        if (text.Equals("OR", StringComparison.OrdinalIgnoreCase))
        {
            kind = QueryTokenKind.Or;
            return true;
        }

        if (text.Equals("NOT", StringComparison.OrdinalIgnoreCase))
        {
            kind = QueryTokenKind.Not;
            return true;
        }

        return false;
    }

    private static List<QueryToken> InsertImplicitOrs(List<QueryToken> tokens)
    {
        if (tokens.Count <= 1)
            return tokens;

        var result = new List<QueryToken>(tokens.Count * 2);

        for (int i = 0; i < tokens.Count; i++)
        {
            QueryToken current = tokens[i];
            result.Add(current);

            if (i == tokens.Count - 1)
                break;

            QueryToken next = tokens[i + 1];

            if (NeedsImplicitOr(current, next))
                result.Add(new QueryToken(QueryTokenKind.Or, "OR", false));
        }

        return result;
    }

    private static bool NeedsImplicitOr(QueryToken left, QueryToken right)
    {
        bool leftIsValue = left.Kind == QueryTokenKind.Term || left.Kind == QueryTokenKind.RightParen;
        bool rightIsValue = right.Kind == QueryTokenKind.Term || right.Kind == QueryTokenKind.LeftParen;

        if (!leftIsValue)
            return false;

        if (!rightIsValue)
            return false;

        return true;
    }

    private static QueryExpression? ParseExpressionTree(List<QueryToken> tokens)
    {
        List<QueryToken> postfix = ToPostfix(tokens);
        if (postfix.Count == 0)
            return null;

        var expressionStack = new Stack<QueryExpression>();

        foreach (QueryToken token in postfix)
        {
            if (token.Kind == QueryTokenKind.Term)
            {
                expressionStack.Push(ParseTermExpression(token));
                continue;
            }

            if (token.Kind == QueryTokenKind.Not)
            {
                if (expressionStack.Count < 1)
                    return null;

                QueryExpression operand = expressionStack.Pop();
                expressionStack.Push(new QueryNotExpression(operand));
                continue;
            }

            if (token.Kind == QueryTokenKind.And)
            {
                if (expressionStack.Count < 2)
                    return null;

                QueryExpression right = expressionStack.Pop();
                QueryExpression left = expressionStack.Pop();
                expressionStack.Push(new QueryAndExpression(left, right));
                continue;
            }

            if (token.Kind == QueryTokenKind.Or)
            {
                if (expressionStack.Count < 2)
                    return null;

                QueryExpression right = expressionStack.Pop();
                QueryExpression left = expressionStack.Pop();
                expressionStack.Push(new QueryOrExpression(left, right));
                continue;
            }

            return null;
        }

        if (expressionStack.Count != 1)
            return null;

        return expressionStack.Pop();
    }

    private static QueryTermExpression ParseTermExpression(QueryToken token)
    {
        string raw = token.Text ?? "";
        string text = raw;
        IReadOnlyList<string>? selectors = null;

        int pipeIndex = raw.IndexOf('|');
        if (pipeIndex >= 0)
        {
            text = raw.Substring(0, pipeIndex);
            string right = raw.Substring(pipeIndex + 1);

            string[] parts = right.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 0)
                selectors = parts;
        }

        return new QueryTermExpression(text.Trim(), token.IsQuoted, selectors);
    }

    private static List<QueryToken> ToPostfix(List<QueryToken> tokens)
    {
        var output = new List<QueryToken>();
        var operatorStack = new Stack<QueryToken>();

        foreach (QueryToken token in tokens)
        {
            if (token.Kind == QueryTokenKind.Term)
            {
                output.Add(token);
                continue;
            }

            if (token.Kind == QueryTokenKind.LeftParen)
            {
                operatorStack.Push(token);
                continue;
            }

            if (token.Kind == QueryTokenKind.RightParen)
            {
                while (operatorStack.Count > 0 && operatorStack.Peek().Kind != QueryTokenKind.LeftParen)
                    output.Add(operatorStack.Pop());

                if (operatorStack.Count == 0 || operatorStack.Peek().Kind != QueryTokenKind.LeftParen)
                    return new List<QueryToken>();

                operatorStack.Pop();
                continue;
            }

            if (IsOperator(token.Kind))
            {
                while (operatorStack.Count > 0 && IsOperator(operatorStack.Peek().Kind))
                {
                    QueryTokenKind topKind = operatorStack.Peek().Kind;
                    if (ShouldPopOperator(topKind, token.Kind))
                        output.Add(operatorStack.Pop());
                    else
                        break;
                }

                operatorStack.Push(token);
                continue;
            }

            return new List<QueryToken>();
        }

        while (operatorStack.Count > 0)
        {
            QueryToken op = operatorStack.Pop();
            if (op.Kind == QueryTokenKind.LeftParen || op.Kind == QueryTokenKind.RightParen)
                return new List<QueryToken>();

            output.Add(op);
        }

        return output;
    }

    private static bool IsOperator(QueryTokenKind kind)
    {
        return kind == QueryTokenKind.Not || kind == QueryTokenKind.And || kind == QueryTokenKind.Or;
    }

    private static bool ShouldPopOperator(QueryTokenKind stackTop, QueryTokenKind incoming)
    {
        int stackPrecedence = GetPrecedence(stackTop);
        int incomingPrecedence = GetPrecedence(incoming);

        if (incoming == QueryTokenKind.Not)
            return stackPrecedence > incomingPrecedence;

        return stackPrecedence >= incomingPrecedence;
    }

    private static int GetPrecedence(QueryTokenKind kind)
    {
        return kind switch {
            QueryTokenKind.Not => 3,
            QueryTokenKind.And => 2,
            QueryTokenKind.Or => 1,
            _ => 0
        };
    }
}
