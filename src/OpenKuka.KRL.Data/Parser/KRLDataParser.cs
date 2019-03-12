using OpenKuka.KRL.Data.AST;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpenKuka.KRL.Data.Parser
{
    public static class KRLDataParser
    {
        public static List<DataObject> Parse(string inputString)
        {
            return Parse(KRLDataLexer.Tokenize(inputString));
        }

        private static List<DataObject> Parse(List<RegexToken<KrlDataTokenType>> tokens)
        {
            int index = 0;
            int count = tokens.Count;

            var dataList = new List<DataObject>();

            while (index < count)
            {
                var token = tokens[index];
                if (token.Type == KrlDataTokenType.Comma) index++;
                dataList.Add(ParseData(tokens, ref index, false));
            }

            return dataList;
        }
        private static DataObject ParseData(List<RegexToken<KrlDataTokenType>> tokens, ref int index, bool isIdentifierMandatory = true)
        {
            int count = tokens.Count;
            bool hasIdentifier = false;

            DataObject data;
            var name = "";

            if (isIdentifierMandatory)
            {
                // we need at least two tokens to form a DataObject (id + value)
                if (index > count - 2)
                    throw new ArgumentException("expected : more tokens");

                // the first token must be the object identifier
                if (tokens[index].Type != KrlDataTokenType.ID)
                    throw new ArgumentException("expected : identifier");

                hasIdentifier = true;
            }
            else
            {
                // we need at least one token to form a DataObject with no identifier
                if (index > count - 1)
                    throw new ArgumentException("expected : more tokens");

                // the first token is the object identifier
                if (tokens[index].Type == KrlDataTokenType.ID)
                    hasIdentifier = true;
            }

            if (hasIdentifier)
            {
                name = tokens[index++].Value;

                // check if the identifier is followed by '[]'
                if (index + 1 < count)
                {
                    if (tokens[index].Type == KrlDataTokenType.LSquareBracket)
                    {
                        if (tokens[index + 1].Type != KrlDataTokenType.RSquareBracket)
                            throw new ArgumentException("expected : ']'");

                        // append '[]' to the identifier
                        name += "[]";
                        index++;
                        index++;
                    }
                }
            }

            if (index > count - 1)
                throw new ArgumentException("expected : token");

            
            var token = tokens[index];
            switch (token.Type)
            {
                case KrlDataTokenType.BoolValue:
                    data = new BoolData(token.Value);
                    index++;
                    break;

                case KrlDataTokenType.IntNumber:
                    data = new IntData(token.Value);
                    index++;
                    break;

                case KrlDataTokenType.RealNumber:
                    data = new RealData(token.Value);
                    index++;
                    break;

                case KrlDataTokenType.NaN:
                    data = new RealData(token.Value);
                    index++;
                    break;

                case KrlDataTokenType.EnumValue:
                    data = new EnumData(token.Value);
                    index++;
                    break;

                case KrlDataTokenType.DoubleQuotedString:
                    var dq = token.Value.Trim('"');
                    if (dq.Length > 1) data = new StringData(dq);
                    else data = new CharData(dq);
                    index++;
                    break;

                case KrlDataTokenType.SingleQuotedString:
                    var sq = token.Value.Trim('\'');
                    if (sq.Length > 1) data = new StringData(sq);
                    else data = new CharData(sq);
                    index++;
                    break;

                case KrlDataTokenType.BitString:
                    data = new BitArrayData(token.Value);
                    index++;
                    break;

                case KrlDataTokenType.LCurlyBracket:
                    data = ParseStruc(tokens, ref index);
                    break;

                case KrlDataTokenType.ID:
                    throw new ArgumentException("expected : value (got identifier)");

                case KrlDataTokenType.Comma:
                    throw new ArgumentException("expected : value (got comma separator)");

                default:
                    throw new ArgumentException("expected : value");
            }

            data.Name = name;
            return data;
        }
        private static StrucData ParseStruc(List<RegexToken<KrlDataTokenType>> tokens, ref int index)
        {
            int count = tokens.Count;

            // consume the LCurlyBracket
            index++;

            string strucName = "";
            StrucData data;

            if (index + 2 > count)
                throw new ArgumentException("expected : more tokens");

            if (tokens[index].Type != KrlDataTokenType.ID)
                throw new ArgumentException("expected : identifier");

            // if the next token is a colon, then the identifier is for the struc type
            if (tokens[index + 1].Type == KrlDataTokenType.Colon)
            {
                strucName = tokens[index].Value;
                index++;
                index++;
            }

            data = new StrucData(strucName);

            while (index < count)
            {
                var token = tokens[index];
                if (token.Type == KrlDataTokenType.RCurlyBracket) { index++; break; }
                if (token.Type == KrlDataTokenType.Comma) index++;
                data.Add(ParseData(tokens, ref index, true));
            }

            return data;
        }
    }
}
