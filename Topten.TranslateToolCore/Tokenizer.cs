using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TranslateTool
{
    /// <summary>
    /// Token types
    /// </summary>
    public enum Token
    {
        String,
        Identifier,
        Period,
        OpenRound,
        CloseRound,
        Comment,
        Other,
        EOF,
    }

    /// <summary>
    /// Simple tokenizer for C# files that's just smart
    /// enough to parse out strings and a couple of other tokens
    /// </summary>
    public class Tokenizer
    {
        /// <summary>
        ///  Constructs a new tokenizer on a text reader
        /// </summary>
        /// <param name="r">The input text reader</param>
        public Tokenizer(TextReader r)
        {
            _reader = r;
            _char = NextChar();
            NextToken();
        }

        /// <summary>
        /// Gets the current token
        /// </summary>
        public Token Token
        {
            get { return _token; }
        }

        /// <summary>
        /// Gets the line number of the current token
        /// </summary>
        public int TokenLine
        {
            get { return _tokenLine; }
        }

        /// <summary>
        /// Gets the string associated with the current token
        /// </summary>
        public string String
        {
            get { return _sb.ToString(); }
        }

        /// <summary>
        /// Moves to the next token in the input
        /// </summary>
        public void NextToken()
        {
            while (true)
            {
                // Skip white space
                while (char.IsWhiteSpace(_char))
                    NextChar();

                _tokenLine = _lineNumber;

                if (_char == '/')
                {
                    NextChar();
                    if (_char == '/')
                    {
                        NextChar();
                        _sb.Length = 0;

                        // Single line comment
                        while (_char != '\n' && _char != '\0')
                        {
                            _sb.Append(_char);
                            NextChar();
                        }
                        _token = Token.Comment;
                        return;
                    }
                    if (_char == '*')
                    {
                        // Block comment
                        NextChar();

                        _sb.Length = 0;
                        while (_char!='\0')
                        {
                            if (_char == '*')
                            {
                                NextChar();
                                if (_char == '/')
                                {
                                    NextChar();
                                    _token = Token.Comment;
                                    return;
                                }
                                _sb.Append('*');
                            }
                            _sb.Append(_char);
                            NextChar();
                        }

                        continue;
                    }

                    _token = Token.Other;
                    return;
                }

                break;
            }

            if (_char=='@')
            {
                NextChar();
                if (_char == '\"')
                {
                    ParseRawString();
                }
                else
                {
                    _token = Token.Other;
                    return;
                }
            }

            if (Char.IsLetter(_char) || _char == '_')
            {
                // Find end of identifier
                _sb.Length = 0;
                while (Char.IsLetterOrDigit(_char) || _char == '_')
                {
                    _sb.Append(_char);
                    NextChar();
                }
                _token = Token.Identifier;
                return;
            }


            if (_char == '\'')
            {
                ParseString();
                _token = Token.Other;
                return;
            }

            if (_char == '\"')
            {
                ParseString();
                return;
            }

            if (_char == '.')
            {
                _token = Token.Period;
                NextChar();
                return;
            }

            if (_char == '(')
            {
                _token = Token.OpenRound;
                NextChar();
                return;
            }

            if (_char == ')')
            {
                _token = Token.CloseRound;
                NextChar();
                return;
            }

            if (_char == '\0')
            {
                _token = Token.EOF;
                return;
            }

            NextChar();
            _token = Token.Other;
        }


        void ParseRawString()
        {
            _sb.Length = 0;
            NextChar();
            while (true)
            {
                while (_char != '\"' && _char != '\0')
                {
                    _sb.Append(_char);
                    NextChar();
                }
                if (_char == '\"')
                {
                    NextChar();
                    if (_char == '\"')
                    {
                        NextChar();
                        continue;
                    }
                    else
                        break;
                }
            }
            _token = Token.String;
        }

        void ParseString()
        {
            char terminator = _char;

            _sb.Length = 0;
            NextChar();
            while (_char!=terminator && _char!='\0')
            {
                if (_char == '\\')
                {
                    NextChar();
                    switch (_char)
                    {
                        case '0': _sb.Append('\0'); break;
                        case '\'': _sb.Append('\''); break;
                        case '\"': _sb.Append('\"'); break;
                        case '\\': _sb.Append('\\'); break;
                        case '/': _sb.Append('/'); break;
                        case 'b': _sb.Append('\b'); break;
                        case 'f': _sb.Append('\f'); break;
                        case 'n': _sb.Append('\n'); break;
                        case 'r': _sb.Append('\r'); break;
                        case 't': _sb.Append('\t'); break;
                        case 'u':
                            var sbHex = new StringBuilder();
                            for (int i = 0; i < 4; i++)
                            {
                                NextChar();
                                sbHex.Append(_char);
                            }
                            _sb.Append((char)Convert.ToUInt16(sbHex.ToString(), 16));
                            break;

                        default:
                            throw new InvalidDataException(string.Format("Invalid escape sequence in string literal: '\\{0}'", _char));
                    }
                }
                else
                {
                    _sb.Append(_char);
                }
                NextChar();
            }

            if (_char == terminator)
                NextChar();
            _token = Token.String;
        }


        char _pendingChar = '\0';
        char NextChar()
        {
            if (_pendingChar != '\0')
            {
                _char = _pendingChar;
                _pendingChar = '\0';
                return _char;
            }

            _char = NextCharInternal();
            
            if (_char == '\r')
            {
                if (_char == '\n')
                    NextCharInternal();
                _pendingChar = _char;
                _char = '\n';
                _lineNumber++;
                return _char;
            }

            if (_char == '\n')
            {
                if (_char == '\r')
                    NextCharInternal();
                _pendingChar = _char;
                _char = '\n';
                _lineNumber++;
                return _char;
            }

            return _char;

        }

        char NextCharInternal()
        {
            int ch = _reader.Read();
            if (ch<0)
                return '\0';
            return (char)ch;
        }

        static int IndexOfEscapeableChar(string str, int pos)
        {
            int length = str.Length;
            while (pos < length)
            {
                var ch = str[pos];
                if (ch == '\\' || ch == '/' || ch == '\"' || (ch >= 0 && ch <= 0x1f) || (ch >= 0x7f))
                    return pos;
                pos++;
            }

            return -1;
        }

        /// <summary>
        /// Helper function to escape a string using C#/JSON formatting
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EscapeString(string str)
        {
            if (str == null)
            {
                return "null";
            }

            var _writer = new StringWriter();
            _writer.Write("\"");

            int pos = 0;
            int escapePos;
            while ((escapePos = IndexOfEscapeableChar(str, pos)) >= 0)
            {
                if (escapePos > pos)
                    _writer.Write(str.Substring(pos, escapePos - pos));

                switch (str[escapePos])
                {
                    case '\0': _writer.Write("\\0"); break;
                    case '\'': _writer.Write("\\\'"); break;
                    case '\"': _writer.Write("\\\""); break;
                    case '\\': _writer.Write("\\\\"); break;
                    case '/': _writer.Write("\\/"); break;
                    case '\b': _writer.Write("\\b"); break;
                    case '\f': _writer.Write("\\f"); break;
                    case '\n': _writer.Write("\\n"); break;
                    case '\r': _writer.Write("\\r"); break;
                    case '\t': _writer.Write("\\t"); break;
                    default:
                        _writer.Write(string.Format("\\u{0:x4}", (int)str[escapePos]));
                        break;
                }

                pos = escapePos + 1;
            }


            if (str.Length > pos)
                _writer.Write(str.Substring(pos));
            _writer.Write("\"");

            return _writer.ToString();
        }

        TextReader _reader;
        Token _token;
        char _char;
        int _lineNumber;
        int _tokenLine;
        StringBuilder _sb = new StringBuilder();
    }
}
