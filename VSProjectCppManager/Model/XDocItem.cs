
using System.Collections.Generic;

namespace VSProjectCppManager.Model
{
    public class XDocItem : Item
    {
        public string Value { get; set; }
        //public List<Item> Items { get; set; }
        public XDocItem()
        {
            Items = new List<Item>();
        }
    }
}
