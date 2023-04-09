using System;
using System.IO;
using System.Text.RegularExpressions;

namespace grep_sql_Sample
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    class Program
    {
        static void Main(string[] args)
        {
            // 解析するフォルダのパスを指定
            string folderPath = @"C:\WORK";

            // 解析結果を出力するファイルのパスを指定
            string outputFilePath = @"C:\WORK\GREP.txt";

            // フォルダ内のSQLファイルを再帰的に検索し、ファイルパスを取得
            string[] filePaths = Directory.GetFiles(folderPath, "*.sql", SearchOption.AllDirectories);

            // 出力するテキストファイルを作成
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                foreach (string filePath in filePaths)
                {
                    // SQLファイルを読み込む
                    string sql = File.ReadAllText(filePath);

                    // float型の変数を定義している箇所を抽出
                    MatchCollection matches = Regex.Matches(sql, @"declare\s+@([\w_]+)\s+float", RegexOptions.IgnoreCase);

                    // ファイルパスと定義箇所を出力
                    foreach (Match match in matches)
                    {
                        string variableName = match.Groups[1].Value;
                        int lineNumber = GetLineNumber(sql, match.Index);
                        string lineText = GetLineText(sql, match.Index);
                        writer.WriteLine("{0}({1}):{2}", filePath, lineNumber, lineText);
                    }

                    // 変数が使用されている箇所を出力
                    foreach (Match match in matches)
                    {
                        string variableName = match.Groups[1].Value;
                        string pattern = string.Format(@"\b{0}\b", variableName);
                        MatchCollection usageMatches = Regex.Matches(sql, pattern, RegexOptions.IgnoreCase);
                        foreach (Match usageMatch in usageMatches)
                        {
                            int lineNumber = GetLineNumber(sql, usageMatch.Index);
                            string lineText = GetLineText(sql, usageMatch.Index);
                            writer.WriteLine("{0}({1}):{2}", filePath, lineNumber, lineText);
                        }
                    }
                }
            }
        }

        // 指定したインデックスの行番号を返す
        static int GetLineNumber(string text, int index)
        {
            return text.Substring(0, index).Split('\n').Length;
        }

        // 指定したインデックスが含まれる行のテキストを返す
        static string GetLineText(string text, int index)
        {
            int lineStartIndex = text.LastIndexOf('\n', index) + 1;
            int lineEndIndex = text.IndexOf('\n', index);
            if (lineEndIndex < 0)
            {
                lineEndIndex = text.Length;
            }
            return text.Substring(lineStartIndex, lineEndIndex - lineStartIndex).Trim();
        }
    }
}
