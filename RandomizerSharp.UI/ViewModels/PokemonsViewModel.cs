using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.UI.ViewModels
{
    public class PokemonsViewModel : BaseViewModel
    {
        private PokemonViewModel _selectedPokemon;

        public ICollectionView Pokemons { get; }

        public PokemonViewModel SelectedPokemon
        {
            get => _selectedPokemon;
            set
            {
                _selectedPokemon = value;
                OnPropertyChanged();
            }
        }

        public PokemonsViewModel(IEnumerable<Pokemon> pokemons)
        {
            Pokemons = CollectionViewSource.GetDefaultView(pokemons.Select(p => new PokemonViewModel(p)));
        }
    }
}
