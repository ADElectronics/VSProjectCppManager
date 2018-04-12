
namespace VSProjectCppManager.Model
{
    public class FileItem : Item
    {
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

        public string Extension { get; set; }
    }
}
