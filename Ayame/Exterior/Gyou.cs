using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayame
{
	public enum GyouType
	{
		Ji,//地の文
		Serifu,//台詞
		Keizoku,//話者名を更新しない台詞
		Label,//ラベル行
		Ku,//空行
	}
	/// <summary>
	/// ゲーム（実行機械）側に渡すための行オブジェクト
	/// これが全ての処理の単位となる
	/// 実行機械は1行ごとに実行していく
	/// つまりプログラムカウンタの指し示す単位がこれ
	/// </summary>
	public class Gyou
	{
		public GyouType LineType
		{
			get; private set;
		}

		/// <summary>
		/// TypeがLabelのときのみ意味を持つ。ラベルの名前
		/// </summary>
		public string LabelName
		{
			get; private set;
		}

		/// <summary>
		/// TypeがSerifuのときのみ意味を持つ。話者の名前
		/// </summary>
		public List<Command> SpeakerName
		{
			get; private set;
		}

		/// <summary>
		/// この行で行う命令
		/// </summary>
		public List<Command> Commands
		{
			get; private set;
		}

		private Gyou()
		{
			LineType = GyouType.Ji;
			LabelName = "";
			SpeakerName = new List<Command>();
			Commands = new List<Command>();
		}

		/// <summary>
		/// ラベル行を作って返す
		/// </summary>
		/// <param name="labelName">ラベル名</param>
		/// <returns></returns>
		static public Gyou MakeLabel(string labelName)
		{
			return new Gyou
			{
				LineType = GyouType.Label,
				LabelName = labelName
			};
		}

		/// <summary>
		/// 空行を作って返す
		/// </summary>
		/// <returns></returns>
		static public Gyou MakeKu()
		{
			return new Gyou
			{
				LineType = GyouType.Ku
			};
		}

		/// <summary>
		/// 地の文を返す
		/// </summary>
		/// <param name="commands"></param>
		/// <returns></returns>
		static public Gyou MakeJi(List<Command> commands)
		{
			return new Gyou
			{
				LineType = GyouType.Ji,
				Commands = commands
			};
		}
		/// <summary>
		/// 台詞を返す
		/// </summary>
		/// <param name="commands"></param>
		/// <returns></returns>
		static public Gyou MakeSerifu(List<Command> name, List<Command> commands)
		{
			return new Gyou
			{
				LineType = GyouType.Serifu,
				SpeakerName = name,
				Commands = commands
			};
		}
		/// <summary>
		/// 名前継続の台詞を返す
		/// </summary>
		/// <param name="commands"></param>
		/// <returns></returns>
		static public Gyou MakeKeizoku(List<Command> commands)
		{
			return new Gyou
			{
				LineType = GyouType.Keizoku,
				Commands = commands
			};
		}
	}
}
