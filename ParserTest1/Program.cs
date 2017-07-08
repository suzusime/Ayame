using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprache;

namespace ParserTest1
{
	using Expressions = IEnumerable<Expression>;

	interface Expression { }

	class Strexp : Expression
	{
		public string str { get; set; }
		public Strexp(string _str) { str = _str; }
	}
	class Function : Expression
	{
		public Expressions name { get; set; }
		public IEnumerable<Expressions> args { get; set; }
		public Function(Expressions _name, IEnumerable<Expressions> _args)
		{
			name = _name;
			args = _args;
		}
	}
	class Program
	{
		static readonly Parser<char> character = Parse.AnyChar;
		static readonly Parser<string> str = character.AtLeastOnce().Text();
		static readonly Parser<string> tokenstr = character.AtLeastOnce().Token().Text();
		static readonly Parser<string> Cell =
			Parse.AnyChar.Except(Parse.Chars(' ', '[', ']','\n')).AtLeastOnce().Text();
		static readonly Parser<string> spaces = Parse.WhiteSpace.AtLeastOnce().Text();
		static readonly Parser<Strexp> strexp =
			from _str in Cell
			select new Strexp(_str);
		static readonly Parser<Function> function =
			from begin in Parse.Char('[')
			from func in expressions.AtLeastOnce()
			from end in Parse.Char(']')
			select new Function(func.Take(1).Single(), func.Skip(1));
		static readonly Parser<Expression> expression =
			strexp
			.Or<Expression>(function);
		static readonly Parser<Expressions> expressions = expression.Token().AtLeastOnce();

		static void Main(string[] args)
		{
			var result = expressions.Parse("我々は[func [hoge piyo] nyan]である。");
			//bool hoge = result==result;
		}
	}
}
