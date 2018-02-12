using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayame
{
    /// <summary>
    /// 字句解析器の出力に用いるトークン
    /// </summary>
    class Token
    {
        public TokenType Type
        {
            get; set;
        }
        public string Text
        {
            get; set;
        }
        public Token(TokenType type, string text)
        {
            Type = type;
            Text = text;
        }
    }
    /// <summary>
    /// Tokenの種類
    /// </summary>
    enum TokenType
    {
        LF,
        Sharp,
        Dollar,
        Slash,
        //At, //アットマークは"構文上は"意味を持たない（変数名の先頭に含まれる際に特殊な処理を行うが、構文解析時には関係ない）。
        OpenBracket, // [
        CloseBracket, // ]
        Delimiter, //区切り
        Tab,
        Variable, //変数
        NormalString, //普通文字列
        None, //ノードが何も内容を持たないときに入れるトークンのタイプ
    }

    static class TokenTypeExt
    {
        public static string getName(this TokenType value)
        {
            string[] values = { "改行", "#", "$", "/", "[", "]", "区切", "タブ", "変数", "普通文字列", "無" };
            return values[(int)value];
        }
    }
}
