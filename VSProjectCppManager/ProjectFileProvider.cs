using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        #region Публичные свойства
        public ObservableCollection<XDocItem> Items { get; set; } = new ObservableCollection<XDocItem>();

        #endregion      

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
            XmlElement element = xDoc.DocumentElement;

            Items.Clear();

            foreach (XmlNode xnode in element) // ItemGroup
            {
                var item = new XDocItem
                {
                    Name = xnode.Name.ToString(),
                    Value = xnode.Value,
                    //Path = xnode.Name.ToString()
                };

                Items.Add(item);

                foreach (XmlNode childnode in xnode.ChildNodes) // Filter, None, ClInclude, ClCompile
                {
                    string data = String.Empty;
                    if(childnode.Attributes != null && childnode.Attributes.Count > 0)
                    {
                        data = childnode.Attributes[0].Name + "=" + childnode.Attributes[0].Value;
                    }

                    var childitem = new XDocItem
                    {
                        Name = childnode.Name.ToString(),
                        Value = data,
                    };
                    
                    Items[Items.Count - 1].Items.Add(childitem);

                    switch (childnode.Name)
                    {
                        case "Filter": // 1

                            if (childnode.ChildNodes.Count > 0)
                            {
                                XmlNode fnode = childnode.ChildNodes[0];

                                childitem = new XDocItem
                                {
                                    Name = fnode.Name.ToString(),
                                    Value = fnode.InnerText,
                                };

                                Int32 count = Items[Items.Count - 1].Items.Count - 1;
                                Items[Items.Count - 1].Items[count].Items.Add(childitem);
                            }
                            break;

                        default:

                            if (childnode.ChildNodes.Count > 0)
                            {
                                XmlNode fnode = childnode.ChildNodes[0]; // Filter

                                childitem = new XDocItem
                                {
                                    Name = fnode.Name.ToString(),
                                    Value = fnode.InnerText,
                                };

                                Int32 count = Items[Items.Count - 1].Items.Count - 1;
                                Items[Items.Count - 1].Items[count].Items.Add(childitem);
                            }

                            break;
                    }
                }
            }
        }
    }
}
