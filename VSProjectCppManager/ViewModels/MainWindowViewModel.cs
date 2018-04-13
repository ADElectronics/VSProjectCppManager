using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using Prism.Mvvm;
using VSProjectCppManager.Models;

namespace VSProjectCppManager.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        List<Item> NewItems;

        #region Публичные свойства

        #region PathToProject
        string _PathToProject;
        public string PathToProject
        {
            get
            {
                return _PathToProject;
            }
            set
            {
                if (_PathToProject != value)
                {
                    _PathToProject = value;
                    RaisePropertyChanged("PathToProject");
                }
            }
        }
        #endregion

        #region FilesFilter
        string _FilesFilter;
        public string FilesFilter
        {
            get
            {
                return _FilesFilter;
            }
            set
            {
                if (_FilesFilter != value)
                {
                    _FilesFilter = value;
                    RaisePropertyChanged("FilesFilter");
                }
            }
        }
        #endregion

        #region SelectedFilterFile
        string _SelectedFilterFile;
        public string SelectedFilterFile
        {
            get
            {
                return _SelectedFilterFile;
            }
            set
            {
                if (_SelectedFilterFile != value)
                {
                    _SelectedFilterFile = value;
                    UpdateFilterList();
                    RaisePropertyChanged("SelectedFilterFile");
                }
            }
        }
        #endregion

        #region FilterFilesList
        ObservableCollection<string> _FilterFilesList;
        public ObservableCollection<string> FilterFilesList
        {
            get
            {
                if (_FilterFilesList == null)
                    _FilterFilesList = new ObservableCollection<string>();
                return _FilterFilesList;
            }
            private set
            {
                if (_FilterFilesList != value)
                    _FilterFilesList = value;
            }
        }
        #endregion

        public ObservableCollection<Item> FilesList { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<XDocItem> XDocAttributesList { get; set; } = new ObservableCollection<XDocItem>();
        #endregion

        #region Команды
        public ICommand C_SetPathToProject { get; set; }
        public ICommand C_AddSelected { get; set; }
        public ICommand C_ClearAll { get; set; }
        public ICommand C_UpdateProject { get; set; }
        public ICommand C_SaveFilters { get; set; }
        #endregion

        #region Main
        public MainWindowViewModel()
        {
            FilesFilter = ".c .h .cpp makefile .mk";

            #region Команды

            #region C_SetPathToProject
            C_SetPathToProject = new RelayCommand((p) =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Файл проекта (*.vcxproj)|*.vcxproj| Все файлы (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    PathToProject = Path.GetDirectoryName(openFileDialog.FileName);

                    UpdateProjectList();
                    //UpdateFilterList(); // автоматом при смене выбранного файла фильтров
                }
            });
            #endregion

            #region C_AddSelected
            C_AddSelected = new RelayCommand((p) =>
            {
                AddFilterList();
            }, (p) => !String.IsNullOrEmpty(PathToProject) & !String.IsNullOrEmpty(FilesFilter));
            #endregion

            #region C_ClearAll
            C_ClearAll = new RelayCommand((p) =>
            {

            }, (p) => !String.IsNullOrEmpty(PathToProject) & !String.IsNullOrEmpty(FilesFilter));
            #endregion

            #region C_UpdateProject
            C_UpdateProject = new RelayCommand((p) =>
            {
                UpdateProjectList();
            }, (p) => !String.IsNullOrEmpty(PathToProject) & !String.IsNullOrEmpty(FilesFilter));
            #endregion

            #region C_SaveFilters
            C_SaveFilters = new RelayCommand((p) =>
            {
                SaveFilterList();
            }, (p) => !String.IsNullOrEmpty(PathToProject) & !String.IsNullOrEmpty(FilesFilter));
            #endregion

            #endregion
        }
        #endregion

        #region Приватные функции
        void UpdateProjectList()
        {
            string[] flist = Directory.GetFiles(PathToProject, "*.vcxproj.filters", SearchOption.TopDirectoryOnly);
            FilterFilesList.Clear();
            if (flist != null)
            {
                foreach (var item in flist)
                {
                    FilterFilesList.Add(item);
                }
                SelectedFilterFile = FilterFilesList[FilterFilesList.Count - 1];
            }

            FilesList.Clear();
            ItemProvider.CutPathToProject = PathToProject;
            ItemProvider.SetExtensions(FilesFilter);
            NewItems = ItemProvider.GetItems(PathToProject);
            NewItems.ForEach((item) =>
            {
                FilesList.Add(item);
            });
            //RaisePropertyChanged("Items");
        }

        void UpdateFilterList()
        {
            XDocAttributesList.Clear();
            List<XDocItem> NewItems2 = XDocProvider.GetItems(SelectedFilterFile);
            NewItems2.ForEach((item) =>
            {
                XDocAttributesList.Add(item);
            });
        }

        void AddFilterList()
        {

        }

        void SaveFilterList()
        {
            XDocProvider.GenerateItems(NewItems);
        }
        #endregion
    }
}
