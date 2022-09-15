using System.Collections.Immutable;
using System.Text;

namespace RV.Chess.PGN
{
    internal class Parser
    {
        private static ISet<char> _allowedSanChars = new HashSet<char>("PNBRQKabcdefgh12345678x=O-#+");

        private readonly string _text;
        private int _position;
        private readonly ImmutableArray<Token> _tokens;
        private int _nodeTextStart;

        internal Parser(string text, ImmutableArray<Token> tokens)
        {
            _text = text;
            _tokens = tokens;
        }

        internal ImmutableArray<SyntaxNode> Parse()
        {
            var nodes = new List<SyntaxNode>();

            while (_position < _tokens.Length)
            {
                var node = ReadNode();

                if (node is not InvalidSyntax)
                {
                    nodes.Add(node);
                }
            }

            return nodes.ToImmutableArray();
        }

        private SyntaxNode ReadNode()
        {
            _nodeTextStart = Current.Start;

            switch (Current.Kind)
            {
                case TokenKind.SquareBracketOpen:
                    return ParseTagPair();
                case TokenKind.Integer:
                    return ParseMoveNumber();
                case TokenKind.CurlyBracketOpen:
                    return ParseBraceComment();
                case TokenKind.Asterisk:
                case TokenKind.GameTerminator:
                    return ParseGameTerminator();
                case TokenKind.Symbol:
                    return ParseSAN();
                case TokenKind.ParenthesisOpen:
                    return ParseRAV();
                default:
                    if (Current.Kind == TokenKind.Semicolon
                        && (_position == 0 || Peek(-1).Kind == TokenKind.NewLine))
                    {
                        return ParseRestOfLineComment();
                    }
                    else
                    {
                        _position++;
                    }
                    break;
            }

            return new InvalidSyntax();
        }

        private RAVSyntax ParseRAV()
        {
            var variation = new List<Token>();
            var openParenCount = 0;

            _position++;

            while (_position < _tokens.Length
                && (openParenCount > 0 || Current.Kind != TokenKind.ParenthesisClose))
            {
                if (Current.Kind == TokenKind.ParenthesisOpen)
                {
                    openParenCount++;
                }
                else if (Current.Kind == TokenKind.ParenthesisClose)
                {
                    openParenCount--;
                }

                if (Current.Kind == TokenKind.EndOfFile)
                {
                    throw new InvalidPgnException("Unterminated recursive variation", _nodeTextStart, _text);
                }

                variation.Add(Current);
                _position++;
            }

            var text = _text[(_nodeTextStart + 1)..Current.Start];
            var parser = new Parser(_text, variation.ToImmutableArray());
            var items = parser.Parse();

            _position++;

            return new RAVSyntax(text, items);
        }

        private static bool IsValidSan(string san)
        {
            if (san.Length < 2 || san.Length > 7)
            {
                return false;
            }

            for (var i = 0; i < san.Length; i++)
            {
                if (!_allowedSanChars.Contains(san[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private SANSyntax ParseSAN()
        {
            var san = Current.Value as string ?? string.Empty;
            var isValid = IsValidSan(san);

            if (Current.Value != null && isValid)
            {
                if (Lookahead.Kind == TokenKind.Annotation)
                {
                    var node = new SANSyntax(Current.Text, (string)Current.Value, (string)Lookahead.Value!);
                    _position += 2;
                    return node;
                }
                else
                {
                    var node = new SANSyntax(Current.Text, (string)Current.Value);
                    _position++;
                    return node;
                }
            }

            throw new InvalidPgnException("Invalid SAN", Current.Start, _text);
        }

        private CommentSyntax ParseBraceComment()
        {
            _position++;
            var sb = new StringBuilder();

            while (Current.Kind != TokenKind.CurlyBracketClose && Current.Kind != TokenKind.EndOfFile)
            {
                sb.Append(Current.Value);
                _position++;
            }

            return new CommentSyntax(_text[_nodeTextStart..Current.End], sb.ToString(), CommentSyntax.CommentType.Brace);
        }

        private CommentSyntax ParseRestOfLineComment()
        {
            _position++;
            var sb = new StringBuilder();

            while (Current.Kind != TokenKind.NewLine && Current.Kind != TokenKind.EndOfFile)
            {
                sb.Append(Current.Text);
                _position++;
            }

            return new CommentSyntax(_text[_nodeTextStart..Current.End], sb.ToString(), CommentSyntax.CommentType.RestOfLine);
        }

        private GameTerminatorSyntax ParseGameTerminator()
        {
            if (Current.Kind == TokenKind.Asterisk)
            {
                _position++;
                return new GameTerminatorSyntax("*", GameResult.Unknown);
            }

            if (Current.Kind == TokenKind.GameTerminator && Current.Value != null)
            {
                var value = (string)Current.Value;
                switch (value)
                {
                    case "1-0":
                        _position++;
                        return new GameTerminatorSyntax(value, GameResult.White);
                    case "0-1":
                        _position++;
                        return new GameTerminatorSyntax(value, GameResult.Black);
                    case "1/2-1/2":
                        _position++;
                        return new GameTerminatorSyntax(value, GameResult.Tie);
                }
            }

            throw new InvalidPgnException("Invalid game terminator", _nodeTextStart, _text);
        }

        private MoveNumberSyntax ParseMoveNumber()
        {
            var number = (int)(Current.Value ?? 0);
            var end = Current.End;
            _position++;

            while (Current.Kind == TokenKind.Period)
            {
                end = Current.End;
                _position++;
            }

            return new MoveNumberSyntax(number, _text[_nodeTextStart..end]);
        }

        private TagPairSyntax ParseTagPair()
        {
            _position++;
            SkipWhitespace();
            var tagKey = MatchToken(TokenKind.Symbol);
            SkipWhitespace();
            var tagValue = MatchToken(TokenKind.String).Value?.ToString() ?? string.Empty;
            SkipWhitespace();
            var endBracket = MatchToken(TokenKind.SquareBracketClose);

            return new TagPairSyntax(tagKey.Text, tagValue, _text[_nodeTextStart..endBracket.End]);
        }

        private Token Current => Peek(0);

        private Token Lookahead => Peek(1);

        private void SkipWhitespace()
        {
            while (Current.Kind == TokenKind.Whitespace)
            {
                _position++;
            }
        }

        private Token Peek(int offset)
        {
            var idx = _position + offset;

            if (idx >= _tokens.Length)
            {
                return _tokens[_tokens.Length - 1];
            }

            return _tokens[idx];
        }

        private Token ReadTokenAndMove()
        {
            var c = Current;
            _position++;
            return c;
        }

        private Token MatchToken(TokenKind kind)
        {
            if (Current.Kind == kind)
            {
                return ReadTokenAndMove();
            }

            throw new InvalidPgnException("Invalid comment syntax", Current.Start, _text);
        }

        private Token MatchToken(TokenKind kind, string text)
        {
            if (Current.Kind == kind && Current.Text == text)
            {
                return ReadTokenAndMove();
            }

            throw new InvalidPgnException("Invalid comment syntax", Current.Start, _text);
        }
    }
}
