using Prism.Mvvm;
using System.Collections.Generic;

namespace VSProjectCppManager.Models
{
    public class Item : BindableBase
    {
        public string Name { get; set; }
        public string Path { get; set; }
        //public bool Selected { get; set; }
        public List<Item> Items { get; set; }

        bool _Selected;
        public bool Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                if (_Selected != value)
                {
                    _Selected = value;
                    RaisePropertyChanged("Selected");
                }
            }
        }
    }
}
