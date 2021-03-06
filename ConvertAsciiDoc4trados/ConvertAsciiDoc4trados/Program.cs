﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace ConvertAsciiDoc4trados
{
    class Program
    {

        private const string HEADHTML1 = "<html><head><meta http-equiv=&quot;Content-Type&quot;content=&quot;text/html;charset=UTF-8&quot;/></head><body><br>";
        private const string HEADHTML2 = "</body></html>";
        //private const string[] tags = { }

        static void Main(string[] args)
        {
            //Console.WriteLine(System.Environment.CommandLine)

            //Get command line with array
            string[] cmds = null;
            cmds = System.Environment.GetCommandLineArgs();

            List<string> files_asciidoc = new List<string>();
            List<string> files_htm = new List<string>();
            string cmd = null;


            foreach (string cmd_loopVariable in cmds)
            {
                cmd = cmd_loopVariable;
                //Bypass self path information
                String[] arguments = Environment.GetCommandLineArgs();
                if (!cmd.Contains(arguments[0]))
                {
                    if (Directory.Exists(cmd))
                    {
                        //Get directory contents with .asciidoc extention
                        files_asciidoc.AddRange(GetFilesMostDeep(cmd, "*.asciidoc"));
                        files_htm.AddRange(GetFilesMostDeep(cmd, "*.htm"));

                        Console.WriteLine(cmd);
                    }
                    else
                    {
                        Console.WriteLine("Specified is not a directory path.");
                    }
                }
            }

            // asciidoc
            if ((files_asciidoc.Count > 0))
            {
                string f = null;
                foreach (string f_loopVariable in files_asciidoc)
                {
                    f = f_loopVariable;
                    Console.WriteLine(f);
                }

                purse_asciiDoc(ref files_asciidoc);

            // htm
            }else if ((files_htm.Count > 0))
            {
                string f = null;
                foreach (string f_loopVariable in files_htm)
                {
                    f = f_loopVariable;
                    Console.WriteLine(f);
                }

                purse_htm(ref files_htm);
            }
            else
            {
                Console.WriteLine("There is no target asciidoc in the specified directory.");
            }

            //#If DEBUG Then
            Console.WriteLine("Push any key to continue...");
            Console.ReadKey();
            //#End If
        }

        private static string[] GetFilesMostDeep(string stRootPath, string stPattern)
        {
            System.Collections.Specialized.StringCollection hStringCollection = new System.Collections.Specialized.StringCollection();

            // このディレクトリ内のすべてのファイルを検索する
            foreach (string stFilePath in System.IO.Directory.GetFiles(stRootPath, stPattern))
            {
                hStringCollection.Add(stFilePath);
            }

            // このディレクトリ内のすべてのサブディレクトリを検索する (再帰)
            foreach (string stDirPath in System.IO.Directory.GetDirectories(stRootPath))
            {
                string[] stFilePathes = GetFilesMostDeep(stDirPath, stPattern);

                // 条件に合致したファイルがあった場合は、ArrayList に加える
                if ((stFilePathes != null))
                {
                    hStringCollection.AddRange(stFilePathes);
                }
            }

            // StringCollection を 1 次元の String 配列にして返す
            string[] stReturns = new string[hStringCollection.Count];
            hStringCollection.CopyTo(stReturns, 0);

            return stReturns;
        }

        private static void purse_asciiDoc(ref List<string> pathes)
        {
            System.Text.Encoding enc = System.Text.Encoding.GetEncoding("utf-8");

            foreach (string path in pathes)
            {
                //Get file contents as an array
                string[] lines = System.IO.File.ReadAllLines(path, enc);

                //check format
                var checkFormat = checkAsciiDocFormat(ref lines);
                if (!checkFormat)
                {
                    Console.WriteLine("\nThere is some format error in the file: " + path);
                    return;
                }

                // Delete unceccesary linebreak in asciidoc
                var newlines = deleteUnneccesaryLineBreak(ref lines);
                // for debut
                //var newlines = lines;


                StreamWriter writer = new StreamWriter(path, false, enc);
                foreach (string con in newlines)
                {
                    if(con.Equals(""))
                    {
                        writer.WriteLine(con);
                    }
                    else {
                        writer.WriteLine(con);
                    }
                }
                writer.Close();

                // 再度開く
                 lines = System.IO.File.ReadAllLines(path, enc);

            }

            foreach (string path in pathes)
            {
                //Get file contents as an array
                string[] lines = System.IO.File.ReadAllLines(path, enc);

                string[] conts = inputHtmlTag(lines);

                //Set extention
                string newFileName = System.IO.Path.ChangeExtension(path, "htm");

                //実際にファイル名を変更する
                //fileNameがない場合や、newFileNameが存在する場合は例外がスローされる
                System.IO.File.Move(path, newFileName);

                StreamWriter writer = new StreamWriter(newFileName, false, enc);
                writer.WriteLine(HEADHTML1);
                foreach (string con in conts)
                {
                    writer.WriteLine(con);
                }
                writer.WriteLine(HEADHTML2);
                writer.Close();

            }
        }

        private static void purse_htm(ref List<string> pathes)
        {
            System.Text.Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            foreach (string path in pathes)
            {
                //Get file contents as an array
                string[] conts = System.IO.File.ReadAllLines(path, enc);

                //Set extention
                string newFileName = System.IO.Path.ChangeExtension(path, "asciidoc");

                //実際にファイル名を変更する
                //fileNameがない場合や、newFileNameが存在する場合は例外がスローされる
                System.IO.File.Move(path, newFileName);


                StreamWriter writer = new StreamWriter(newFileName, false, enc);
                // トラドスに削除された改行を復活
                for(int i=0; i<conts.Length; i++)
                {
                    //Regex reg = new Regex("<[^<|>]+?> <[^<|>]+?>");
                    //Match m = reg.Match(conts[i]);
                    //if (m.Success)
                    //{
                    //    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    //    {
                    //        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "(<[^<|>]+?>) (<[^<|>]+?>)", "$1$2");
                    //        conts[i] = temp;
                    //    }
                    //}

                    Regex reg = new Regex("<br>");
                    Match m = reg.Match(conts[i]);
                    if (m.Success)
                    {
                        for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                        {
                            // <br> タグの後ろに改行がある行の "<br>" をとる
                            var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "<br>$", "");
                            conts[i] = temp;
                            // なぜかTradosから訳文生成すると、<br>の後ろにスペースが入るので削除
                            temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "<br></code> ", Environment.NewLine);
                            conts[i] = temp;
                            temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "<br> ", Environment.NewLine);
                            conts[i] = temp;
                            temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "<br>", Environment.NewLine);
                            conts[i] = temp;
                        }
                        writer.WriteLine(conts[i]);
                    }
                    else { writer.WriteLine(conts[i]); }
                }
                writer.Close();


                // 再度開きなおす
                conts = System.IO.File.ReadAllLines(newFileName, enc);
                writer = new StreamWriter(newFileName, false, enc);

                deleteTag(ref conts);
                for(int i=0; i<conts.Length; i++)
                {
                    if (i == 0)
                    {
                        // Do nothing
                        // Delete head
                    }else if (i == conts.Length-1)
                    {
                        // Do nothing
                        // Delete tail
                        break;
                    }
                    else
                    {
                        writer.WriteLine(conts[i]);
                    }
                }
                writer.Close();

            }
        }

        private static string[] inputHtmlTag(string[] lines)
        {
            string reg1 = "^##BRAL##([^##BRAL##|##BRAR##])+?##BRAR##";
            string reg111 = "^##BRAL####BRAL##.*?##BRAR####BRAR##";
            string reg2 = "^=+";
            string reg21 = "^##PLUS##$";
            string reg3 = "https?.+?##BRAL##.+?##BRAR##";
            // \{[^\{|\}]+?\}/[^\{|\}]+?\[[^\{|\}]+?\]
            //string reg31 = "##CurlybL##.+?##CurlybR##/.+?##BRAL##.+?##BRAR##";
            string reg31 = "##CurlybL##((?!##CurlybR##|##CurlybL##).)*?##CurlybR##/((?!##CurlybR##|##CurlybL##).)*?##BRAL##((?!##CurlybR##|##CurlybL##).)*?##BRAR##";
            string reg32 = "##CurlybL##.+?##CurlybR####BRAL##.+?##BRAR##";

            // Open Blocks
            string reg4 = "^-{3,}(<br>)?$";
            //string reg41 = "^-{3,}";

            // Grave accent
            string reg5 = "`.*?`";
            //////

            string reg6 = "^##ASTA##[^##ASTA##]+?##ASTA##";
            string reg61 = "^##PLUS##";
            string reg62 = "^##ASTA## ";
            string reg63 = "^##DOTT##";
            string reg64 = "^(##ASTA##){4,}";

            //string reg7 = "(\\S):{2,}$";
            //string reg71 = "(\\S):{2,} ";
            string reg8 = "^/{2}[^/]+$";
            string reg81 = "^/{3,}";
            
            string reg9 = "<<[^>]*?>>";
            string reg90 = "^>>> ";
            /////
            string reg10 = "include::.+?##BRAL##.*?##BRAR##";
            string reg101 = "image::.+?##BRAL##.*?##BRAR##";
            string reg102 = "image:.+?##BRAL##.*?##BRAR##";
            string reg103 = "link:.+?##BRAL##.*?##BRAR##";

            string reg11 = "^- ";
            string reg12 = "##PLUS##.*?##PLUS##";

            string reg13 = "^\\+$";
            string reg14 = "\\*{2,}";

            string reg15 = "^\\|==+$";

            string reg16 = ":{2,}<br>$";

            // Escape <!-- -->
            escapeExistingCommentBlock(ref lines);

            //replace ccommentblock
            replaceCommentblock(ref lines);

            // escape braket,plus,asta
            escape4Regex(ref lines);            

            escapeChar(ref lines);

            // escape Unicode char
            ecapeUnicodeChar(ref lines);

            // ^---+
            addBrTagWhenCustomSubstitutions(ref lines);

            //^-{3,}
            breaklineProcess(ref lines);

            ////////////////////////////////////
            // ^-{3,}(<br>)?$
            //inputCodeTag(reg4, ref lines);


            //replaceCodeBlockWithoutDelimiters(ref lines);

            for (int i=0; i<lines.Length; i++)
            {
                // Change formatting marks
                //escapeFormattingMarks(ref lines[i]);

                // "::"
                //replaceMach2CODE(reg7, ref lines[i]);
                //replaceMach2CODE(reg71, ref lines[i]);
                replaceDoubleColon(ref lines[i]);

                // escape space after colon
                replaceColonSpace(ref lines[i]);
                // escape space after period
                //replacePeriodSpace(ref lines[i]);
                // escape head space
                replaceHeadSpace(ref lines[i]);
                // escape table space
                //replaceTableSpace(ref lines[i]);
                // escape table bar
                replaceTableBar(ref lines[i]);
                // escape space after ?
                replaceExclamationSpace(ref lines[i]);
                replaceQuestionSpace(ref lines[i]);
                replaceAstaSpace(ref lines[i]);

                // escape multiple space
                //replaceMultipleSpace(ref lines[i]);

                // Input tag
                // Head asta
                replaceMach2CODE(reg62, ref lines[i]);
                // <<
                replaceMach2PRE(reg9, ref lines[i]);

                // "^\\[.*?\\]+";
                replaceMach2CODE_NonGroup(reg1, ref lines[i]);
                // "^\\[.*?\\]+";
                replaceMach2CODE(reg111, ref lines[i]);
                // "==+"
                replaceMach2CODE(reg2, ref lines[i]);
                // ^+$
                replaceMach2CODE(reg21, ref lines[i]);
                // "http.+?\\[.+?\\]"
                replaceMach2CODE(reg3, ref lines[i]);
                // {.+?}.+?[.+?]
                replaceMach2CODE_NonGroup2(reg31, ref lines[i]);
                replaceMach2CODE(reg32, ref lines[i]);
                // "\*"
                //replaceMach2CODE(reg6, ref lines[i]);
                replaceMach2CODE(reg61, ref lines[i]);
                replaceMach2CODE_NonGroup2(reg63, ref lines[i]);
                replaceMach2CODE_NonGroup(reg64, ref lines[i]);

                // "^include::.+\\[\\]"
                replaceMach2CODE(reg10, ref lines[i]);
                // "^image::.+\\[\\]"
                //replaceMach2CODE_NonGroup(reg101, ref lines[i]);
                replaceMach2CODE_NonGroup2(reg102, ref lines[i]);

                replaceMach2CODE_NonGroup2(reg103, ref lines[i]);

                // "^//.+"
                //replaceMach2CODE(reg8, ref lines[i]);
                // ^- 
                replaceMach2CODE(reg11, ref lines[i]);
                // +.*?+
                replaceMach2CODE(reg12, ref lines[i]);
                // ^+$
                replaceMach2CODE(reg13, ref lines[i]);
                // **
                replaceMach2CODE(reg14, ref lines[i]);
                // ::$
                replaceMach2CODE_NonGroup2(reg16, ref lines[i]);
                //replaceMachTable(reg15, ref lines[i]);
                // ^|
                //replaceMachTable(reg16, ref lines[i]);
                // escape << >> anchor
                escapeAnchor(ref lines[i]);
                // \n
                //escapeLineBreak(ref lines[i]);

            }
            replaceMachTable(reg15, ref lines);

            //linebreak
            //breaklineProcess(reg41, ref lines);

            // Unescapte Braket,plus,asta
            unescape4Regex(ref lines);

            return lines;
        }
        
        // <code>
        private static void replaceMach2CODE(string strReg, ref string line)
        {
            Regex reg = new Regex(strReg);
            Match m = reg.Match(line);
            if (m.Success)
            {
                for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                {
                    Regex reg1 = new Regex("<pre>[^ ]+?</pre>");
                    Match m1 = reg1.Match(line);
                    if (m1.Success)
                    {

                    }else
                    {
                        //////////////////////////////////////
                        var rep = "<code>" + m.Groups[ctr].Value + "</code>";
                        if (m.Groups[ctr].Value.Contains("["))
                        {
                            var str = @"\" + m.Groups[ctr].Value + @"\";
                            var temp = System.Text.RegularExpressions.Regex.Replace(line, str, rep);
                            line = temp;
                        }
                        else
                        {

                            var temp = System.Text.RegularExpressions.Regex.Replace(line, m.Groups[ctr].Value, rep);
                            line = temp;

                        }

                    }
                    
                }
            }
        }

        // <code>
        private static void replaceMach2CODE_NonGroup(string strReg, ref string line)
        {
            Regex reg = new Regex(strReg);
            Match m = reg.Match(line);
            if (m.Success)
            {

                Regex reg1 = new Regex("<pre>[^ ]+?</pre>");
                Match m1 = reg1.Match(line);
                if (m1.Success)
                {

                }
                else
                {
                    //////////////////////////////////////
                    var rep = "<code>" + m.Value + "</code>";
                    if (m.Value.Contains("["))
                    {
                        var str = @"\" + m.Value + @"\";
                        var temp = System.Text.RegularExpressions.Regex.Replace(line, str, rep);
                        line = temp;
                    }
                    else
                    {

                        var temp = System.Text.RegularExpressions.Regex.Replace(line, m.Value, rep);
                        line = temp;

                    }

                }
            }
        }

        // <code>
        private static void replaceMach2CODE_NonGroup2(string strReg, ref string line)
        {
            Regex reg = new Regex(strReg);
            Match m = reg.Match(line);
            if (m.Success)
            {

                Regex reg1 = new Regex("<pre>[^ ]+?</pre>");
                Match m1 = reg1.Match(line);
                if (m1.Success)
                {

                }
                else
                {
                    while (m.Success)
                    {
                        //////////////////////////////////////
                        var rep = "<code>" + m.Value + "</code>";
                        if (m.Value.Contains("["))
                        {
                            var str = @"\" + m.Value + @"\";
                            var temp = System.Text.RegularExpressions.Regex.Replace(line, str, rep);
                            line = temp;
                        }
                        else
                        {

                            var temp = System.Text.RegularExpressions.Regex.Replace(line, strReg, rep);
                            line = temp;

                        }
                        m = m.NextMatch();
                    }

                }
            }
        }

        // <Pre>
        private static void replaceMach2PRE(string strReg, ref string line)
        {
            Regex reg = new Regex(strReg);
            Match m = reg.Match(line);
            while (m.Success)
            {
                var rep = "<pre>" + m.Value + "</pre>";
                var temp = System.Text.RegularExpressions.Regex.Replace(line, m.Value, rep);
                line = temp;
                m = m.NextMatch();
            }
        }

        private static void replaceColonSpace(ref string line)
        {
            Regex reg = new Regex(":[\t| ]+");
            Match m = reg.Match(line);
            while (m.Success)
            {
                var temp = System.Text.RegularExpressions.Regex.Replace(line, ":([\t| ]+)", ":<pre>$1</pre>");
                line = temp;
                m = m.NextMatch();
            }
        }

        //string reg7 = "\\S:{2,}$";
        //string reg71 = "\\S:{2,} ";
        private static void replaceDoubleColon(ref string line)
        {
            Regex reg = new Regex("\\S:{2,}$");
            Match m = reg.Match(line);

            Regex reg2 = new Regex("\\S:{2,} ");
            Match m2 = reg2.Match(line);
            //while (m.Success)
            //{
                var temp = System.Text.RegularExpressions.Regex.Replace(line, "(\\S)(:{2,})$", "$1<pre>$2</pre>");
                line = temp;
                m = m.NextMatch();
            //}
            //while (m2.Success)
            //{
                temp = System.Text.RegularExpressions.Regex.Replace(line, "(\\S)(:{2,} )", "$1<pre>$2</pre>");
                line = temp;
                m = m.NextMatch();
            //}
        }

        private static void replacePeriodSpace(ref string line)
        {
            Regex reg = new Regex("##DOTT## +");
            Match m = reg.Match(line);
            while (m.Success)
            {
                var temp = System.Text.RegularExpressions.Regex.Replace(line, "##DOTT##( +)", "##DOTT##<pre>$1</pre>");
                line = temp;
                m = m.NextMatch();
            }
        }

        private static void replaceExclamationSpace(ref string line)
        {
            Regex reg = new Regex("\\! +");
            Match m = reg.Match(line);
            while (m.Success)
            {
                var temp = System.Text.RegularExpressions.Regex.Replace(line, "\\!( +)", "!<pre>$1</pre>");
                line = temp;
                m = m.NextMatch();
            }
        }

        private static void replaceQuestionSpace(ref string line)
        {
            Regex reg = new Regex("##QUEST## +");
            Match m = reg.Match(line);
            while (m.Success)
            {
                var temp = System.Text.RegularExpressions.Regex.Replace(line, "##QUEST##( +)", "##QUEST##<pre>$1</pre>");
                line = temp;
                m = m.NextMatch();
            }
        }

        private static void replaceAstaSpace(ref string line)
        {
            Regex reg = new Regex("##ASTA## +");
            Match m = reg.Match(line);
            while (m.Success)
            {
                var temp = System.Text.RegularExpressions.Regex.Replace(line, "##ASTA##( +)", "##ASTA##<pre>$1</pre>");
                line = temp;
                m = m.NextMatch();
            }
        }

        private static void replaceHeadSpace(ref string line)
        {
            Regex reg = new Regex("^[ |\t]+");
            Match m = reg.Match(line);
            while (m.Success)
            {
                var temp = System.Text.RegularExpressions.Regex.Replace(line, "^([ |\t]+)", "<pre>$1</pre>");
                line = temp;
                m = m.NextMatch();
            }
        }

        private static void replaceTableSpace(ref string line)
            {
            Regex reg = new Regex(" +\\|");
            Match m = reg.Match(line);
            //while (m.Success)
            //{
                var temp = System.Text.RegularExpressions.Regex.Replace(line, "( +\\|)", "<pre>$1</pre>");
                line = temp;
            //    m = m.NextMatch();
            //}
        }

        // Escape multiple space in string and stirng
        private static void replaceMultipleSpace(ref string line)
        {
            Regex reg = new Regex("[^.|\\s] {2,}");
            Match m = reg.Match(line);
            while (m.Success)
            {
                var temp = System.Text.RegularExpressions.Regex.Replace(line, "([^.|\\s])( {2,})", "$1<pre>$2</pre>");
                line = temp;
                m = m.NextMatch();
            }
        }

        private static void replaceTableBar(ref string line)
        {
            Regex reg = new Regex("^\\|[^=]");
            Match m = reg.Match(line);
            while (m.Success)
            {
                var temp = System.Text.RegularExpressions.Regex.Replace(line, "^\\|([^=])", "<pre>|</pre>$1");
                line = temp;
                m = m.NextMatch();
            }
        }

        // <Format>
        //private static void replaceMach2FORM_Grave_accent(string strReg, ref string line)
        //{
        //    Regex reg = new Regex(strReg);
        //    Match m = reg.Match(line);
        //    if (m.Success)
        //    {
        //        for (int ctr = 0; ctr < m.Groups.Count; ctr++)
        //        {
        //            Regex reg1 = new Regex("`");
        //            Match m1 = reg1.Match(m.Groups[ctr].Value);
        //            if (m1.Success)
        //            {
        //                var rep = "<form>" + m1.Groups[ctr].Value + "</form>";
        //                var temp = System.Text.RegularExpressions.Regex.Replace(line, "`", rep);
        //                line = temp;
        //            }

        //        }

        //    }
        //}

        // For coding text part
        private static void inputCodeTag(string strReg, ref string[] lines)
        {
            Regex reg = new Regex(strReg);
            bool findCodeHead = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], strReg))
                {
                    if (findCodeHead)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], strReg, lines[i] + "</code>");
                        lines[i] = temp;
                        findCodeHead = false;
                    }
                    else
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], strReg, "<code>" + lines[i]);
                        lines[i] = temp;
                        findCodeHead = true;
                    }
                }
            }
        }

        // For coding text part
        private static void replaceComment(string strReg, ref string[] lines)
        {
            Regex reg = new Regex(strReg);
            bool findCodeHead = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], strReg))
                {
                    if (findCodeHead)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "^(/{3,})(<br>)", "$1-->$2");
                        lines[i] = temp;
                        findCodeHead = false;
                    }
                    else
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], strReg, "<!--" + lines[i]);
                        lines[i] = temp;
                        findCodeHead = true;
                    }
                }
            }
        }

        // Escape existing Comment block
        private static void escapeExistingCommentBlock(ref string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "<!--", "##CommentBlockHead##");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "-->", "##CommentBlockTail##");
                lines[i] = temp;
            }
        }

        // comment out
        private static void replaceCommentblock(ref string[] lines)
        {
            bool findCodeHead = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^/{4,}.*?$"))
                {
                    if (findCodeHead)
                    {
                        if (i + 1 == lines.Length)
                        {
                            var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "(^/{4,}.*?$)", "$1-->");
                            lines[i] = temp;
                            findCodeHead = false;
                        }
                        else
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(lines[i + 1], "^/{4,}.*?$"))
                            {
                                continue;
                            }
                            else
                            {
                                var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "(^/{4,}.*?$)", "$1-->");
                                lines[i] = temp;
                                findCodeHead = false;
                            }
                        }
                    }
                    else
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "(^/{4,}.*?$)", "<!--$1");
                        lines[i] = temp;
                        findCodeHead = true;
                    }
                }
            }
        }

        // For code block without delimiters
        private static void replaceCodeBlockWithoutDelimiters(ref string[] lines)
        {
            bool findCodeHead = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "##BRAL##source,.+?##BRAR##"))
                {
                    if (findCodeHead == false)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "(##BRAL##source,.+?##BRAR##)<br>", "<code>$1");
                        lines[i] = temp;
                        findCodeHead = true;
                    }
                }else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^([\n| |##PLUS##]+)?<br>$"))
                {
                    if (findCodeHead)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "((^[\n| |##PLUS##]+)?<br>$)", "$1</code>");
                        lines[i] = temp;
                        findCodeHead = false;
                    }

                }else
                {
                    if (findCodeHead)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "<br>", "");
                        lines[i] = temp;
                    }
                    
                }
            }
        }

        // <table>
        private static void replaceMachTable(string strReg, ref string[] lines)
        {
            Regex reg = new Regex(strReg);
            bool findCodeHead = false;
            for (int i = 0; i < lines.Length; i++)
            {
                Regex reg1 = new Regex("<pre>.+?</pre>");
                Match m1 = reg1.Match(lines[i]);
                if (m1.Success)
                {

                }
                else
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], strReg))
                    {
                        if (findCodeHead)
                        {
                            var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], strReg, "<Table>" + lines[i] + "</Table>");
                            lines[i] = temp;
                            findCodeHead = false;
                        }
                        else
                        {
                            var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], strReg, "<Table>" + lines[i] + "</Table>");
                            lines[i] = temp;
                            findCodeHead = true;
                        }
                    }else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\|[^=]"))
                    {
                        Regex reg2 = new Regex("\\|");
                        Match m = reg2.Match(lines[i]);
                        var rep = "<tr>" + m.Value + "</tr>";
                        var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\|", rep);
                        lines[i] = temp;
                        findCodeHead = true;
                    }
                }

            }
        }

        // <br>
        private static void breaklineProcess(ref string[] lines)
        {
            bool findCodeHead = false;
            bool findCommandHead = false;

            for (int i = 0; i < lines.Length; i++)
            {
                // <br>
                Regex reg = new Regex("<br>");
                Match m = reg.Match(lines[i]);

                // Comment
                //Regex reg2 = new Regex("[ |^]///+");
                Regex reg2 = new Regex("<!--///+");
                Match m2 = reg2.Match(lines[i]);

                Regex reg3 = new Regex("^/{3,}");
                Match m3 = reg3.Match(lines[i]);

                if (m.Success)
                {

                }if (m2.Success)
                {
                    findCommandHead = true;
                    while(findCommandHead)
                    {
                        i++;
                        if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "///+-->"))
                        //if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "[ |^]///+"))
                        {
                            findCommandHead = false;
                            lines[i] += "<br>";
                            break;
                        }
                        // Delete unneseccary <br>
                        var temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "<br>$", "");
                        lines[i] = temp;
                    }   
                }if (m3.Success)
                {
                    findCommandHead = true;
                    while(findCommandHead)
                    {
                        i++;
                        if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^/{3,}"))
                        //if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "[ |^]///+"))
                        {
                            findCommandHead = false;
                            lines[i] += "<br>";
                            break;
                        }
                    }
                }
                else
                {
                    //// ^-{3,}   
                    //if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], strReg))
                    //{
                    //    if (findCodeHead)
                    //    {
                    //        findCodeHead = false;
                    //        lines[i] += "<br>";
                    //    }
                    //    else
                    //    {
                    //        findCodeHead = true;
                    //    }
                    //}
                    //else
                    //{
                    //    if (findCodeHead == false)
                    //    {
                    //        lines[i] += "<br>";
                    //    }
                    //}
                }
            }
        }
        private static void addBrTagWhenCustomSubstitutions(ref string[] lines)
        {
            Regex reg = new Regex("^-{3,}");
            for (int i = 0; i < lines.Length; i++)
            {
                Match m = reg.Match(lines[i]);

                if (m.Success)
                {
                    var temp_mach = m.Value;
                    var temp = "<code>" + lines[i];
                    lines[i] = temp;
                    var inCodeContents = true;
                    while (inCodeContents)
                    {
                        i++;
                        if (lines[i] == temp_mach)
                        {
                            lines[i] += "</code><br>";
                            break;
                        // Code 内の Comment を 翻訳可能にする
                        }else if(System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^#( |$)"))
                            {
                            lines[i - 1] += "</code><br>";
                            lines[i] += "<br>";
                            while (true)
                            {
                                i++;
                                if (lines[i] == temp_mach)
                                {
                                    lines[i] += "</code><br>";
                                    inCodeContents = false;
                                    break;
                                }
                                else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^[^#]"))
                                {
                                    var temp1 = "<code>" + lines[i];
                                    lines[i] = temp1;
                                    break;
                                }
                                lines[i] += "<br>";
                            }
                        
                        }else
                        {
                            //lines[i] += "<br>";
                        }
                    }
                }else
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^/{3,}"))
                    {

                    }else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "<!--/{3,}"))
                    {

                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^/{2} .+?$"))
                    {
                        var temp1 = "<code>" + lines[i] + "</code><br>";
                        lines[i] = temp1;
                    }
                    else
                    {
                        lines[i] += "<br>";
                    }
                        
                }
            }
        }

        private static void escapeChar(ref string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var temp = "";
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "^>>> ", "&gt;&gt;&gt; ");
                lines[i] = temp;
                // for <1> at head
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "(^<)([^<|>]+?)(>)", "&lt;$2&gt;");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "([^<])<([^<])", "$1&lt;$2");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "([^>])>([^>])", "$1&gt;$2");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "([^>|-])>$", "$1&gt;");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "( )<$", "$1&lt;");
                lines[i] = temp;
                // Do twice for like -> [cols="<,<,<,<",options="header",grid="cols"]
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "([^<])<([^<])", "$1&lt;$2");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "([^>])>([^>])", "$1&gt;$2");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "&amp;", "#amp;#");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "&apos;", "#apos;#");
                lines[i] = temp;
            }　
        }

        private static void ecapeUnicodeChar(ref string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var temp = "";
                // escapeUnicode char in document
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "(&#\\d{4})", "<pre>$1</pre>");
                lines[i] = temp;
            }
        }

        //
        // Escape braket,plus,asta
        private static void escape4Regex(ref string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var temp = "";
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\[", "##BRAL##");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\]", "##BRAR##");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\(", "##PARENTL##");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\)", "##PARENTR##");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\+", "##PLUS##");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\*", "##ASTA##");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\{", "##CurlybL##");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\}", "##CurlybR##");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\.", "##DOTT##");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\\\", "##BSLASH##");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\?", "##QUEST##");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\|", "##BARBAR##");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\^", "##HATHAT##");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "\\$", "##DOLLDOLL##");
                lines[i] = temp;
            }

        }
        //
        // Unscape braket, plus, asta
        private static void unescape4Regex(ref string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var temp = "";
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##BRAL##", @"[");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##BRAR##", @"]");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##PARENTL##", @"(");
                lines[i] = temp;
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##PARENTR##", @")");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##PLUS##", @"+");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##ASTA##", @"*");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##CurlybL##", @"{");
                lines[i] = temp;    
                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##CurlybR##", @"}");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##DOTT##", @".");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##BSLASH##", @"\");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##QUEST##", @"?");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##BARBAR##", @"|");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##HATHAT##", @"^");
                lines[i] = temp;

                temp = System.Text.RegularExpressions.Regex.Replace(lines[i], "##DOLLDOLL##", @"$");
                lines[i] = temp;
            }

        }

        private static void escapeAnchor(ref string line)
        {
            var temp = "";
            temp = System.Text.RegularExpressions.Regex.Replace(line, "<<", "&lt;&lt;");
            line = temp;
            temp = System.Text.RegularExpressions.Regex.Replace(line, ">>", "&gt;&gt;");
            line = temp;
        }

        private static void escapeLineBreak(ref string line)
        {
            //var temp = "";
            //temp = System.Text.RegularExpressions.Regex.Replace(line, Environment.NewLine, "<br>");
            //line = temp;
            Regex reg = new Regex("<br>");
            Match m = reg.Match(line);
            if (m.Success)
            {

            }
            else
            {
                line += "<br>";
            }
            
        }

        private static void escapeFormattingMarks(ref string line)
        {
            // Monospaced
            //Regex reg = new Regex("(^| )`.*?`");
            Regex reg = new Regex("`.*?`");
            Match m = reg.Match(line);
            if (m.Success)
            {
                while(m.Success)
                {
                    var rep = "##MP##";
                    var rep_cont = System.Text.RegularExpressions.Regex.Replace(m.Value, "`", rep);
                    var temp = System.Text.RegularExpressions.Regex.Replace(line, m.Value, rep_cont);
                    line = temp;
                    m = m.NextMatch();

                }

            }
            // File name
            //reg = new Regex("(^| )_.*?_");
            reg = new Regex(" _.*?_ ");
            m = reg.Match(line);
            if (m.Success)
            {
                while (m.Success)
                {
                    var rep = "##FN##";
                    var rep_cont = System.Text.RegularExpressions.Regex.Replace(m.Value, "_", rep);
                    var temp = System.Text.RegularExpressions.Regex.Replace(line, " _.*?_ ", rep_cont);
                    line = temp;
                    m = m.NextMatch();

                }

            }

            //// Monospaced
            //reg = new Regex("'.+?'");
            //m = reg.Match(line);
            //if (m.Success)
            //{
            //    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
            //    {
            //        var temp = System.Text.RegularExpressions.Regex.Replace(m.Groups[ctr].Value, "`", "$Emphasized$");
            //        var temp2 = System.Text.RegularExpressions.Regex.Replace(line, m.Groups[ctr].Value, temp);
            //        line = temp2;

            //    }

            //}
            // Strong
            //reg = new Regex("\\*.+?\\*");
            //m = reg.Match(line);
            //if (m.Success)
            //{
            //    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
            //    {
            //        var temp = System.Text.RegularExpressions.Regex.Replace(m.Groups[ctr].Value, @"\*", "$Strong$");
            //        var temp2 = System.Text.RegularExpressions.Regex.Replace(line, m.Groups[ctr].Value, temp);
            //        line = temp2;

            //    }

            //}
            // Single quoted
            //reg = new Regex("`.+?'");
            //m = reg.Match(line);
            //if (m.Success)
            //{
            //    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
            //    {
            //        var temp = System.Text.RegularExpressions.Regex.Replace(m.Groups[ctr].Value, "[`|']", "$SingleQuoted$");
            //        var temp2 = System.Text.RegularExpressions.Regex.Replace(line, m.Groups[ctr].Value, temp);
            //        line = temp2;

            //    }

            //}
            // Double quoted
            //reg = new Regex("``.+?''");
            //m = reg.Match(line);
            //if (m.Success)
            //{
            //    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
            //    {
            //        var temp = System.Text.RegularExpressions.Regex.Replace(m.Groups[ctr].Value, "[``|'']", "$DoubleQuoted$");
            //        var temp2 = System.Text.RegularExpressions.Regex.Replace(line, m.Groups[ctr].Value, temp);
            //        line = temp2;

            //    }

            //}
            // Unquoted
            //reg = new Regex("#.+?#");
            //m = reg.Match(line);
            //if (m.Success)
            //{
            //    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
            //    {
            //        var temp = System.Text.RegularExpressions.Regex.Replace(m.Groups[ctr].Value, "#", "$UQ$");
            //        var temp2 = System.Text.RegularExpressions.Regex.Replace(line, m.Groups[ctr].Value, temp);
            //        line = temp2;

            //    }

            //}
        }

        ////////////////////////////// ここから
        private static void deleteTag(ref string[] conts)
        {
            for (int i = 0; i < conts.Length; i++)
            {
                //if (i == 0)
                //{
                //    var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], HEADHTML1, "");
                //    conts[i] = temp;
                //    i++;
                //}

                Regex reg = new Regex("<.*?>");
                Match m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "<.*?>", "");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("##MP##");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "##MP##", "`");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("##FN##");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "##FN##", "_");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("\\$UQ\\$");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "\\$UQ\\$", "#");
                        conts[i] = temp;
                    }
                }
                // Trados 自身の翻訳ファイル生成時のタグ変換がうまくいっていない
                reg = new Regex(" &amp;lt;");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], " &amp;lt;", "<");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("&lt;");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "&lt;", "<");
                        conts[i] = temp;
                    }
                }
                // Trados 自身の翻訳ファイル生成時のタグ変換がうまくいっていない
                reg = new Regex(" &amp;gt;");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], " &amp;gt;", ">");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("&gt;");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "&gt;", ">");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("&apos;");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "&apos;", "'");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("&quot;");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "&quot;", "\"");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("&rsquo;");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "&rsquo;", "’");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("&mdash;");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "&mdash;", "—");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("&ndash");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "&ndash;", "–");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("&amp;");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "&amp;", "&");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("<!--///");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "<!--///", "///");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("-->");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "-->", "");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("#amp;#");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "#amp;#", "&amp;");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("#apos;#");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "#apos;#", "&apos;");
                        conts[i] = temp;
                    }
                }
                //##CommentBlockHead##
                reg = new Regex("##CommentBlockHead##");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "##CommentBlockHead##", "<!--");
                        conts[i] = temp;
                    }
                }
                reg = new Regex("##CommentBlockTail##");
                m = reg.Match(conts[i]);
                if (m.Success)
                {
                    for (int ctr = 0; ctr < m.Groups.Count; ctr++)
                    {
                        var temp = System.Text.RegularExpressions.Regex.Replace(conts[i], "##CommentBlockTail##", "-->");
                        conts[i] = temp;
                    }
                }
            }

        }

        private static bool checkAsciiDocFormat(ref string[] lines)
        {
            // - の数を確認
            string line1 = "";
            string line2 = "";
            var line1_true = true;

            Regex reg = new Regex("^-{3,}");
            for (int i = 0; i < lines.Length; i++)
            {
                Match m = reg.Match(lines[i]);

                if (m.Success)
                {
                    if (line1_true)
                    {
                        line1_true = false;
                        line1 += lines[i];
                    }else
                    {
                        line1_true = true;
                        line2 += lines[i];
                    }
                }
            }
            if (line1.Equals(line2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// 
        /// ^[^=|:|\/|-|*| |.].+[a-zA-Z,]$
        /// 
        /// 
        // asciidoc センテンス内の不要な改行を削除
        private static string[] deleteUnneccesaryLineBreak(ref string[] lines)
        {
            bool findCodeHead = false;
            List<string> newLines = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                // ////
                Regex reg = new Regex("^/{5,}");
                Match m = reg.Match(lines[i]);

                // /////+
                Regex reg1 = new Regex("^/{4}"); 
                Match m1 = reg1.Match(lines[i]);

                // ---- 
                Regex reg2 = new Regex("^-{4,}");
                Match m2 = reg2.Match(lines[i]);

                // // 
                Regex reg3 = new Regex("^/{2} .+$");
                Match m3 = reg3.Match(lines[i]);

                // Blanck line
                Regex reg4 = new Regex("^$");
                Match m4 = reg4.Match(lines[i]);

                // Blanck line
                Regex reg5 = new Regex("^\\[source,.+?\\]$");
                Match m5 = reg5.Match(lines[i]);

                if (m.Success)
                {
                    findCodeHead = true;
                    while (findCodeHead)
                    {
                        newLines.Add(lines[i]);
                        i++;
                        if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], m.Value))
                        {
                            newLines.Add(lines[i]);
                            break;
                        }
                    }
                }
                else if (m1.Success)
                {
                    findCodeHead = true;
                    while (findCodeHead)
                    {
                        newLines.Add(lines[i]);
                        i++;
                        if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^/{4}"))
                        {

                            newLines.Add(lines[i]);
                            break;
                        }
                    }
                }
                else if (m2.Success)
                {
                    findCodeHead = true;
                    while (findCodeHead)
                    {
                        newLines.Add(lines[i]);
                        i++;
                        if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^-{4,}"))
                        {
                            newLines.Add(lines[i]);
                            break;
                        }
                    }
                }
                else if (m3.Success)
                {
                    findCodeHead = true;
                    while (findCodeHead)
                    {
                        newLines.Add(lines[i]);
                        i++;
                        if (!System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^/{2} .+$"))
                        {
                            newLines.Add(lines[i]);
                            break;
                        }
                    }
                }
                // ^\\[source,.+?\\]$
                else if (m5.Success)
                {
                    findCodeHead = true;
                    while (i < lines.Length - 1)
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(lines[i+1], "^-{4,}"))
                        {
                            newLines.Add(lines[i]);
                            break;
                        }
                        else
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(lines[i + 1], "^$"))
                            {
                                newLines.Add(lines[i]);
                                break;
                            }
                            newLines.Add(lines[i]);
                            i++;

                        }
                    }
                }
                // For blank line
                else if (m4.Success)
                {
                    findCodeHead = true;
                    newLines.Add(lines[i]);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\[.+?\\]"))
                {
                    newLines.Add(lines[i]);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\+ *$"))
                {
                    newLines.Add(lines[i]);
                }
                //else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\*+ "))
                //{
                //    newLines.Add(lines[i]);
                //}
                else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\*.+\\* *$"))
                {
                    newLines.Add(lines[i]);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\[\\[.+?\\]\\]"))
                {
                    newLines.Add(lines[i]);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\={2,}"))
                {
                    newLines.Add(lines[i]);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^-{2}$"))
                {
                    newLines.Add(lines[i]);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^~{3,}"))
                {
                    newLines.Add(lines[i]);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^={1,} "))
                {
                    newLines.Add(lines[i]);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "(^type:|^example:|^format:|^required:)"))
                {
                    while (i < lines.Length - 1)
                    {
                        newLines.Add(lines[i]);
                        i++;
                        if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\[.+?\\]"))
                        {
                            //newLines.Add(lines[i]);
                            i--;
                            break;
                        }
                    }
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^[^=|^:|^\\/|^\\-|^ ].+[a-zA-Z, ]$") ||
                    System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^[^(type:|example:|format:|required:|={1,} )]"))
                {
                    var temp = "";
                    while (i < lines.Length)
                    {

                        if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "(\\.|:|\\?|!|\\.\\)) *$"))
                        {
                            if (temp == "")
                            {
                                temp += lines[i];
                            }
                            else
                            {
                                temp += " " + lines[i];
                            }
                            newLines.Add(temp);
                            break;
                        }
                        else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^$"))
                        {
                            if (temp == "")
                            {
                                newLines.Add(lines[i]);
                            }
                            else
                            {
                                newLines.Add(temp);
                                newLines.Add(lines[i]);
                            }
                            break;
                        }
                        else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^~{3,}"))
                        {
                            if (temp == "")
                            {
                                newLines.Add(lines[i]);
                            }
                            else
                            {
                                newLines.Add(temp);
                                newLines.Add(lines[i]);
                            }
                            break;
                        }
                        else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\[.+?\\]$"))
                        {
                            if (temp == "")
                            {
                                newLines.Add(lines[i]);
                            }
                            else
                            {
                                newLines.Add(temp);
                                newLines.Add(lines[i]);
                            }
                            break;
                        }
                        else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^={4,}$"))
                        {
                            if (temp == "")
                            {
                                newLines.Add(lines[i]);
                            }
                            else
                            {
                                newLines.Add(temp);
                                newLines.Add(lines[i]);
                            }
                            break;
                        }
                        else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\+{1} ?$"))
                        {
                            if (temp == "")
                            {
                                newLines.Add(lines[i]);
                            }
                            else
                            {
                                newLines.Add(temp);
                                newLines.Add(lines[i]);
                            }
                            break;
                        }
                        //else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\. "))
                        //{
                        //    if (!temp.Equals(""))
                        //    {
                        //        newLines.Add(lines[i]);
                        //    }
                        //    newLines.Add(lines[i]);
                        //    temp = "";
                        //}
                        //////////////////////////
                        ////////////////////////// ここの修正が必要
                        else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^\\*+ "))
                        {
                            if (i == lines.Length - 1)
                            {
                                if (temp == "")
                                {
                                    newLines.Add(lines[i]);
                                }
                                else
                                {
                                    newLines.Add(temp);
                                    newLines.Add(lines[i]);
                                }
                                break;
                            }
                            else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i+1], "^\\*+ "))
                            {
                                if (temp == "")
                                {
                                    newLines.Add(lines[i]);
                                }
                                else
                                {
                                    newLines.Add(temp);
                                    newLines.Add(lines[i]);
                                }
                                break;
                            }
                            else if (System.Text.RegularExpressions.Regex.IsMatch(lines[i + 1], "^--$"))
                            {
                                if (temp == "")
                                {
                                    newLines.Add(lines[i]);
                                }
                                else
                                {
                                    newLines.Add(temp);
                                    newLines.Add(lines[i]);
                                }
                                break;
                            }
                            else if(System.Text.RegularExpressions.Regex.IsMatch(lines[i+1], "^/{2}")) 
                            {
                                if (temp == "")
                                {
                                    newLines.Add(lines[i]);
                                }
                                else
                                {
                                    newLines.Add(temp);
                                    newLines.Add(lines[i]);
                                }
                                break;
                            }
                            else
                            {
                                if (temp == "")
                                {
                                    temp += lines[i];
                                }
                                else
                                {
                                    newLines.Add(temp);
                                    temp = "";
                                    temp += lines[i];
                                }
                                //temp += lines[i];
                            }
                            
                        }
                        else if (temp == "")
                        {
                            temp += lines[i];
                            //if (lines[i + 1].Equals(""))
                            //{
                            //    newLines.Add(temp);
                            //    break;
                            //}
                            //else
                            //{
                            //    temp += " " + lines[i + 1];
                            //    i++;
                            //}
                            if (i == lines.Length - 1)
                            {
                                newLines.Add(temp);
                            }

                        }
                        else
                        {
                            temp += " " + lines[i];
                            //temp += " " + lines[i + 1];
                            //i++;
                            if (i == lines.Length - 1)
                            {
                                newLines.Add(temp);
                            }

                        }
                        i++;
                    }
                }

                else
                {
                    newLines.Add(lines[i]);
                }
            }
            return newLines.ToArray();
        }
    }
}
    