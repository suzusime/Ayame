using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayame
{
	enum CommandType
	{
		Plain,//そのまま文字を表示
		Var,//変数を呼び出す
		Func,//函数
	}
	/// <summary>
	/// 原子的なコマンド
	/// これを実行機械が順次評価していく
	/// </summary>
	class Command
	{
		public CommandType ComType
		{
			get; private set;
		}

		public string Text
		{
			get; private set;
		}

		public List<Command> Children
		{
			get; private set;
		}

		private Command()
		{
			Children = new List<Command>();
		}

		static public Command MakePlain(string str)
		{
			return new Command()
			{
				ComType = CommandType.Plain,
				Text = str
			};
		}
		static public Command MakeVar(string str)
		{
			return new Command()
			{
				ComType = CommandType.Var,
				Text = str
			};
		}
		static public Command MakeFunc(List<Command> children)
		{
			return new Command()
			{
				ComType = CommandType.Func,
				Children = children
			};
		}
	}
}
