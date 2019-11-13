using Milvaneth.Common;
using Milvaneth.Communication.Procedure;
using Milvaneth.Communication.Vendor;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Milvaneth.Subwindow
{
    public class SubwindowRouter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal IProcedure Procedure;
        internal string Email;
        internal LobbyServiceResult Service;
        internal LobbyCharacterResult Character;

        internal byte[] RenewToken
        {
            get => ApiVendor.GetRenew();
            set => ApiVendor.SetRenew(value);
        }

        internal string Username
        {
            get => ApiVendor.GetUsername();
            set => ApiVendor.SetUsername(value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        private string _dispname = " ";
        public string Dispname
        {
            get => _dispname;
            set
            {
                _dispname = value;
                OnPropertyChanged(nameof(Dispname));
            }
        }

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal void InteractiveTask(Action action)
        {
            Task.Run(() =>
            {
                IsBusy = true;

                action.Invoke();

                IsBusy = false;
            });
        }
    }
}
