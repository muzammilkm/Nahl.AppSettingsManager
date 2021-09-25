using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using Nahl.AppSettingsManager.VisualStudio.Models;
using EnvDTE80;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Nahl.AppSettingsManager.VisualStudio
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ICollectionView _variablesView;
        private string _searchKeyword;
        private string _selectedProjectNamePreview;
        private string _selectedAppSettingFileNamePreview;
        private Project _selectedProject;
        private AppSettingJsonFile _selectedAppSettingJsonFile;
        private bool _isDirty;
        private ObservableCollection<Project> _projects;
        private ObservableCollection<AppSettingJsonFile> _appSettingJsonFiles;
        private ObservableCollection<Variable> _variables;
        private int _totalVariableCount;
        private bool _isSearchKeyword;

        public MainViewModel()
        {
            _variables = new ObservableCollection<Variable>();
            _projects = new ObservableCollection<Project>();
            _appSettingJsonFiles = new ObservableCollection<AppSettingJsonFile>();
            _variablesView = CollectionViewSource.GetDefaultView(_variables);
            _variablesView.Filter = VariableFilter;
            _variablesView.CollectionChanged += _variablesView_CollectionChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                IsSearchKeyword = !string.IsNullOrEmpty(_searchKeyword);
                OnPropertyChanged();
            }
        }

        public bool IsSearchKeyword
        {
            get => _isSearchKeyword;
            set
            {
                _isSearchKeyword = value;
                OnPropertyChanged();
            }
        }

        public static Dictionary<string, object> DotNotationToDictionary(Dictionary<string, string> dotNotation)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var dotObject in dotNotation)
            {
                var prop = "";
                var isStringObject = false;
                var leafDictionary = dictionary;

                for (var i = 0; i < dotObject.Key.Length; i++)
                {
                    if (dotObject.Key[i] == '.' && !isStringObject)
                    {
                        if (!leafDictionary.ContainsKey(prop))
                        {
                            leafDictionary.Add(prop, new Dictionary<string, object>());
                        }
                        leafDictionary = (Dictionary<string, object>)leafDictionary[prop];
                        prop = "";
                        continue;
                    }
                    else if (dotObject.Key[i] == '[')
                    {
                        if (!string.IsNullOrWhiteSpace(prop))
                        {
                            if (!leafDictionary.ContainsKey(prop))
                            {
                                leafDictionary.Add(prop, new Dictionary<string, object>());
                            }
                            leafDictionary = (Dictionary<string, object>)leafDictionary[prop];
                        }
                        prop = "";
                        isStringObject = true;
                        continue;
                    }
                    prop += dotObject.Key[i];
                }
                prop = prop.Replace("'", "").Replace("]", "");

                leafDictionary.Add(prop, dotObject.Value);
            }

            return dictionary;
        }

        public void SaveVariables()
        {
            var modifiedVariables = Variables
                .Where(x => x.IsDirty)
                .GroupBy(x => new { x.ProjectId, x.FileName });
            try
            {
                foreach (var modifiedVariable in modifiedVariables)
                {
                    var variables = Variables
                            .Where(x => x.ProjectId == modifiedVariable.Key.ProjectId && x.FileName == modifiedVariable.Key.FileName)
                            .ToDictionary(x => x.Name, x => x.Value);

                    var betterDictionary = DotNotationToDictionary(variables);
                    var json = JsonConvert.SerializeObject(betterDictionary, Formatting.Indented);
                    var jsonPath = $@"{Path.GetDirectoryName(modifiedVariable.Key.ProjectId)}\{modifiedVariable.Key.FileName}";

                    File.WriteAllText(jsonPath, json);
                }
                IsDirty = false;
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public ObservableCollection<Project> Projects { get => _projects; set { _projects = value; OnPropertyChanged(); } }

        public ObservableCollection<AppSettingJsonFile> AppSettingJsonFiles { get => _appSettingJsonFiles; set { _appSettingJsonFiles = value; OnPropertyChanged(); } }

        public ObservableCollection<Variable> Variables { get => _variables; set { _variables = value; OnPropertyChanged(); } }

        public ICollectionView VariablesView { get { return _variablesView; } }

        public string SelectedProjectNamePreview
        {
            get => _selectedProjectNamePreview;
            private set
            {
                _selectedProjectNamePreview = value;
                OnPropertyChanged();
            }
        }

        public string SelectedAppSettingFileNamePreview
        {
            get => _selectedAppSettingFileNamePreview;
            private set
            {
                _selectedAppSettingFileNamePreview = value;
                OnPropertyChanged();
            }
        }

        public bool IsDirty
        {
            get => _isDirty; set
            {
                _isDirty = value;
                OnPropertyChanged();
            }
        }

        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (_selectedProject != value)
                {
                    _selectedProject = value;
                    if (_selectedProject != null)
                        _selectedProject.IsChecked = !_selectedProject.IsChecked;
                }
            }
        }

        public AppSettingJsonFile SelectedAppSettingJsonFile
        {
            get => _selectedAppSettingJsonFile;
            set
            {
                if (_selectedAppSettingJsonFile != value)
                {
                    _selectedAppSettingJsonFile = value;
                    if (_selectedAppSettingJsonFile != null)
                        _selectedAppSettingJsonFile.IsChecked = !_selectedAppSettingJsonFile.IsChecked;
                }
            }
        }

        public IEnumerable<AppSettingJsonFile> SelectedAppSettingJsonFiles { get; private set; }

        public IEnumerable<Project> SelectedProjects { get; private set; }

        private static IEnumerable<EnvDTE.Project> GetSolutionProjects(IEnumerable<EnvDTE.Project> projects)
        {
            foreach (var project in projects)
            {
                if (project.Kind != ProjectKinds.vsProjectKindSolutionFolder)
                    yield return project;

                var solutionFolders = project.ProjectItems
                    .OfType<EnvDTE.ProjectItem>()
                    .Where(item => item.SubProject != null)
                    .Select(item => item.SubProject);

                var projectsInFolder = GetSolutionProjects(solutionFolders);

                if (projectsInFolder != null)
                {
                    foreach (var p in projectsInFolder)
                        yield return p;
                }

            }
        }
        public void RefreshData(DTE2 dTE)
        {
            try
            {
                _variables.Clear();
                _projects.Clear();
                _appSettingJsonFiles.Clear();

                var projects = GetSolutionProjects(dTE.Solution.OfType<EnvDTE.Project>());

                foreach (var p in projects)
                {
                    if (string.IsNullOrEmpty(p?.FullName))
                        continue;

                    var jsonFiles = new List<string>();

                    try
                    {
                        var projDir = Path.GetDirectoryName(p.FullName);
                        jsonFiles = Directory.GetFiles(projDir, "appsettings*.json").ToList();

                        if (!jsonFiles.Any())
                            continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                    foreach (var file in jsonFiles)
                    {
                        var fileContent = File.ReadAllText(file);
                        var appSettingObject = JObject.Parse(fileContent);
                        var jsonContent = appSettingObject
                            .SelectTokens("$..*")
                            .Where(t => !t.HasValues)
                            .ToDictionary(t => t.Path, t => t.ToString());
                        foreach (var json in jsonContent)
                        {
                            AddVariable(p.FileName, p.Name, Path.GetFileName(file), json.Key, json.Value);
                        }
                    }
                }

                _totalVariableCount = Variables.Count;
                var projectGroups = Variables.GroupBy(x => new { x.ProjectId, x.ProjectName });
                foreach (var group in projectGroups)
                {
                    var project = new Project(group.Key.ProjectId, group.Key.ProjectName);
                    project.PropertyChanged += Project_PropertyChanged;
                    _projects.Add(project);
                }

                var appSettingJsonFileGroups = Variables.GroupBy(x => new { x.FileName });
                foreach (var group in appSettingJsonFileGroups)
                {
                    var appSettingJsonFile = new AppSettingJsonFile(group.Key.FileName);
                    appSettingJsonFile.PropertyChanged += AppSettingJsonFile_PropertyChanged;
                    _appSettingJsonFiles.Add(appSettingJsonFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void _variablesView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var variable = item as Variable;
                    if (variable != null)
                    {
                        variable.PropertyChanged += Variable_PropertyChanged;

                        if (variable.IsNew)
                        {
                            var project = Projects.FirstOrDefault();
                            variable.ProjectId = project.ProjectId;
                            variable.ProjectName = project.ProjectName;

                            var appSettingJsonFile = AppSettingJsonFiles.FirstOrDefault();
                            variable.FileName = appSettingJsonFile.FileName;
                        }
                    }
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var variable = item as Variable;
                    if (variable != null)
                    {
                        variable.PropertyChanged -= Variable_PropertyChanged;
                        IsDirty = _totalVariableCount != Variables.Count || Variables.Any(x => x.IsDirty);
                        // new row delete = false
                        // new row add = true
                        // new row data change = true
                        // existing row delete = true
                        // existing data change = true
                    }
                }
            }
        }

        private void AddVariable(string projectId, string projectName, string fileName, string name, string value)
        {
            var variable = new Variable(projectId, projectName, fileName, name, value);
            //variable.PropertyChanged += Variable_PropertyChanged;
            _variables.Add(variable);
        }

        private void Variable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var variable = sender as Variable;
            if (e.PropertyName == "IsDirty")
            {
                IsDirty = _totalVariableCount != Variables.Count || Variables.Any(x => x.IsDirty);
            }

            if (e.PropertyName == "ProjectId")
            {
                var project = Projects.FirstOrDefault(x => x.ProjectId == variable.ProjectId);
                variable.ProjectName = project.ProjectName;
            }
        }

        public void Reset()
        {
            SearchKeyword = "";
            SelectedProjectNamePreview = "";

            foreach (var project in Projects)
                project.IsChecked = false;

            foreach (var appSettingJsonFile in AppSettingJsonFiles)
                appSettingJsonFile.IsChecked = false;
        }

        private void AppSettingJsonFile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                UpdateAppSettingJsonFileNamePreview();
            }
        }

        private void Project_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                UpdateProjectNamePreview();
            }
        }

        private bool VariableFilter(object obj)
        {
            var variable = obj as Variable;
            if (SelectedProjects != null && !SelectedProjects.Any(x => x.ProjectId == variable.ProjectId))
                return false;

            if (SelectedAppSettingJsonFiles != null && !SelectedAppSettingJsonFiles.Any(x => x.FileName == variable.FileName))
                return false;

            return string.IsNullOrWhiteSpace(_searchKeyword) ||
                variable.Name.ToLower().Contains(_searchKeyword.ToLower()) || variable.Value.ToLower().Contains(_searchKeyword.ToLower());
        }

        private void UpdateProjectNamePreview()
        {
            SelectedProjects = Projects.Where(x => x.IsChecked);
            var count = SelectedProjects.Count();
            if (count >= 1)
            {
                var firstProject = SelectedProjects.FirstOrDefault();
                _selectedProjectNamePreview = firstProject?.ProjectName;
                if (count >= 2)
                    _selectedProjectNamePreview = $"{_selectedProjectNamePreview} ({count}+)";
            }
            else
            {
                _selectedProjectNamePreview = "";
                SelectedProjects = null;
            }
            OnPropertyChanged("SelectedProjectNamePreview");
        }

        private void UpdateAppSettingJsonFileNamePreview()
        {
            SelectedAppSettingJsonFiles = AppSettingJsonFiles.Where(x => x.IsChecked);
            var count = SelectedAppSettingJsonFiles.Count();
            if (count >= 1)
            {
                var firstProject = SelectedAppSettingJsonFiles.FirstOrDefault();
                _selectedAppSettingFileNamePreview = firstProject?.FileName;
                if (count >= 2)
                    _selectedAppSettingFileNamePreview = $"{_selectedAppSettingFileNamePreview} ({count}+)";
            }
            else
            {
                _selectedAppSettingFileNamePreview = "";
                SelectedAppSettingJsonFiles = null;
            }
            OnPropertyChanged("SelectedAppSettingFileNamePreview");
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "SearchKeyword" ||
                propertyName == "SelectedProjectNamePreview" ||
                propertyName == "SelectedAppSettingFileNamePreview")
                _variablesView.Refresh();
        }
    }
}
