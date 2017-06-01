using System;
using System.ComponentModel;
using System.Windows.Data;
using RandomizerSharp.RomHandlers;
using RandomizerSharp.UI.ViewModels.Commands;

namespace RandomizerSharp.UI.ViewModels
{
    public class RomHandlerModelView : BaseViewModel
    {
        private AbstractRomHandler _romHandler;
        private PokemonsViewModel _pokemonsViewModel;
        private AbilitiesViewModel _abilities;

        public AbstractRomHandler RomHandler
        {
            get => _romHandler;
            set
            {
                _romHandler = value;
                Pokemons = new PokemonsViewModel(this, RomHandler.Pokemons);
                Abilities = new AbilitiesViewModel(this, RomHandler.Abilities);
                OnPropertyChanged();
                SaveRom.OnCanExecuteChanged();
            }
        }

        public PokemonsViewModel Pokemons
        {
            get => _pokemonsViewModel;
            set
            {
                _pokemonsViewModel = value;
                OnPropertyChanged();
            }
        }

        public AbilitiesViewModel Abilities
        {
            get => _abilities;
            set
            {
                _abilities = value;
                OnPropertyChanged();
            }
        }

        public Command OpenRom { get; }
        public Command SaveRom { get; }

        public event EventHandler RomOpened;
        public event EventHandler RomSaved;

        public RomHandlerModelView()
            : base(null)
        {
            OpenRom = new Command(
                arg => true,
                arg =>
                {
                    if (arg is string path)
                    {
                        RomHandler = new Gen5RomHandler(path);
                        RomOpened?.Invoke(this, null);
                    }

                    // TODO: We might want to give an error if this fails
                });

            SaveRom = new Command(
                arg => RomHandler != null,
                arg =>
                {
                    if (arg is string path)
                    {
                        RomHandler.SaveRom(path);
                        RomSaved?.Invoke(this, null);
                    }

                    // TODO: We might want to give an error if this fails
                });
        }
    }
}
