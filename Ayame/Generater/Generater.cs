using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayame.Generator
{
    static class Generater
    {
        static Generater()
        {

        }

        /// <summary>
        /// 構文木から目的コードを生成する
        /// </summary>
        /// <param name="src">構文木</param>
        /// <returns>目的コード</returns>
        static string Generate(Node src)
        {
            switch (src.Type)
            {
                case NodeType.Script:
                    List<string> tmp = new List<string>(src.Children.Count);
                    for(int i=0; i<tmp.Count; ++i)
                    {
                        tmp[i] = Generate(src.Children[i]);
                    }
                    return string.Join("\n", tmp);
                case NodeType.Line:
                    switch (src.Children.Count)
                    {
                        default:
                            throw new NotImplementedException();
                    }
                case NodeType.LabelLine:
                    return ":" + src.Children[0].Content.Text;//Childrenの要素は1つであり、その中身はStr
                case NodeType.Str:
                    if (src.Content.Type == TokenType.NormalString)
                    {
                        return "puts,"+src.Content.Text;
                    }
                    else if (src.Content.Type == TokenType.Variable)
                    {
                        return "putvar,"+src.Content.Text;
                    }
                    else
                    {
                        //ここに来ることがあったらパーサがおかしい
                        throw new Exception();
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
