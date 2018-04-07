using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayame
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
        static List<Gyou> Generate(Node src)
        {
            switch (src.Type)
            {
                case NodeType.Script:
                    List<Gyou> res = new List<Gyou>();
                    foreach(Node item in src.Children)
                    {
						res.AddRange(Generate(item));
                    }
                    return res;
                case NodeType.Line:
                    switch (src.Children.Count)
                    {
						case 0://空行の場合
							return new List<Gyou>()
							{
								Gyou.MakeKu()
							};
						case 1://地の文の場合
							return new List<Gyou>()
							{
								Gyou.MakeJi(MakeCommands(src.Children[0]))
							};
						case 2://台詞文の場合
							if (src.Children[0].Children[0].Content.Text == "")
							{
								//継続の場合
								return new List<Gyou>()
								{
									Gyou.MakeKeizoku(MakeCommands(src.Children[1]))
								};
							}
							else
							{
								//名前もある場合
								return new List<Gyou>()
								{
									Gyou.MakeSerifu(MakeCommands(src.Children[0]), MakeCommands(src.Children[1]))
								};
							}
                        default:
                            throw new Exception();
                    }
                case NodeType.LabelLine:
					return new List<Gyou>()
					{
						Gyou.MakeLabel(src.Content.Text)
					};
                default:
                    throw new NotImplementedException();
            }
        }

		static private List<Command> MakeCommands(Node src)
		{
			switch (src.Type)
			{
				case NodeType.Expr:
					List<Command> res = new List<Command>();
					foreach(var item in src.Children)
					{
						res.AddRange(MakeCommands(item));
					}
					return res;
				case NodeType.Str:
					if (src.Content.Type == TokenType.Variable)
					{
						return new List<Command>()
						{
							Command.MakeVar(src.Content.Text)
						};
					}
					else if (src.Content.Type == TokenType.NormalString)
					{
						return new List<Command>()
						{
							Command.MakePlain(src.Content.Text)
						};
					}
					else
					{
						throw new Exception();
					}
				case NodeType.Func:
					List<Command> children = new List<Command>();
					foreach(var item in src.Children)
					{
						children.AddRange(MakeCommands(item));
					}
					return new List<Command>()
					{
						Command.MakeFunc(children)
					};
				default:
					throw new Exception();
			}
		}
		#region テスト
		static public void Test()
		{
			var test1 = Generate(Parser.Parse(""));
			var test2 = Generate(Parser.Parse("こんにちは"));
			var test3 = Generate(Parser.Parse("\tまいくのテストちゅー\n"));
			var test4 = Generate(Parser.Parse("七瀬\tご注文は[if true うさぎ たぬき]ですか？\n"));

			Console.WriteLine("OK: Generater");
		}
		#endregion
	}
}
