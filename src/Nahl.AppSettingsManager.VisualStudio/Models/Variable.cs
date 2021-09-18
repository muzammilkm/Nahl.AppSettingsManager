using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Nahl.AppSettingsManager.VisualStudio.Models
{
    public class Variable : INotifyPropertyChanged
    {
        public Variable()
        {
            IsNew = true;
        }

        public Variable(string projectId, string projectName, string fileName, string name, string value)
        {
            _old_projectId = _projectId = projectId;
            _old_projectName = _projectName = projectName;
            _old_fileName = _fileName = fileName;
            _old_name = _name = name;
            _old_value = _value = value;
            IsNew = false;
        }

        private string _old_name;
        private string _old_value;
        private string _old_projectId;
        private string _old_projectName;
        private string _old_fileName;

        private string _name;
        private string _value;
        private string _projectId;
        private string _projectName;
        private string _fileName;
        private bool _isChanged;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
                CheckIsChanged();
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
                CheckIsChanged();
            }
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged();
                CheckIsChanged();
            }
        }

        public string ProjectId
        {
            get => _projectId;
            set
            {
                _projectId = value;
                OnPropertyChanged();
                CheckIsChanged();
            }
        }

        private void CheckIsChanged()
        {
            IsDirty = (_old_projectId != _projectId || _old_projectName != _projectName || 
                _old_fileName != _fileName ||
                _old_name != _name || _old_value != _value);
        }

        public bool IsDirty
        {
            get => _isChanged;
            set
            {
                _isChanged = value;
                OnPropertyChanged();
            }
        }
        public bool IsNew { get; set; }

        public bool IsDuplicate { get; set; }

        public string ProjectName { get => _projectName; set => _projectName = value; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
