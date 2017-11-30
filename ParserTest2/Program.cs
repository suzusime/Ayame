using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprache;

namespace ParserTest2
{
	class Program
	{
		/// <summary>
		/// 基本文字のパーサ。
		/// 基本文字：文法記号を除いた普通の文字。
		/// </summary>
		static readonly Parser<char> NomalChar = Parse.CharExcept("[]");
		static void Main(string[] args)
		{
			testNormalChar();
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
				Console.WriteLine("OK: "+name);
			} else
			{
				Console.WriteLine("NG: "+name);
			}
		}
		#region テスト函数
		static private void testNormalChar()
		{
			bool ok = true;
			char c1 = NomalChar.Parse("hoge");
			ok = ('h' == c1) && ok;
			try
			{
				//パースエラーになる
				char c2 = NomalChar.Parse("[hoge]");
				ok = false;
			}
			catch (ParseException)
			{

			}
			Assert("NomalChar", ok);
		}
		#endregion
	}
}
