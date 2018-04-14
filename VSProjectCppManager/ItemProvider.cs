using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VSProjectCppManager.Models;

namespace VSProjectCppManager
{
    public class ItemProvider
    {
        // Задаются пользователем
        string[] extensions = new string[] { ".c", ".h", ".mk", "makefile" };

        #region Публичные свойства
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
        public string CutPathToProject { get; set; } = String.Empty;
        #endregion

        #region Публичные методы
        public void SetExtensions(string ext)
        {
            char[] delimiters = new char[] { ',', ' ', ';', ':' };
            ext = ext.RemoveChars('*');
            extensions = ext.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        }
       
        public void GetItems(string path)
        {
            Items.Clear();

            foreach(Item i in UpdateItems(path))
            {
                Items.Add(i);
            }

            List<Item> UpdateItems(string p)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(p);
                List<Item> NewItems = new List<Item>();

                foreach (DirectoryInfo directory in dirInfo.GetDirectories())
                {
                    var item = new DirectoryItem
                    {
                        Name = directory.Name,
                        Path = directory.FullName.Replace(CutPathToProject + "\\", String.Empty),
                        Selected = true,
                        Items = UpdateItems(directory.FullName),
                    };

                    NewItems.Add(item);
                }

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    if (extensions.Any(file.Extension.ToLower().Contains) | extensions.Any(file.Name.ToLower().Contains))
                    {
                        FileItem item;

                        if (String.Equals(CutPathToProject.ToLower(), file.Directory.FullName.ToLower()))
                        {
                            item = new FileItem
                            {
                                Name = file.Name,
                                Path = file.FullName.Replace(CutPathToProject + "\\", String.Empty),
                                PathToFile = String.Empty,
                                Selected = true
                            };
                        }
                        else
                        {
                            item = new FileItem
                            {
                                Name = file.Name,
                                Path = file.FullName.Replace(CutPathToProject + "\\", String.Empty),
                                PathToFile = file.Directory.FullName.Replace(CutPathToProject + "\\", String.Empty),
                                Selected = true
                            };
                        }

                        NewItems.Add(item);
                    }
                }

                return NewItems;
            }           
        }
        #endregion
    }
}
