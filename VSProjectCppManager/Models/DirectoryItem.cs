using System.Collections.Generic;

namespace VSProjectCppManager.Models
{
    public class DirectoryItem : Item
    {
        //public List<Item> Items { get; set; }
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

                    foreach(Item item in Items)
                    {
                        if (item is DirectoryItem)
                        {
                            var i = item as DirectoryItem;
                            i.Selected = value;
                        }

                        if (item is FileItem)
                        {
                            var i = item as FileItem;
                            i.Selected = value;
                        }
                    }
                }              
            }
        }
        public DirectoryItem()
        {
            Items = new List<Item>();
        }
    }
}
