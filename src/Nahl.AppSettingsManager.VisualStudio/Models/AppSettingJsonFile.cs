using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Nahl.AppSettingsManager.VisualStudio.Models
{
    public class AppSettingJsonFile : INotifyPropertyChanged
    {
        public AppSettingJsonFile()
        {

        }

        public AppSettingJsonFile(string fileId, string fileName)
        {
            FileId = fileId;
            FileName = fileName;
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

        public string FileId { get; set; }

        public string FileName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
