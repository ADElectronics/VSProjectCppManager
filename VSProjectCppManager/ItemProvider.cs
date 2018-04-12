using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VSProjectCppManager.Model;

namespace VSProjectCppManager
{
    public static class ItemProvider
    {
        static string[] extensions = new string[] { ".c", ".h", ".mk", "makefile" };

        public static string CutPathToProject { get; set; } = String.Empty;
        public static string RemoveChars(this string input, params char[] chars)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (!chars.Contains(input[i]))
                    sb.Append(input[i]);
            }
            return sb.ToString();
        }

        public static void SetExtensions(string ext)
        {
            char[] delimiters = new char[] { ',', ' ', ';', ':' };
            ext = ext.RemoveChars('*');
            extensions = ext.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        }

        public static List<Item> GetItems(string path)
        {
            List<Item> items = new List<Item>();
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            foreach (DirectoryInfo directory in dirInfo.GetDirectories())
            {
                var item = new DirectoryItem
                {
                    Name = directory.Name,
                    Path = directory.FullName.Replace(CutPathToProject + "\\", String.Empty),
                    Selected = true,
                    Items = GetItems(directory.FullName),
                };

                items.Add(item);
            }

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                if (extensions.Any(file.Extension.ToLower().Contains) | extensions.Any(file.Name.ToLower().Contains))
                {
                    FileItem item = new FileItem
                    {
                        Name = file.Name,
                        Path = file.FullName.Replace(CutPathToProject + "\\", String.Empty),
                        Selected = true
                    };

                    items.Add(item);
                }
            }

            return items;
        }
    }
}
