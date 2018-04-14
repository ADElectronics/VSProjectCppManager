using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using VSProjectCppManager.Models;

namespace VSProjectCppManager
{
    public class FilterFileProvider
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

        public void UpdateItems()
        {
            XmlElement DocElement = xDoc.DocumentElement;
            Items.Clear();

            foreach (XmlNode Node in DocElement) // ItemGroup
            {
                var NodeItem = new XDocItem
                {
                    Name = Node.Name.ToString(),
                    Value = Node.Value,
                    //Path = 
                };

                Items.Add(NodeItem);

                foreach (XmlNode ChildNode in Node.ChildNodes) // Filter, None, ClInclude, ClCompile
                {
                    string ChildValueData = String.Empty;
                    if (ChildNode.Attributes != null && ChildNode.Attributes.Count > 0)
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

        public void GenerateFrom(ObservableCollection<Item> dirsFiles)
        {
            xDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xDoc.DocumentElement;
            xDoc.InsertBefore(xmlDeclaration, root);

            XmlElement rootElement = xDoc.CreateElement(string.Empty, "Project", string.Empty);
            rootElement.SetAttribute("ToolsVersion", "4.0");
            rootElement.SetAttribute("xmlns", "http://schemas.microsoft.com/developer/msbuild/2003");
            xDoc.AppendChild(rootElement);

            // Filter - все корневые и вложенные папки, отображаем как фильтры
            // None - все дополнительные файлы, не компилируемые 
            // ClInclude - компилируемые h, hpp файлы (и другие?....)
            // ClCompile - компилируемые с, срр файлы (и другие?....)

            for (byte i = 0; i < 4; i++) // без цикла никуда и никак) шутка. 
            {
                XmlElement itemGroupElement = xDoc.CreateElement(string.Empty, "ItemGroup", string.Empty);
                rootElement.AppendChild(itemGroupElement);

                switch (i)
                {
                    case 0: // ItemGroup - None - Filter
                        
                        void NextDItem(Item RootItem)
                        {

                            if(RootItem.Selected & RootItem is FileItem)
                            {
                                //if (!extensionsClInclude.Any(rootitem.Name.ToLower().Contains) & !extensionsClCompile.Any(rootitem.Name.ToLower().Contains))
                                if (extensionsNone.Any(RootItem.Name.ToLower().Contains))
                                {                               
                                    XmlElement noneElement = xDoc.CreateElement(string.Empty, "None", string.Empty);
                                    noneElement.SetAttribute("Include", RootItem.Path);
                                    itemGroupElement.AppendChild(noneElement);

                                    if(((FileItem)RootItem).PathToFile != String.Empty)
                                    {
                                        XmlElement noneFilterElement = xDoc.CreateElement(string.Empty, "Filter", string.Empty);
                                        XmlText noneFilterText = xDoc.CreateTextNode(((FileItem)RootItem).PathToFile);
                                        noneFilterElement.AppendChild(noneFilterText);
                                        noneElement.AppendChild(noneFilterElement);
                                    }                   
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

                        break;

                    case 1: // ItemGroup - Filter - UniqueIdentifier

                        void NextFilter(DirectoryItem rootitem)
                        {
                            if(rootitem.Selected)
                            { 
                                Guid UI = Guid.NewGuid();
                                XmlElement FElement = xDoc.CreateElement(string.Empty, "Filter", string.Empty);
                                FElement.SetAttribute("Include", rootitem.Path);
                                itemGroupElement.AppendChild(FElement);

                                XmlElement UIElement = xDoc.CreateElement(string.Empty, "UniqueIdentifier", string.Empty);
                                XmlText UIText = xDoc.CreateTextNode(UI.ToString());
                                UIElement.AppendChild(UIText);
                                FElement.AppendChild(UIElement);
                            }

                            foreach (DirectoryItem item in rootitem.Items.OfType<DirectoryItem>())
                            {
                                NextFilter(item);
                            }
                        }

                        foreach (DirectoryItem item in dirsFiles.OfType<DirectoryItem>())
                        {
                            NextFilter(item);
                        }

                        break;

                    case 2: // ItemGroup - ClInclude - Filter

                        void NextClIncItem(Item rootitem)
                        {
                            if (rootitem is FileItem)
                            {
                                if (rootitem.Selected & extensionsClInclude.Any(rootitem.Name.ToLower().Contains))
                                {

                                    XmlElement clIncludeElement = xDoc.CreateElement(string.Empty, "ClInclude", string.Empty);
                                    clIncludeElement.SetAttribute("Include", rootitem.Path);
                                    itemGroupElement.AppendChild(clIncludeElement);

                                    if (((FileItem)rootitem).PathToFile != String.Empty)
                                    {
                                        XmlElement clIncludeFilterElement = xDoc.CreateElement(string.Empty, "Filter", string.Empty);
                                        XmlText clIncludeFilterText = xDoc.CreateTextNode(((FileItem)rootitem).PathToFile);
                                        clIncludeFilterElement.AppendChild(clIncludeFilterText);
                                        clIncludeElement.AppendChild(clIncludeFilterElement);
                                    }
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
                        break;

                    case 3: // ItemGroup - ClCompile - Filter

                        void NextClComItem(Item rootitem)
                        {
                            if (rootitem is FileItem)
                            {
                                if (rootitem.Selected & extensionsClCompile.Any(rootitem.Name.ToLower().Contains))
                                {
                                    XmlElement clCompileElement = xDoc.CreateElement(string.Empty, "ClCompile", string.Empty);
                                    clCompileElement.SetAttribute("Include", rootitem.Path);
                                    itemGroupElement.AppendChild(clCompileElement);

                                    if (((FileItem)rootitem).PathToFile != String.Empty)
                                    {
                                        XmlElement clCompileFilterElement = xDoc.CreateElement(string.Empty, "Filter", string.Empty);
                                        XmlText clCompileFilterText = xDoc.CreateTextNode(((FileItem)rootitem).PathToFile);
                                        clCompileFilterElement.AppendChild(clCompileFilterText);
                                        clCompileElement.AppendChild(clCompileFilterElement);
                                    }
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
                        break;

                    default:

                        break;
                }
            }

            // Для отладки
            xDoc.Save("FiltersFile_Debug.xml");
        }
        #endregion
    }
}
