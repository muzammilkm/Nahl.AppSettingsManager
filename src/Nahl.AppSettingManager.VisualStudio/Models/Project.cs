using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Nahl.AppSettingManager.VisualStudio.Models
{
    public class Project : INotifyPropertyChanged
    {
        public Project()
        {

        }

        public Project(string projectId, string projectName)
        {
            ProjectId = projectId;
            ProjectName = projectName;
        }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged();
            }
        }

        public string ProjectId { get; set; }

        public string ProjectName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
