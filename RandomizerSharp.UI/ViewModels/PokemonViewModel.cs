using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.UI.ViewModels
{
    public class PokemonViewModel : BaseViewModel
    {
        private readonly Pokemon _pokemon;

        public PokemonViewModel(RomHandlerModelView handlerModelView, Pokemon pokemon)
            : base(handlerModelView) => _pokemon = pokemon ?? throw new ArgumentNullException();

        public int? Id => _pokemon?.Id;

        public List<Evolution> EvolutionsFrom => _pokemon?.EvolutionsFrom;

        public string Name
        {
            get => _pokemon.Name;
            set
            {
                _pokemon.Name = value;
                OnPropertyChanged();
            }
        }

        public int HP
        {
            get => _pokemon.Hp;
            set
            {
                _pokemon.Hp = value;
                OnPropertyChanged();
            }
        }

        public int Attack
        {
            get => _pokemon.Attack;
            set
            {
                _pokemon.Attack = value;
                OnPropertyChanged();
            }
        }

        public int Defense
        {
            get => _pokemon.Defense;
            set
            {
                _pokemon.Defense = value;
                OnPropertyChanged();
            }
        }

        public int Spatk
        {
            get => _pokemon.Spatk;
            set
            {
                _pokemon.Spatk = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Special));
            }
        }

        public int Spdef
        {
            get => _pokemon.Spdef;
            set
            {
                _pokemon.Spdef = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Special));
            }
        }

        public int Special => _pokemon.Special;

        public int Speed
        {
            get => _pokemon.Speed;
            set
            {
                _pokemon.Speed = value;
                OnPropertyChanged();
            }
        }

        public Ability Ability1
        {
            get => _pokemon.Ability1;
            set
            {
                _pokemon.Ability1 = value;
                OnPropertyChanged();
            }
        }

        public Ability Ability2
        {
            get => _pokemon.Ability2;
            set
            {
                _pokemon.Ability2 = value;
                OnPropertyChanged();
            }
        }

        public Ability Ability3
        {
            get => _pokemon.Ability3;
            set
            {
                _pokemon.Ability3 = value;
                OnPropertyChanged();
            }
        }

        public int CatchRate
        {
            get => _pokemon.CatchRate;
            set
            {
                _pokemon.CatchRate = value;
                OnPropertyChanged();
            }
        }

        public Item CommonHeldItem
        {
            get => _pokemon.CommonHeldItem;
            set
            {
                _pokemon.CommonHeldItem = value;
                OnPropertyChanged();
            }
        }

        public Item RareHeldItem
        {
            get => _pokemon.RareHeldItem;
            set
            {
                _pokemon.RareHeldItem = value;
                OnPropertyChanged();
            }
        }

        public Item DarkGrassHeldItem
        {
            get => _pokemon.DarkGrassHeldItem;
            set
            {
                _pokemon.DarkGrassHeldItem = value;
                OnPropertyChanged();
            }
        }

        public ExpCurve GrowthExpCurve
        {
            get => _pokemon.GrowthExpCurve;
            set
            {
                _pokemon.GrowthExpCurve = value;
                OnPropertyChanged();
            }
        }

        public Typing PrimaryType
        {
            get => _pokemon.PrimaryType;
            set
            {
                _pokemon.PrimaryType = value;
                OnPropertyChanged();
            }
        }

        public Typing SecondaryType
        {
            get => _pokemon.SecondaryType;
            set
            {
                _pokemon.SecondaryType = value;
                OnPropertyChanged();
            }
        }

        public Bitmap Sprite
        {
            get => _pokemon.Sprite;
            set
            {
                _pokemon.Sprite = value;
                OnPropertyChanged();
            }
        }

        public MachineLearnt[] TMHMCompatibility => _pokemon.TMHMCompatibility;

        public bool[] MoveTutorCompatibility => _pokemon.MoveTutorCompatibility;
    }
}
