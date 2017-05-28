using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using RandomizerSharp.Properties;
using RandomizerSharp.RomHandlers;
using RandomizerSharp.UI.ViewModels.Commands;

namespace RandomizerSharp.UI.ViewModels
{
    public class RomHandlerModelView : BaseViewModel
    {
        private AbstractRomHandler _romHandler;
        private PokemonsViewModel _pokemonsViewModel;

        public AbstractRomHandler RomHandler
        {
            get => _romHandler;
            set
            {
                _romHandler = value;
                PokemonsViewModel = new PokemonsViewModel(this, RomHandler.Pokemons);
                OnPropertyChanged();
                SaveRom.OnCanExecuteChanged();
            }
        }

        public PokemonsViewModel PokemonsViewModel
        {
            get => _pokemonsViewModel;
            set
            {
                _pokemonsViewModel = value;
                OnPropertyChanged();
            }
        }

        public Command OpenRom { get; }
        public Command SaveRom { get; }

        public RomHandlerModelView()
            : base(null)
        {
            OpenRom = new Command(
                arg => true,
                arg =>
                {
                    if (arg is string path)
                        RomHandler = new Gen5RomHandler(path);

                    // TODO: We might want to give an error if this fails
                });

            SaveRom = new Command(
                arg => RomHandler != null,
                arg =>
                {
                    if (arg is string path)
                        RomHandler.SaveRom(path);

                    // TODO: We might want to give an error if this fails
                });
        }
    }
}
