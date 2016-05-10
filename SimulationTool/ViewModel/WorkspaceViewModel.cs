using System;
using System.Windows.Input;
using SimulationTool.Commands;

namespace SimulationTool.ViewModel
{
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        private string header;
        public string Header
        {
            get { return header; }
            set
            {
                header = value;
                OnPropertyChanged("Header");
            }
        }

        private DelegateCommand closeCommand;
        public ICommand CloseCommand => closeCommand ?? (closeCommand = new DelegateCommand(OnRequestClose));

        public event EventHandler RequestClose;
        private void OnRequestClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
