using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using Microsoft.Win32;
using Prism.Mvvm;
using VSProjectCppManager.Models;

namespace VSProjectCppManager.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

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
                    //UpdateFilterList();
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

        #region SelectedProjectFile
        string _SelectedProjectFile;
        public string SelectedProjectFile
        {
            get
            {
                return _SelectedProjectFile;
            }
            set
            {
                if (_SelectedProjectFile != value)
                {
                    _SelectedProjectFile = value;
                    RaisePropertyChanged("SelectedProjectFile");
                }
            }
        }
        #endregion

        #region ProjectFilesList
        ObservableCollection<string> _ProjectFilesList;
        public ObservableCollection<string> ProjectFilesList
        {
            get
            {
                if (_ProjectFilesList == null)
                    _ProjectFilesList = new ObservableCollection<string>();
                return _ProjectFilesList;
            }
            private set
            {
                if (_ProjectFilesList != value)
                    _ProjectFilesList = value;
            }
        }
        #endregion

        public ItemProvider Files { get; private set; } = new ItemProvider();
        public ProjectFileProvider Project { get; private set; } = new ProjectFileProvider();
        public FilterFileProvider Filters { get; private set; } = new FilterFileProvider();

        #endregion

        #region Команды
        public ICommand C_SetPathToProject { get; set; }
        public ICommand C_AddSelected { get; set; }
        public ICommand C_ClearAll { get; set; }
        public ICommand C_UpdateExtensionFilter { get; set; }
        public ICommand C_SaveFilters { get; set; }
        #endregion

        #region Main
        public MainWindowViewModel()
        {
            FilesFilter = ".c .h .cpp makefile .mk .ld .s .asm .xaml .txt .a .py";

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
                    UpdateFilterList();
                    UpdateExtensionFilter();
                    //UpdateFilterList(); // автоматом при смене выбранного файла фильтров
                }
            });
            #endregion

            #region C_AddSelected
            C_AddSelected = new RelayCommand((p) =>
            {
                UpdateWithSelectedItems();
            }, (p) => !String.IsNullOrEmpty(PathToProject) & !String.IsNullOrEmpty(FilesFilter));
            #endregion

            #region C_UpdateExtensionFilter
            C_UpdateExtensionFilter = new RelayCommand((p) =>
            {
                UpdateExtensionFilter();
            }, (p) => !String.IsNullOrEmpty(PathToProject) & !String.IsNullOrEmpty(FilesFilter));
            #endregion

            #region C_SaveFilters
            C_SaveFilters = new RelayCommand((p) =>
            {
                SaveAll();
            }, (p) => !String.IsNullOrEmpty(PathToProject) & !String.IsNullOrEmpty(FilesFilter));
            #endregion

            #endregion
        }
        #endregion

        #region Приватные функции
        void UpdateProjectList()
        {        
            string[] plist = Directory.GetFiles(PathToProject, "*.vcxproj", SearchOption.TopDirectoryOnly);

            ProjectFilesList.Clear();
            if (plist != null)
            {
                foreach (var item in plist)
                {
                    ProjectFilesList.Add(item);
                }
                SelectedProjectFile = ProjectFilesList[ProjectFilesList.Count - 1];
            }

            Project.LoadFrom(SelectedProjectFile);
            Project.UpdateItems();
        }

        void UpdateFilterList()
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

            Filters.LoadFrom(SelectedFilterFile);
            Filters.UpdateItems();
        }

        void UpdateExtensionFilter()
        {
            Files.CutPathToProject = PathToProject;
            Files.SetExtensions(FilesFilter);
            Files.GetItems(PathToProject);
        }

        void UpdateWithSelectedItems()
        {
            Filters.GenerateFrom(Files.Items);
            Filters.UpdateItems();
            Project.UpdateWith(Files.Items);
            Project.UpdateItems();
        }

        void SaveAll()
        {
            Filters.SaveTo(SelectedFilterFile);
            Project.SaveTo(SelectedProjectFile);
        }
        #endregion
    }
}
