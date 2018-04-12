using Prism.Mvvm;
using System.Collections.Generic;

namespace VSProjectCppManager.Model
{
    public class Item : BindableBase
    {
        public string Name { get; set; }
        public string Path { get; set; }
        //public bool Selected { get; set; }
        public List<Item> Items { get; set; }
    }
}
