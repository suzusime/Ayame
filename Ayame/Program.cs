﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayame
{

	class Program
	{
		static void Main(string[] args)
		{
			Lexer.Test();
			Parser.Test();
			Generater.Test();

			Console.ReadLine();
		}
	}

	public class Compiler
	{
		public static List<Gyou> Compile(string script)
		{
			return Generater.Generate(Parser.Parse(script));
		}
	}
}
