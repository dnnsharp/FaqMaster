using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class JsonUtil
    {
        public static T Deserialize<T>(string json)
        {
            // JavaScriptSerializer doesn't handle javascript style comments, so let's take those out manually
            json = Regex.Replace(json, "//.*", "");
            json = Regex.Replace(json, "\\/\\*.*?\\*\\/", "", RegexOptions.Singleline);
            return new JavaScriptSerializer().Deserialize<T>(json);

        }

        public static string JsonEncode(object o)
        {
            if (o == null)
                return "null";

            //if (o.GetType() == typeof(bool))
            //    return o.ToString().ToLower();

            return new JavaScriptSerializer().Serialize(o);

            //return JsonEncode(o.ToString(), true);
        }

        public static string JsonEncode(string s, bool appendDelimiters = false)
        {
            if (s == null)
                return "null";

            StringBuilder sb = new StringBuilder();

            // leading delimiter
            if (appendDelimiters)
                sb.Append("\"");

            if (s != null) {
                char[] chars = null;
                int lastWritePosition = 0;

                for (int i = 0; i < s.Length; i++) {
                    var c = s[i];

                    // don't escape standard text/numbers except '\' and the text delimiter
                    if (c >= ' ' && c < 128 && c != '\\' && c != '\"')
                        continue;

                    string escapedValue;

                    switch (c) {
                        case '\t':
                            escapedValue = @"\t";
                            break;
                        case '\n':
                            escapedValue = @"\n";
                            break;
                        case '\r':
                            escapedValue = @"\r";
                            break;
                        case '\f':
                            escapedValue = @"\f";
                            break;
                        case '\b':
                            escapedValue = @"\b";
                            break;
                        case '\\':
                            escapedValue = @"\\";
                            break;
                        case '\u0085': // Next Line
                            escapedValue = @"\u0085";
                            break;
                        case '\u2028': // Line Separator
                            escapedValue = @"\u2028";
                            break;
                        case '\u2029': // Paragraph Separator
                            escapedValue = @"\u2029";
                            break;
                        case '\'':
                            // this charater is being used as the delimiter
                            escapedValue = @"\'";
                            break;
                        case '"':
                            // this charater is being used as the delimiter
                            escapedValue = "\\\"";
                            break;
                        default:
                            escapedValue = (c <= '\u001f') ? ToCharAsUnicode(c) : null;
                            break;
                    }

                    if (escapedValue == null)
                        continue;

                    if (i > lastWritePosition) {
                        if (chars == null)
                            chars = s.ToCharArray();

                        // write unchanged chars before writing escaped text
                        sb.Append(chars, lastWritePosition, i - lastWritePosition);
                    }

                    lastWritePosition = i + 1;
                    sb.Append(escapedValue);
                }

                if (lastWritePosition == 0) {
                    // no escaped text, write entire string
                    sb.Append(s);
                } else {
                    if (chars == null)
                        chars = s.ToCharArray();

                    // write remaining text
                    sb.Append(chars, lastWritePosition, s.Length - lastWritePosition);
                }
            }

            // trailing delimiter
            if (appendDelimiters)
                sb.Append("\"");

            return sb.ToString();
        }

        public static string ToCharAsUnicode(char c)
        {
            char h1 = IntToHex((c >> 12) & '\x000f');
            char h2 = IntToHex((c >> 8) & '\x000f');
            char h3 = IntToHex((c >> 4) & '\x000f');
            char h4 = IntToHex(c & '\x000f');

            return new string(new[] { '\\', 'u', h1, h2, h3, h4 });
        }

        public static char IntToHex(int n)
        {
            if (n <= 9) {
                return (char)(n + 48);
            }
            return (char)((n - 10) + 97);
        }
    }
}
