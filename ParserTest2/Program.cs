using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprache;

namespace ParserTest2
{

	using Expressions = List<Expression>;
	interface Expression { }

	/*class Expressions : IEnumerable<Expression>
	{
		public List<Expression> Item;
		public Expressions()
		{
			Item = new List<Expression>();
		}
		public void Add(Expression expr)
		{
			Item.Add(expr);
		}
		public Expressions(Expression expr)
		{
			Item = new List<Expression>() { expr };
		}
		public IEnumerator<Expression> GetEnumerator()
		{
			return ((IEnumerable<Expression>)Item).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Expression>)Item).GetEnumerator();
		}
	}*/

	class Strexp : Expression
	{
		public string str { get; set; }
		public Strexp(string _str) { str = _str; }
	}
	class Funcexp : Expression
	{
		public Expressions name { get; set; }
		public IEnumerable<Expressions> args { get; set; }
		public Funcexp(Expressions _name, IEnumerable<Expressions> _args)
		{
			name = _name;
			args = _args;
		}
	}
	class Program
	{
		/// <summary>
		/// 基本文字のパーサ。
		/// 基本文字：文法記号を除いた普通の文字。
		/// </summary>
		static readonly Parser<char> NormalChar = Parse.CharExcept(" []");
		/// <summary>
		/// 基本文字列のパーサ。
		/// </summary>
		static readonly Parser<string> NormalString = NormalChar.Many().Text();
		static void Main(string[] args)
		{
			testNormalChar();
			testNormalString();
			testStringExpression();
			testStringExpressions();
			Console.ReadLine();//ウェイト
		}
		/// <summary>
		/// 区切り文字列のパーサ。
		/// </summary>
		static readonly Parser<string> Delimiter = Parse.Chars(" ").AtLeastOnce().Text();
		/// <summary>
		/// 文字列式のパーサ。
		/// </summary>
		static readonly Parser<Strexp> StringExpression =
			from str in NormalString
			select new Strexp(str);
		/// <summary>
		/// 文字列式
		/// </summary>
		static readonly Parser<List<Strexp>> StringExpressions =
			from s1 in StringExpression
			from s in (from del in Delimiter from sm in StringExpression select sm).Many()
			select s.ToList().Insert(0,s1);
			
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
			char c1 = NormalChar.Parse("hoge");
			ok = ('h' == c1) && ok;
			try
			{
				//パースエラーになる
				char c2 = NormalChar.Parse("[hoge]");
				ok = false;
			}
			catch (ParseException)
			{

			}
			char c3 = NormalChar.Parse("夢見");
			ok = (c3 == '夢') && ok;
			Assert("NomalChar", ok);
		}
		static private void testNormalString()
		{
			bool ok = true;
			string s1 = NormalString.Parse("ご注文はうさぎですか？");
			ok = ok && (s1 == "ご注文はうさぎですか？");
			string s2 = NormalString.Parse("hoge[aito");
			ok = ok && (s2 == "hoge");
			string s3 = NormalString.Parse("[nyan]");
			ok = ok && (s3 == "");
			string s4 = NormalString.Parse("hoge fuga");
			ok = ok && (s4 == "hoge");
			Assert("NormalString", ok);
		}
		static private void testStringExpression()
		{
			bool ok = true;
			Strexp e1 = StringExpression.Parse("素晴らしき日々");
			ok = ok && (e1.str == new Strexp("素晴らしき日々").str);
			Assert("StringExpression", ok);
		}
		static private void testStringExpressions()
		{
			bool ok = true;
			List<Strexp> e1 = StringExpressions.Parse("素晴らしき日々 ～不連続存在～");
			ok = ok && (e1[0].str == (new List<Strexp>() { new Strexp("素晴らしき日々")})[0].str);
			Assert("StringExpressions", ok);
		}
		#endregion
	}
}
