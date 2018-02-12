using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayame
{
    /// <summary>
    /// 抽象構文木の節点
    /// </summary>
    class Node
    {
        public NodeType Type
        {
            get; set;
        }
        public List<Node> Children
        {
            get; set;
        }
        public Token Content { get; set; }
        public Node(NodeType type, Token content, List<Node> children)
        {
            Type = type;
            Content = content;
            Children = children;
        }
    }

    /// <summary>
    /// Nodeの種類
    /// </summary>
    enum NodeType
    {
        Script,//スクリプト
        Line,//行
        LabelLine,//ラベル行
        Expr,//式
        Str,//文字列
        Func,//函数
    }

    static class NodeTypeExt
    {
        public static string getName(this NodeType value)
        {
            string[] values = { "スクリプト", "行", "ラベル行", "式", "文字列", "函数" };
            return values[(int)value];
        }
    }
}
