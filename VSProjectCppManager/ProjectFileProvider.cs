using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VSProjectCppManager.Models;

namespace VSProjectCppManager
{
    public class ProjectFileProvider
    {
        XmlDocument xDoc;
        // Пока такое жесткое разделение на типы файлов вне зависимости от первичной фильтрации
        static string[] extensionsNone = new string[] { ".mk", "makefile", ".s", ".ld", ".xaml", ".txt", ".a", ".py" };
        static string[] extensionsClInclude = new string[] { ".hpp", ".h" };
        static string[] extensionsClCompile = new string[] { ".cpp", ".c", ".asm" };

        #region Публичные свойства
        public ObservableCollection<XDocItem> Items { get; set; } = new ObservableCollection<XDocItem>();
        #endregion

        #region Публичные методы
        public void LoadFrom(string path)
        {
            xDoc = new XmlDocument();
            xDoc.Load(path);
        }

        public void LoadFrom(XmlDocument doc)
        {
            xDoc = doc;
        }

        public void SaveTo(string path)
        {
            xDoc.Save(path);
        }

        public void UpdateWith(ObservableCollection<Item> dirsFiles)
        {
            // Локальная функция простого поиска\создания указанных элементов в документе
            XmlElement FindElement(XmlDocument Doc, string NameOfChild, string NameOfElements)
            {
                // Получаем список елементов ItemGroup, в которых и хранятся списки файлов
                XmlElement[] ItemGroupList = Doc.DocumentElement.GetElementsByTagName(NameOfElements).Cast<XmlElement>().ToArray<XmlElement>();
                XmlElement element;

                foreach (XmlElement Element in ItemGroupList)
                {
                    if (Element.HasChildNodes & Element.ChildNodes[0].Name == NameOfChild)
                    {
                        element = Element;
                        return element;
                    }
                }

                // не найден
                element = Doc.CreateElement(string.Empty, NameOfElements, string.Empty);
                return element;
            }

            XmlElement RootNoneElement = FindElement(xDoc, "None", "ItemGroup");
            XmlElement RootClIncludeElement = FindElement(xDoc, "ClInclude", "ItemGroup");
            XmlElement RootClCompileElement = FindElement(xDoc, "ClCompile", "ItemGroup");

            // Пока что просто очищаем всё и добавляем выбранное...
            // В дальнейшем надо сделать корректную проверку на наличие элементов и просто пропуск существующих уже
            void DeleteAllChildNodes(XmlElement elem)
            {
                if(elem.HasChildNodes)
                {
                    elem.RemoveAll();
                }                 
            }

            DeleteAllChildNodes(RootNoneElement);
            DeleteAllChildNodes(RootClIncludeElement);
            DeleteAllChildNodes(RootClCompileElement);

            // None - все дополнительные файлы, не компилируемые 
            void NextDItem(Item RootItem)
            {

                if (RootItem.Selected & RootItem is FileItem)
                {
                    //if (!extensionsClInclude.Any(rootitem.Name.ToLower().Contains) & !extensionsClCompile.Any(rootitem.Name.ToLower().Contains))
                    if (extensionsNone.Any(RootItem.Name.ToLower().Contains))
                    {
                        XmlElement noneElement = xDoc.CreateElement(string.Empty, "None", string.Empty);
                        noneElement.SetAttribute("Include", RootItem.Path);
                        RootNoneElement.AppendChild(noneElement);
                    }
                }
                else if (RootItem is DirectoryItem)
                {
                    foreach (Item item in RootItem.Items.OfType<Item>())
                    {
                        NextDItem(item);
                    }
                }
            }

            foreach (Item item in dirsFiles.OfType<Item>())
            {
                NextDItem(item);
            }

            // ClInclude - компилируемые h, hpp файлы (и другие?....)
            void NextClIncItem(Item rootitem)
            {
                if (rootitem is FileItem)
                {
                    if (rootitem.Selected & extensionsClInclude.Any(rootitem.Name.ToLower().Contains))
                    {

                        XmlElement clIncludeElement = xDoc.CreateElement(string.Empty, "ClInclude", string.Empty);
                        clIncludeElement.SetAttribute("Include", rootitem.Path);
                        RootClIncludeElement.AppendChild(clIncludeElement);
                    }
                }
                else if (rootitem is DirectoryItem)
                {
                    foreach (Item item in rootitem.Items.OfType<Item>())
                    {
                        NextClIncItem(item);
                    }
                }
            }

            foreach (Item item in dirsFiles.OfType<Item>())
            {
                NextClIncItem(item);
            }

            // ClCompile - компилируемые с, срр файлы (и другие?....)
            void NextClComItem(Item rootitem)
            {
                if (rootitem is FileItem)
                {
                    if (rootitem.Selected & extensionsClCompile.Any(rootitem.Name.ToLower().Contains))
                    {
                        XmlElement clCompileElement = xDoc.CreateElement(string.Empty, "ClCompile", string.Empty);
                        clCompileElement.SetAttribute("Include", rootitem.Path);
                        RootClCompileElement.AppendChild(clCompileElement);
                    }
                }
                else if (rootitem is DirectoryItem)
                {
                    foreach (Item item in rootitem.Items.OfType<Item>())
                    {
                        NextClComItem(item);
                    }
                }
            }

            foreach (Item item in dirsFiles.OfType<Item>())
            {
                NextClComItem(item);
            }

            // Для отладки
            xDoc.Save("ProjectFile_Debug.xml");
        }

        public void UpdateItems()
        {
            XmlElement DocElement = xDoc.DocumentElement;
            Items.Clear();

            foreach (XmlNode Node in DocElement)
            {
                var NodeItem = new XDocItem
                {
                    Name = Node.Name.ToString(),
                    Value = Node.Value,
                    //Path = 
                };

                Items.Add(NodeItem);

                foreach (XmlNode ChildNode in Node.ChildNodes)
                {
                    string ChildValueData = String.Empty;
                    if(ChildNode.Attributes != null && ChildNode.Attributes.Count > 0)
                    {
                        ChildValueData = ChildNode.Attributes[0].Name + "=" + ChildNode.Attributes[0].Value;
                    }

                    var ChildItem = new XDocItem
                    {
                        Name = ChildNode.Name.ToString(),
                        Value = ChildValueData,
                    };
                    
                    Items[Items.Count - 1].Items.Add(ChildItem);

                    if (ChildNode.ChildNodes.Count > 0)
                    {
                        XmlNode IntChildNode = ChildNode.ChildNodes[0];

                        ChildItem = new XDocItem
                        {
                            Name = IntChildNode.Name.ToString(),
                            Value = IntChildNode.InnerText,
                        };

                        Int32 ItemsCount = Items[Items.Count - 1].Items.Count - 1;
                        Items[Items.Count - 1].Items[ItemsCount].Items.Add(ChildItem);
                    }
                }
            }
            
        }
#endregion
    }
}
