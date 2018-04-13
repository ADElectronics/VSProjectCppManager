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
    public static class XDocProvider
    {
        static string[] extensionsNone = new string[] { ".mk", "makefile" };
        static string[] extensionsClInclude = new string[] { ".hpp", ".h" };
        static string[] extensionsClCompile = new string[] { ".cpp", ".c", ".asm" };

        // https://stackoverflow.com/questions/2294882/how-to-create-treeview-from-xml-file-using-wpf
        public static List<XDocItem> GetItems(string path)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);

            return GetItems(xDoc);
        }
        public static List<XDocItem> GetItems(XmlDocument xDoc)
        {
            List<XDocItem> items = new List<XDocItem>();
            XmlElement element = xDoc.DocumentElement;

            foreach (XmlNode xnode in element) // ItemGroup
            {
                var item = new XDocItem
                {
                    Name = xnode.Name.ToString(),
                    Value = xnode.Value,
                    //Path = xnode.Name.ToString()
                };

                items.Add(item);

                foreach (XmlNode childnode in xnode.ChildNodes) // Filter, None, ClInclude, ClCompile
                {
                    string nn = childnode.Attributes[0].Name; // Include
                    string nnv = childnode.Attributes[0].Value;

                    var childitem = new XDocItem
                    {
                        Name = childnode.Name.ToString(),
                        Value = nn + "=" + nnv,
                    };

                    items[items.Count - 1].Items.Add(childitem);

                    switch (childnode.Name)
                    {
                        case "Filter": // 1

                            if (childnode.ChildNodes.Count > 0)
                            {
                                XmlNode fnode = childnode.ChildNodes[0]; // UniqueIdentifier

                                // Guid g = Guid.NewGuid();
                                childitem = new XDocItem
                                {
                                    Name = fnode.Name.ToString(),
                                    Value = fnode.InnerText,
                                };

                                Int32 count = items[items.Count - 1].Items.Count - 1;
                                items[items.Count - 1].Items[count].Items.Add(childitem);
                            }
                            break;

                        case "None": // 2 
                        //    break;

                        case "ClInclude": // 3
                        //    break;

                        case "ClCompile": // 4
                        //    break;

                        default:

                            if (childnode.ChildNodes.Count > 0)
                            {
                                XmlNode fnode = childnode.ChildNodes[0]; // Filter

                                childitem = new XDocItem
                                {
                                    Name = fnode.Name.ToString(),
                                    Value = fnode.InnerText,
                                };

                                Int32 count = items[items.Count - 1].Items.Count - 1;
                                items[items.Count - 1].Items[count].Items.Add(childitem);
                            }

                            break;
                    }
                }
            }

            XmlNodeList childnodes = element.SelectNodes("ItemGroup");
            foreach (XmlNode n in childnodes)
                Debug.WriteLine(n.InnerText);

            return items;
        }

        public static XmlDocument GenerateItems(List<Item> dirsFiles)//, string[] extFilter)
        {
            XmlDocument xDoc = new XmlDocument();
            // По хорошему надо еще научится грузить файл и обновлять значения в нём, а не переписывать всё
            //xDoc.Load("XMLFile2.xml");
            XmlDeclaration xmlDeclaration = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xDoc.DocumentElement;
            xDoc.InsertBefore(xmlDeclaration, root);

            XmlElement rootElement = xDoc.CreateElement(string.Empty, "Project", string.Empty);
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
                        
                        void NextDItem(Item rootitem)
                        {

                            if(rootitem.Selected & rootitem is FileItem)
                            {
                                //if (!extensionsClInclude.Any(rootitem.Name.ToLower().Contains) & !extensionsClCompile.Any(rootitem.Name.ToLower().Contains))
                                if (extensionsNone.Any(rootitem.Name.ToLower().Contains))
                                {                               
                                    XmlElement noneElement = xDoc.CreateElement(string.Empty, "None", string.Empty);
                                    noneElement.SetAttribute("Include", rootitem.Path);
                                    itemGroupElement.AppendChild(noneElement);

                                    if(((FileItem)rootitem).PathToFile != String.Empty)
                                    {
                                        XmlElement noneFilterElement = xDoc.CreateElement(string.Empty, "Filter", string.Empty);
                                        XmlText noneFilterText = xDoc.CreateTextNode(((FileItem)rootitem).PathToFile);
                                        noneFilterElement.AppendChild(noneFilterText);
                                        noneElement.AppendChild(noneFilterElement);
                                    }                   
                                }
                            }
                            else if (rootitem is DirectoryItem)
                            {
                                foreach (Item item in rootitem.Items.OfType<Item>())
                                {
                                    NextDItem((Item)item);
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
                                Guid uniqueIdentifier = Guid.NewGuid();
                                XmlElement filterElement = xDoc.CreateElement(string.Empty, "Filter", string.Empty);
                                filterElement.SetAttribute("Include", rootitem.Path);
                                itemGroupElement.AppendChild(filterElement);

                                XmlElement uniqueIdentifierElement = xDoc.CreateElement(string.Empty, "UniqueIdentifier", string.Empty);
                                XmlText uniqueIdentifierText = xDoc.CreateTextNode(uniqueIdentifier.ToString());
                                uniqueIdentifierElement.AppendChild(uniqueIdentifierText);
                                filterElement.AppendChild(uniqueIdentifierElement);
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
                                    NextClIncItem((Item)item);
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
                                    NextClComItem((Item)item);
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
            xDoc.Save("XMLFile_Debug.xml");

            return xDoc;
        }

    }
}
