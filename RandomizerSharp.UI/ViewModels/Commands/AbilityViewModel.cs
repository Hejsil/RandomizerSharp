using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.UI.ViewModels.Commands
{
    public class AbilityViewModel : BaseViewModel
    {
        private readonly Ability _ability;

        public int Id => _ability.Id;

        public string Name
        {
            get => _ability.Name;
            set
            {
                _ability.Name = value;
                OnPropertyChanged();
            }
        }

        public AbilityViewModel(RomHandlerModelView parent, Ability ability)
            : base(parent) => _ability = ability ?? throw new ArgumentNullException();
    }
}
