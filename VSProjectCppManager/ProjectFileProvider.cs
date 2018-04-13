using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VSProjectCppManager
{
    public static class ProjectFileProvider
    {
        static XmlDocument xDoc;

        public static void LoadDocument(string path)
        {
            xDoc = new XmlDocument();
            xDoc.Load(path);
        }

        public static void LoadDocument(XmlDocument doc)
        {
            xDoc = doc;
        }




    }
}
