using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ParserTest3
{
	class Token {
		public TokenType Type {
			get; set;
		}
		public string Text
		{
			get; set;
		}
		public Token(TokenType type, string text) {
			Type = type;
			Text = text;
		}
	}
	
	enum TokenType
	{
		LF,
		Sharp,
		Dollar,
		Slash,
		//At, //アットマークは"構文上は"意味を持たない（変数名の先頭に含まれる際に特殊な処理を行うが、構文解析時には関係ない）。
		OpenBlacket, // [
		CloseBlacket, // ]
		Delimiter, //区切り
		Tab,
		Variable, //変数
		NormalString, //普通文字列
	}

	class Program
	{
		static void Main(string[] args)
		{
			testPreprocess();
			testTokenize();
			Console.ReadLine();
		}

		/// <summary>
		/// プリプロセスを行う
		/// </summary>
		/// <param name="src"></param>
		/// <returns></returns>
		static string Preprocess(string src)
		{
			//改行文字をLFに揃える
			string res = src.Replace("\r\n", "\n").Replace("\r","\n");
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
					if(s == "\n")
					{
						res.Add(new Token(TokenType.LF, s));
					}
					else if(s == "#")
					{
						res.Add(new Token(TokenType.Sharp, s));
					}
					else if(s == "$")
					{
						res.Add(new Token(TokenType.Dollar, s));
					}
					else if (s == "/")
					{
						//スラッシュが2連続だとコメントモードに入る
						if(res.Count > 0 && res.Last().Type == TokenType.Slash)
						{
							res.RemoveAt(res.Count - 1);
							isCommentMode = true;
						} else
						{
							res.Add(new Token(TokenType.Slash, s));
						}
					}
					else if (s == "[")
					{
						res.Add(new Token(TokenType.OpenBlacket, s));
					}
					else if (s == "]")
					{
						res.Add(new Token(TokenType.CloseBlacket, s));
					}
					else if (s == "\t")
					{
						res.Add(new Token(TokenType.Tab, s));
					}
					else if(s==" ")
					{
						//前も区切り文字だった場合一つのトークンにする
						if (res.Count > 0 && res.Last().Type == TokenType.Delimiter)
						{
							res.Last().Text += " ";
						} else
						{
							res.Add(new Token(TokenType.Delimiter, s));
						}
					}
					//変数名として使用可能な文字
					else if (!Regex.IsMatch(s, @"[^a-zA-z0-9]"))
					{
						//前が$であった場合は変数名とする
						if(res.Count > 0 && res.Last().Type == TokenType.Dollar)
						{
							res.RemoveAt(res.Count - 1);
							res.Add(new Token(TokenType.Variable, s));
						}
						//変数名が前から続いている場合も変数名とする
						else if (res.Count > 0 && res.Last().Type == TokenType.Variable)
						{
							res.Last().Text += s;
						}
						//前が普通文字列であった場合は普通文字列の続きとして扱う
						else if(res.Count > 0 && res.Last().Type == TokenType.NormalString)
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
				}
			}
			return res;
		}

		#region テスト
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
		#endregion
	}
}
