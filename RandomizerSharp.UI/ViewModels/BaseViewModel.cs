using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RandomizerSharp.Properties;

namespace RandomizerSharp.UI.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public RomHandlerModelView Parent { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected BaseViewModel(RomHandlerModelView parent) => Parent = parent;
    }
}
