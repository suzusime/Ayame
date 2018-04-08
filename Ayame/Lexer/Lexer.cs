using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Ayame
{
	/// <summary>
	/// 字句解析器
	/// </summary>
	static class Lexer
	{
		/// <summary>
		/// 字句解析を行う
		/// </summary>
		/// <returns></returns>
		static public List<Token> Lex(string src)
		{
			List<Token> list = Tokenize(Preprocess(src));
			ModifyTokenList(list);
			return list;
		}

		/// <summary>
		/// プリプロセスを行う
		/// </summary>
		/// <param name="src"></param>
		/// <returns></returns>
		static string Preprocess(string src)
		{
			//改行文字をLFに揃える
			string res = src.Replace("\r\n", "\n").Replace("\r", "\n");
			return res;
		}

		/// <summary>
		/// 字句解析を行う
		/// </summary>
		/// <param name="src"></param>
		/// <returns></returns>
		static List<Token> Tokenize(string src)
		{
			bool isCommentMode = false;
			bool isEscapeMode = false;//次の文字をエスケープするフラグ
			int bracketCount = 0;//今何個の[]の入れ子の中にいるのか
			List<Token> res = new List<Token>();
			TextElementEnumerator charEnum = StringInfo.GetTextElementEnumerator(src);
			while (charEnum.MoveNext())
			{
				string s = charEnum.GetTextElement();
				if (isCommentMode)
				{
					if (s == "\n")
					{
						isCommentMode = false;
					}
				}
				else
				{
					if (isEscapeMode) {
						//エスケープモードの場合問答無用で普通文字列にする
						if (res.Count > 0 && res.Last().Type == TokenType.NormalString)
						{
							res.Last().Text += s;
						}
						else
						{
							res.Add(new Token(TokenType.NormalString, s));
						}
						isEscapeMode = false;
					}
					else if (s == "\n")
					{
						if (bracketCount > 0)
						{
							//前も区切り文字だった場合一つのトークンにする
							if (res.Count > 0 && res.Last().Type == TokenType.Delimiter)
							{
								res.Last().Text += " ";
							}
							else
							{
								res.Add(new Token(TokenType.Delimiter, s));
							}
						}
						else
						{
							res.Add(new Token(TokenType.LF, s));
						}
					}
					else if (s == "#")
					{
						res.Add(new Token(TokenType.Sharp, s));
					}
					else if (s == "$")
					{
						res.Add(new Token(TokenType.Dollar, s));
					}
					else if (s == "/")
					{
						//スラッシュが2連続だとコメントモードに入る
						if (res.Count > 0 && res.Last().Type == TokenType.Slash)
						{
							res.RemoveAt(res.Count - 1);
							isCommentMode = true;
						}
						else
						{
							res.Add(new Token(TokenType.Slash, s));
						}
					}
					else if (s == "[")
					{
						res.Add(new Token(TokenType.OpenBracket, s));
						bracketCount++;
					}
					else if (s == "]")
					{
						res.Add(new Token(TokenType.CloseBracket, s));
						bracketCount--;
					}
					else if (s == "\t")
					{
						if (bracketCount > 0)
						{
							//前も区切り文字だった場合一つのトークンにする
							if (res.Count > 0 && res.Last().Type == TokenType.Delimiter)
							{
								res.Last().Text += " ";
							}
							else
							{
								res.Add(new Token(TokenType.Delimiter, s));
							}
						}
						else
						{
							//前も区切り文字だった場合一つのトークンにする
							if (res.Count > 0 && res.Last().Type == TokenType.Tab)
							{
								res.Last().Text += "\t";
							}
							else
							{
								res.Add(new Token(TokenType.Tab, s));
							}
						}
					}
					else if (s == " ")
					{
						//前も区切り文字だった場合一つのトークンにする
						if (res.Count > 0 && res.Last().Type == TokenType.Delimiter)
						{
							res.Last().Text += " ";
						}
						else
						{
							res.Add(new Token(TokenType.Delimiter, s));
						}
					}
					else if (s == "\\")
					{
						//エスケープモードに入る
						isEscapeMode = true;
					}
					//変数名として使用可能な文字
					else if (!Regex.IsMatch(s, @"[^a-zA-Z0-9@]"))
					{
						//前が$であった場合は変数名とする
						if (res.Count > 0 && res.Last().Type == TokenType.Dollar)
						{
							res.RemoveAt(res.Count - 1);
							res.Add(new Token(TokenType.Variable, "$" + s));
						}
						//変数名が前から続いている場合も変数名とする
						else if (res.Count > 0 && res.Last().Type == TokenType.Variable)
						{
							res.Last().Text += s;
						}
						//前が普通文字列であった場合は普通文字列の続きとして扱う
						else if (res.Count > 0 && res.Last().Type == TokenType.NormalString)
						{
							res.Last().Text += s;
						}
						//それ以外の場合は新たに普通文字列をつくる
						else
						{
							res.Add(new Token(TokenType.NormalString, s));
						}
					}
					//それ以外の文字は普通文字列を構成する
					else
					{
						if (res.Count > 0 && res.Last().Type == TokenType.NormalString)
						{
							res.Last().Text += s;
						}
						else
						{
							res.Add(new Token(TokenType.NormalString, s));
						}
					}

					if (bracketCount < 0)
					{

					}
				}
			}
			return res;
		}

		/// <summary>
		/// トークンリストから文法上不要なトークンを削除する。
		/// </summary>
		/// <param name="list"></param>
		static void ModifyTokenList(List<Token> list)
		{
			list.RemoveAll(x => (x.Type == TokenType.Dollar) || (x.Type == TokenType.Slash));
		}

		#region テスト
		/// <summary>
		/// テストを行う函数
		/// </summary>
		/// <param name="args"></param>
		static public void Test()
		{
			testPreprocess();
			testTokenize();
			testModifyTokenList();
		}

		/// <summary>
		/// exprがfalseのときにエラーを出す。
		/// </summary>
		/// <param name="name"></param>
		/// <param name="expr"></param>
		static private void Assert(string name, bool expr)
		{
			if (expr)
			{
				Console.WriteLine("OK: " + name);
			}
			else
			{
				Console.WriteLine("NG: " + name);
			}
		}
		static private void testPreprocess()
		{
			bool ok = true;
			ok = ok && "hogeあいう123#$@;][" == Preprocess("hogeあいう123#$@;][");
			ok = ok && "a\nvf\n\nb" == Preprocess("a\nvf\n\nb");
			ok = ok && "a\nvf\n\nb" == Preprocess("a\rvf\n\nb");
			ok = ok && "@\n@\n\n@" == Preprocess("@\r\n@\r\n\r@");
			Assert("Preprocess", ok);
		}
		static private void testTokenize()
		{
			bool ok = true;

			//要素の直接比較は難しそうなので要素数でテストする

			var tes1 = Tokenize("");
			ok = ok && tes1.Count == 0;

			var tes2 = Tokenize("[");
			ok = ok && tes2.Count == 1;

			var tes3 = Tokenize("hoge");
			ok = ok && tes3.Count == 1;

			var tes4 = Tokenize("hoge//にゃん#h$g\nほげ");
			ok = ok && tes4.Count == 1;

			var tes5 = Tokenize("ねえ、$@heroくん");
			ok = ok && tes5.Count == 3;

			var tes6 = Tokenize("[bool   flag1 false]");
			ok = ok && tes6.Count == 7;

			var tes7 = Tokenize("You are $hoge ing.");
			ok = ok && tes7.Count == 7;

			var tes8 = Tokenize("[switch [select どうしてって言われると……なんとなく、だ。   そんなの……お前のことが好きだからに決まってる。] [jump badend][jump happyend]]");
			ok = ok && tes8.Count == 22;

			var tes9 = Tokenize("[");
			ok = ok && tes9.Count == 1;

			var tes10 = Tokenize("[");
			ok = ok && tes10.Count == 1;

			Assert("Tokenize", ok);
		}
		static private void testModifyTokenList()
		{
			bool ok = true;

			var list1 = Tokenize("hoge");
			var list1b = new List<Token>(list1);
			ModifyTokenList(list1b);
			ok = ok && list1.Count == list1b.Count;

			var list2 = Tokenize("ho/ge//");
			var list2b = new List<Token>(list2);
			ModifyTokenList(list2b);
			ok = ok && list2.Count - 1 == list2b.Count;

			var list3 = Tokenize("ho$geほげ$にゃん");
			var list3b = new List<Token>(list3);
			ModifyTokenList(list3b);
			ok = ok && list3.Count - 1 == list3b.Count;

			Assert("ModifyTokenList", ok);
		}
		#endregion
	}

    class LexErrorException : Exception
    {
        public LexErrorException(string message) : base(message) { }
    }

}
