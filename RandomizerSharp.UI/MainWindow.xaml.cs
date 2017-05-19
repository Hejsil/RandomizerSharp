using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using RandomizerSharp.Randomizers;
using RandomizerSharp.RomHandlers;
using static System.Windows.Forms.DialogResult;

namespace RandomizerSharp.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly OpenFileDialog _openFileDialog = new OpenFileDialog
        {
            Filter = @"NDS Files|*.nds"
        };

        private readonly SaveFileDialog _saveFileDialog = new SaveFileDialog
        {
            Filter = @"NDS Files|*.nds"
        };

        private string _romToRandomize;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PickButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_openFileDialog.ShowDialog() != OK || !Gen5RomHandler.IsLoadable(_openFileDialog.FileName))
            {
                System.Windows.MessageBox.Show("This rom can't be picked. It is probably not BW/BW2");
                return;
            }

            _romToRandomize = _openFileDialog.FileName;
            PickButton.Content = $"{Path.GetFileName(_openFileDialog.FileName)} is picked";
            PickButton.IsEnabled = false;

            SaveButton.Content = "Randomize Rom";
            SaveButton.IsEnabled = true;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_saveFileDialog.ShowDialog() != OK) return;

            try
            {
                var romHandler = new Gen5RomHandler(_romToRandomize);
                var randomizer = new Randomizer(romHandler, StreamWriter.Null);
                var gen5Randomizer = new Gen5Randomizer(romHandler, StreamWriter.Null);

                randomizer.RandomizeStarters(true);
                randomizer.RandomizeStaticPokemon(true);
                randomizer.RandomizeIngameTrades(true, true, true);
                randomizer.RandomizeTrainerPokes(true, false, true, true);
                randomizer.RandomEncounters(Randomizer.Encounters.CatchEmAll, false);
                randomizer.RandomizeTmMoves(true, false, true, 1.0);
                randomizer.RandomizeTmhmCompatibility(Randomizer.TmsHmsCompatibility.RandomPreferType);
                randomizer.RandomizeMoveTutorMoves(true, false, 1.0);
                randomizer.RandomizeMoveTutorCompatibility(true);
                randomizer.RandomizeFieldItems(true);

                gen5Randomizer.ApplyFastestText();
                gen5Randomizer.RandomizeHiddenHollowPokemon();

                romHandler.SaveRom(_saveFileDialog.FileName);

                System.Windows.MessageBox.Show("Rom randomized");
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show($"Failed to randomize {exception}");
            }

            PickButton.Content = "Pick Rom";
            PickButton.IsEnabled = true;

            SaveButton.Content = "No rom has been picked";
            SaveButton.IsEnabled = false;
        }
    }
}
