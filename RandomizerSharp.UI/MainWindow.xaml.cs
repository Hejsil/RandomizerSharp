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

                var world = new WorldRandomizer(romHandler);
                world.RandomizeStarters(true);
                world.RandomizeStaticPokemon(true);
                world.RandomizeIngameTrades(true, true, true);
                world.RandomizeFieldItems(true);
                world.RandomizeHiddenHollowPokemon();

                var trainer = new TrainerRandomizer(romHandler);
                trainer.RandomizeTrainerPokes(true, false, true, true);

                var wild = new WildRandomizer(romHandler);
                wild.RandomEncounters(EncountersRandomization.CatchEmAll, false);

                var move = new MoveRandomizer(romHandler);
                move.RandomizeTmMoves(true, false, true, 1.0);
                move.RandomizeTmhmCompatibility(TmsHmsCompatibility.RandomPreferType);
                move.RandomizeMoveTutorMoves(true, false, 1.0);
                move.RandomizeMoveTutorCompatibility(true);

                var util = new UtilityTweacker(romHandler);
                util.ApplyFastestText();

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
