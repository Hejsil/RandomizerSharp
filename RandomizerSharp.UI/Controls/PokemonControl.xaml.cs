using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.UI.Controls
{
    /// <summary>
    /// Interaction logic for PokemonControl.xaml
    /// </summary>
    public partial class PokemonControl : UserControl
    {
        public PokemonControl()
        {
            InitializeComponent();

            LevelingRateComboBox.ItemsSource = (ExpCurve[]) Enum.GetValues(typeof(ExpCurve));
        }
    }
}
