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

        public PokemonViewModel(Pokemon pokemon) => _pokemon = pokemon;

        public int? Id => _pokemon?.Id;

        public List<Evolution> EvolutionsFrom => _pokemon?.EvolutionsFrom;

        public string Name
        {
            get => _pokemon?.Name;
            set
            {
                _pokemon.Name = value;
                OnPropertyChanged();
            }
        }

        public int? HP
        {
            get => _pokemon?.Hp;
            set
            {
                if (value == null)
                    return;

                _pokemon.Hp = (int)value;
                OnPropertyChanged();
            }
        }

        public int? Attack
        {
            get => _pokemon?.Attack;
            set
            {
                if (value == null)
                    return;

                _pokemon.Attack = (int)value;
                OnPropertyChanged();
            }
        }

        public int? Defense
        {
            get => _pokemon?.Defense;
            set
            {
                if (value == null)
                    return;

                _pokemon.Defense = (int)value;
                OnPropertyChanged();
            }
        }

        public int? Spatk
        {
            get => _pokemon?.Spatk;
            set
            {
                if (value == null)
                    return;

                _pokemon.Spatk = (int)value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Special));
            }
        }

        public int? Spdef
        {
            get => _pokemon?.Spdef;
            set
            {
                if (value == null)
                    return;

                _pokemon.Spdef = (int)value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Special));
            }
        }

        public int? Special => _pokemon?.Special;

        public int? Speed
        {
            get => _pokemon?.Speed;
            set
            {
                if (value == null)
                    return;

                _pokemon.Speed = (int)value;
                OnPropertyChanged();
            }
        }

        public int? Ability1
        {
            get => _pokemon?.Ability1;
            set
            {
                if (value == null)
                    return;

                _pokemon.Ability1 = (int)value;
                OnPropertyChanged();
            }
        }

        public int? Ability2
        {
            get => _pokemon?.Ability2;
            set
            {
                if (value == null)
                    return;

                _pokemon.Ability2 = (int)value;
                OnPropertyChanged();
            }
        }

        public int? Ability3
        {
            get => _pokemon?.Ability3;
            set
            {
                if (value == null)
                    return;

                _pokemon.Ability3 = (int)value;
                OnPropertyChanged();
            }
        }

        public int? CatchRate
        {
            get => _pokemon?.CatchRate;
            set
            {
                if (value == null)
                    return;

                _pokemon.CatchRate = (int)value;
                OnPropertyChanged();
            }
        }

        public int? CommonHeldItem
        {
            get => _pokemon?.CommonHeldItem;
            set
            {
                if (value == null)
                    return;

                _pokemon.CommonHeldItem = (int)value;
                OnPropertyChanged();
            }
        }

        public int? RareHeldItem
        {
            get => _pokemon?.RareHeldItem;
            set
            {
                if (value == null)
                    return;

                _pokemon.RareHeldItem = (int)value;
                OnPropertyChanged();
            }
        }

        public int? DarkGrassHeldItem
        {
            get => _pokemon?.DarkGrassHeldItem;
            set
            {
                if (value == null)
                    return;

                _pokemon.DarkGrassHeldItem = (int)value;
                OnPropertyChanged();
            }
        }

        public ExpCurve? GrowthExpCurve
        {
            get => _pokemon?.GrowthExpCurve;
            set
            {
                if (value == null)
                    return;

                _pokemon.GrowthExpCurve = (ExpCurve)value;
                OnPropertyChanged();
            }
        }

        public Typing PrimaryType
        {
            get => _pokemon?.PrimaryType;
            set
            {
                if (value == null)
                    return;

                _pokemon.PrimaryType = value;
                OnPropertyChanged();
            }
        }

        public Typing SecondaryType
        {
            get => _pokemon?.SecondaryType;
            set
            {
                if (value == null)
                    return;

                _pokemon.SecondaryType = value;
                OnPropertyChanged();
            }
        }

        public Bitmap Sprite
        {
            get => _pokemon?.Sprite;
            set
            {
                if (value == null)
                    return;

                _pokemon.Sprite = value;
                OnPropertyChanged();
            }
        }

        public bool[] TMHMCompatibility
        {
            get => _pokemon?.TMHMCompatibility;
            set
            {
                if (value == null)
                    return;

                _pokemon.TMHMCompatibility = value;
                OnPropertyChanged();
            }
        }

        public bool[] MoveTutorCompatibility
        {
            get => _pokemon?.MoveTutorCompatibility;
            set
            {
                if (value == null)
                    return;

                _pokemon.MoveTutorCompatibility = value;
                OnPropertyChanged();
            }
        }
    }
}
