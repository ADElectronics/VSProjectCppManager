using System.Windows;
using VSProjectCppManager.ViewModels;

namespace VSProjectCppManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }
    }
}
