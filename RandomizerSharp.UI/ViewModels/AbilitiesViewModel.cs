using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.Properties;
using RandomizerSharp.UI.ViewModels.Commands;

namespace RandomizerSharp.UI.ViewModels
{
    public class AbilitiesViewModel : BaseViewModel
    {
        private AbilityViewModel _selectedAbility;

        public ICollectionView Abilities { get; }

        public AbilityViewModel SelectedAbility
        {
            get => _selectedAbility;
            set
            {
                _selectedAbility = value;
                OnPropertyChanged();
            }
        }

        public AbilitiesViewModel(RomHandlerModelView parent, IEnumerable<Ability> abilities)
            : base(parent)
        {
            Abilities = CollectionViewSource.GetDefaultView(abilities.Select(p => new AbilityViewModel(Parent, p)));
        }
    }
}
