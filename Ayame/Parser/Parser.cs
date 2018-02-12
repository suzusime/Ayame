
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayame
{
    /// <summary>
    /// 構文解析中に結果を返すためのクラス。
    /// </summary>
    class ParseResult
    {
        public Node node { get; set; }
        public int index { get; set; }
        public ParseResult(Node _node, int _index)
        {
            node = _node;
            index = _index;
        }
    }

    class ParseErrorException : Exception
    {
        public ParseErrorException(List<Token> list, int index, string message) : base("構文解析エラー：\n" + message) { }
        public ParseErrorException(List<Token> list, int index, NodeType parsing, TokenType entered)
            : base("構文解析エラー：\n構文要素《" + parsing.getName() + "》の解析中にエラー。\nトークン〈" + entered.getName() + "〉はここに来てはいけません。")
        { }
    }


    /// <summary>
    /// 構文解析器
    /// </summary>
    static class Parser
    {
        /// <summary>
        /// 文字列をパースする
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static ParseResult str(List<Token> list, int index)
        {
            //そこで終了する場合
            if (index >= list.Count)
            {
                //ここで終わってはいけないので例外を投げる
                throw new ParseErrorException(list, index, "構文要素《文字列》の解析中にエラー。\nここで終了してはいけません。");
            }

            if (list[index].Type == TokenType.NormalString)
            {
                int resindex = index + 1;
                return new ParseResult(new Node(NodeType.Str, list[index], new List<Node>()), resindex);
            }
            else if (list[index].Type == TokenType.Variable)
            {
                int resindex = index + 1;
                return new ParseResult(new Node(NodeType.Str, list[index], new List<Node>()), resindex);
            }
            else
            {
                //ダメな要素がある場合
                throw new ParseErrorException(list, index, NodeType.Str, list[index].Type);
            }
        }

        /// <summary>
        /// 函数
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static ParseResult Func(List<Token> list, int index)
        {
            //そこで終了する場合
            if (index >= list.Count)
            {
                //ここで終わってはいけないので例外を投げる
                throw new ParseErrorException(list, index, "構文要素《函数》の解析中にエラー。\nここで終了してはいけません。");
            }

            if (list[index].Type == TokenType.NormalString || list[index].Type == TokenType.Variable)
            {
                ParseResult r1 = str(list, index);
                ParseResult r2 = Func_dash(list, r1.index);
                List<Node> children = new List<Node>() { r1.node };
                children.AddRange(r2.node.Children);
                return new ParseResult(new Node(NodeType.Func, new Token(TokenType.None, ""), children), r2.index);
            }
            else if (list[index].Type == TokenType.OpenBracket)
            {
                ParseResult r1 = Func(list, index + 1);
                //そこで終了する場合
                if (r1.index >= list.Count)
                {
                    //ここで終わってはいけないので例外を投げる
                    throw new ParseErrorException(list, r1.index, "構文要素《函数》の解析中にエラー。\nここで終了してはいけません。");
                }
                ParseResult r2 = Func_dash(list, r1.index);
                List<Node> children = new List<Node>() { r1.node };
                children.AddRange(r2.node.Children);
                return new ParseResult(new Node(NodeType.Func, new Token(TokenType.None, ""), children), r2.index);
            }
            else
            {
                //ダメな要素がある場合
                throw new ParseErrorException(list, index, NodeType.Func, list[index].Type);
            }
        }

        /// <summary>
        /// 函数'
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static ParseResult Func_dash(List<Token> list, int index)
        {
            //そこで終了する場合
            if (index >= list.Count)
            {
                //ここで終わってはいけないので例外を投げる
                throw new ParseErrorException(list, index, "構文要素《函数》の解析中にエラー。\nここで終了してはいけません。");
            }

            //無の場合
            if (list[index].Type == TokenType.CloseBracket)
            {
                return new ParseResult(new Node(NodeType.Func, new Token(TokenType.None, ""), new List<Node>()), index + 1);
            }
            //さらに続く場合
            else if (list[index].Type == TokenType.Delimiter)
            {
                ParseResult r1 = Func(list, index + 1);
                return new ParseResult(new Node(NodeType.Func, new Token(TokenType.None, ""), r1.node.Children), r1.index);
            }
            else
            {
                //ダメな要素がある場合
                throw new ParseErrorException(list, index, NodeType.Func, list[index].Type);
            }
        }

        /// <summary>
        /// 式
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static ParseResult Expr(List<Token> list, int index)
        {
            //そこで終了する場合
            if (index >= list.Count)
            {
                //ここで終わってはいけないので例外を投げる
                throw new ParseErrorException(list, index, "構文要素《式》の解析中にエラー。\nここで終了してはいけません。");
            }
            if (list[index].Type == TokenType.Delimiter)
            {
                ParseResult r1 = Expr_dash(list, index + 1);
                List<Node> children = new List<Node>(r1.node.Children);
                return new ParseResult(new Node(NodeType.Expr, new Token(TokenType.None, ""), children), r1.index);
            }
            else if (list[index].Type == TokenType.NormalString || list[index].Type == TokenType.Variable)
            {
                ParseResult r1 = str(list, index);
                ParseResult r2 = Expr_dash(list, r1.index);
                List<Node> children = new List<Node>() { r1.node };
                children.AddRange(r2.node.Children);
                return new ParseResult(new Node(NodeType.Expr, new Token(TokenType.None, ""), children), r2.index);
            }
            else if (list[index].Type == TokenType.OpenBracket)
            {
                ParseResult r1 = Func(list, index + 1);
                if (r1.index >= list.Count)
                {
                    throw new ParseErrorException(list, r1.index, "構文要素《式》の解析中にエラー。\nここで終了してはいけません。");
                }
                ParseResult r2 = Expr_dash(list, r1.index);
                List<Node> children = new List<Node>() { r1.node };
                children.AddRange(r2.node.Children);
                return new ParseResult(new Node(NodeType.Expr, new Token(TokenType.None, ""), children), r2.index);
            }
            else
            {
                throw new ParseErrorException(list, index, NodeType.Expr, list[index].Type);
            }
        }

        /// <summary>
        /// 式'
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static ParseResult Expr_dash(List<Token> list, int index)
        {
            //そこで終了する場合
            if (index >= list.Count)
            {
                //ここで終わってもよい
                //終わった場合さらに一つ先を返す
                return new ParseResult(new Node(NodeType.Expr, new Token(TokenType.None, ""), new List<Node>()), index + 1);
            }

            //さらに続く場合。ちょっと実装をさぼって式に戻すことにする
            if (list[index].Type == TokenType.Delimiter || list[index].Type == TokenType.NormalString
                || list[index].Type == TokenType.Variable || list[index].Type == TokenType.OpenBracket)
            {
                ParseResult r1 = Expr(list, index);
                List<Node> children = new List<Node>(r1.node.Children);
                return new ParseResult(new Node(NodeType.Expr, new Token(TokenType.None, ""), children), r1.index);
            }
            //無の場合
            else if (list[index].Type == TokenType.LF || list[index].Type == TokenType.Tab)
            {
                return new ParseResult(new Node(NodeType.Expr, new Token(TokenType.None, ""), new List<Node>()), index + 1);
            }
            //ダメな場合
            else
            {
                throw new ParseErrorException(list, index, NodeType.Expr, list[index].Type);
            }
        }

        /// <summary>
        /// 行
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static ParseResult Line(List<Token> list, int index)
        {
            //そこで終了する場合
            if (index >= list.Count)
            {
                //ここで終わってもよい
                //終わった場合さらに一つ先を返す
                //空行扱い
                return new ParseResult(new Node(NodeType.Line, new Token(TokenType.None, ""), new List<Node>()), index + 1);
            }

            //最初に式が来る場合
            if (list[index].Type == TokenType.Delimiter || list[index].Type == TokenType.NormalString
                || list[index].Type == TokenType.Variable || list[index].Type == TokenType.OpenBracket)
            {
                ParseResult r1 = Expr(list, index);
                //次にもうひとつ式が続く場合とそうでない場合で場合分け
                if (r1.index - 1 >= list.Count || list[r1.index - 1].Type == TokenType.LF)
                {
                    List<Node> children = new List<Node>() { r1.node };
                    return new ParseResult(new Node(NodeType.Line, new Token(TokenType.None, ""), children), r1.index);
                }
                else if (list[r1.index - 1].Type == TokenType.Tab)
                {
                    ParseResult r2 = Line_dash(list, r1.index);
                    List<Node> children = new List<Node>() { r1.node };
                    children.AddRange(r2.node.Children);
                    return new ParseResult(new Node(NodeType.Line, new Token(TokenType.None, ""), children), r2.index);
                }
                else
                {
                    throw new ParseErrorException(list, index, NodeType.Line, list[r1.index].Type);
                }
            }
            //台詞のみの場合
            else if (list[index].Type == TokenType.Tab)
            {
                ParseResult r1 = Expr(list, index + 1);
                //この場合、キャラ名が空白であるというような処理をしたい
                //それによってタブすらない場合（字の文として扱う？）と区別する
                Node nameNode = new Node(NodeType.Expr, new Token(TokenType.None, ""),
                    new List<Node>() { new Node(NodeType.Str, new Token(TokenType.NormalString, ""), new List<Node>()) }
                    );
                List<Node> children = new List<Node>() {
                    nameNode,
                    r1.node };
                return new ParseResult(new Node(NodeType.Line, new Token(TokenType.None, ""), children), r1.index);
            }
            //ラベルの場合
            else if (list[index].Type == TokenType.Sharp)
            {
                ParseResult r1 = str(list, index + 1);
                List<Node> children = new List<Node>() { r1.node };
                return new ParseResult(new Node(NodeType.LabelLine, r1.node.Content, children), r1.index);
            }
            //空行
            else if (list[index].Type == TokenType.LF)
            {
                return new ParseResult(new Node(NodeType.Line, new Token(TokenType.None, ""), new List<Node>()), index + 1);
            }
            //ダメな場合
            else
            {
                throw new ParseErrorException(list, index, NodeType.Line, list[index].Type);
            }
        }

        /// <summary>
        /// 行'
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static ParseResult Line_dash(List<Token> list, int index)
        {
            //そこで終了する場合
            if (index >= list.Count)
            {
                //ここで終わってもよい
                //終わった場合さらに一つ先を返す
                //空行扱い
                return new ParseResult(new Node(NodeType.Line, new Token(TokenType.None, ""), new List<Node>()), index + 1);
            }

            //最初に式が来る場合
            if (list[index].Type == TokenType.Delimiter || list[index].Type == TokenType.NormalString
                || list[index].Type == TokenType.Variable || list[index].Type == TokenType.OpenBracket)
            {
                ParseResult r1 = Expr(list, index);
                if (r1.index - 1 >= list.Count || list[r1.index - 1].Type == TokenType.LF)
                {
                    List<Node> children = new List<Node>() { r1.node };
                    return new ParseResult(new Node(NodeType.Line, new Token(TokenType.None, ""), children), r1.index);
                }
                else
                {
                    throw new ParseErrorException(list, index, NodeType.Line, list[r1.index].Type);
                }
            }
            //ダメな場合
            else
            {
                throw new ParseErrorException(list, index, NodeType.Line, list[index].Type);
            }
        }

        /// <summary>
        /// スクリプト
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static ParseResult Script(List<Token> list, int index)
        {
            //そこで終了する場合
            if (index >= list.Count)
            {
                //ここで終わってもよい
                //終わった場合さらに一つ先を返す
                //空行扱い
                return new ParseResult(new Node(NodeType.Script, new Token(TokenType.None, ""), new List<Node>()), index + 1);
            }

            // CloseBracket以外全部丸投げ
            if (list[index].Type == TokenType.Delimiter || list[index].Type == TokenType.NormalString
                || list[index].Type == TokenType.Variable || list[index].Type == TokenType.OpenBracket
                || list[index].Type == TokenType.Sharp || list[index].Type == TokenType.Tab
                || list[index].Type == TokenType.LF)
            {
                ParseResult r1 = Line(list, index);
                ParseResult r2 = Script(list, r1.index);
                List<Node> children = new List<Node>() { r1.node };
                children.AddRange(r2.node.Children);
                return new ParseResult(new Node(NodeType.Line, new Token(TokenType.None, ""), children), r2.index);
            }
            //ダメな場合
            else
            {
                throw new ParseErrorException(list, index, NodeType.Script, list[index].Type);
            }
        }

        #region テスト
        /// <summary>
        /// テストを行う函数
        /// </summary>
        /// <param name="args"></param>
        static public void Test()
        {
            testStr();
            testFunc();
            testExpr();
            testLine();
            testScript();
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

        static void testStr()
        {
            bool ok = true;

            //ミスったら例外が出るのでokもなにもないが…
            var test1 = str(Lexer.Lex("hoge"), 0);
            var test2 = str(Lexer.Lex("$hoge"), 0);
            var test3 = str(Lexer.Lex("\nhoge"), 1);

            Assert("str", ok);
        }

        static void testFunc()
        {
            bool ok = true;

            //さいしょ の [ はにゅうりょくしない
            var test1 = Func(Lexer.Lex("hoge]"), 0);
            var test2 = Func(Lexer.Lex("func arg1 arg2]"), 0);
            var test3 = Func(Lexer.Lex("[inner] arg1 arg2]"), 0);
            var test4 = Func(Lexer.Lex("[inner argin] arg1 arg2]"), 0);
            var test5 = Func(Lexer.Lex("[func1 arg11] [func2 arg21 [func3 func33] arg22]]"), 0);

            Assert("Func", ok);
        }

        static void testExpr()
        {
            bool ok = true;

            var test1 = Expr(Lexer.Lex("hoge"), 0);
            var test2 = Expr(Lexer.Lex("hoge  fuga piyo"), 0);
            var test3 = Expr(Lexer.Lex("ねえ、$hero[if true 君 ちゃん]は、私のこと好き？"), 0);
            //タブの前までしか読まない（ひとつの式のパースなので）
            var test4 = Expr(Lexer.Lex("神\tごめん、間違えて君を殺してしまった。\n\n"), 0);

            Assert("Expr", ok);
        }

        static void testLine()
        {
            bool ok = true;

            var test1 = Line(Lexer.Lex("七瀬\tこんにちは"), 0);
            var test2 = Line(Lexer.Lex("七瀬\tこんにちは\n\n"), 0);
            var test3 = Line(Lexer.Lex("#七瀬との出会い"), 0);
            var test4 = Line(Lexer.Lex("#七瀬との出会い\n"), 0);
            var test5 = Line(Lexer.Lex("\tこんにちは"), 0);
            var test6 = Line(Lexer.Lex("\tこんにちは\n"), 0);
            var test7 = Line(Lexer.Lex("\n"), 0);
            var test8 = Line(Lexer.Lex("七瀬\tにゃー//これは嘘\n……なんちゃって。"), 0);
            var test9 = Line(Lexer.Lex("七瀬\n"), 0);

            Assert("Line", ok);
        }

        static void testScript()
        {
            bool ok = true;

            var test1 = Script(Lexer.Lex("七瀬\tこんにちは、$heroさん。"), 0);
            var test2 = Script(Lexer.Lex(@"[str hero 越前]//文字列変数""hero""を定義し「越前」を代入。
[bool flag1 false]//真理値変数""flag1""を定義し「false」を代入。
[set flag1 true]//一度定義した変数に代入。
//これらの函数は返り値が空文字列なので、以上の行（コメントのためにここまでで一行となっている）は空行となる。
//よってクリック待ちなどはかからずにそのまま次へ進む。

七瀬	どうして$hero君はそこまで……赤の他人の私のことを構ってくれるの…？

//選択肢
//select函数は選択肢ウィンドウを表示して、その結果を1から始まる整数で返す。
//switch函数は [switch (1から始まる整数値) (1の場合の処理) (2の場合の処理) ...] を行う函数。
//jump函数は「その行の処理が終わった次の処理をlabelの先に変更する」函数。
[int @select1 [select どうしてって言われると……なんとなく、だ。 そんなの……お前のことが好きだからに決まってる。]]
[switch $@select1 [jump badend] [jump happyend]]
//当然次のようにも書ける：
// [switch [select どうしてって言われると……なんとなく、だ。 そんなの……お前のことが好きだからに決まってる。] [jump badend] [jump happyend]]

#badend
七瀬	……うん、そうだよね。

	[cg 1 nanase_egao] でも、なんとなくそんなことができるのってすごいなって思う。//2行目以降は名前を入れる必要はないのでTAB文字で始める。

//地の文
	そう言って七瀬は笑った。
	笑顔が茜色に照らされて眩しかった。

	(略)//とはいってもあまり長い分岐は一ファイルの中のラベルで完結させないで別ファイルに飛ばすとかすると思う。

#happyend
七瀬	[cg 1 nanase_tere]えっ！？？

	七瀬は素っ頓狂な声を上げる。
	夕陽の中ただでさえ赤く見えていた表情は、みるみるうちにその色を濃くしていった。

//if函数は [if (真理値) (真の場合) (偽の場合)]といった調子。3項演算子みたいなの。
七瀬 じゃあ、もしかしてあの時[if $flag1 ぬいぐるみ 抱き枕] をくれたのって……


	(略)"), 0);

            var test3 = Script(Lexer.Lex("[func hoge]\n"), 0);
            var test4 = Script(Lexer.Lex("[func hoge]\n[func2 hoge]\tにゃん"), 0);


            Assert("Script", ok);
        }
        #endregion
    }
}
