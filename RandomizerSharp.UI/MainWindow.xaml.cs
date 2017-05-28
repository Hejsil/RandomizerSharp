using System;
using System.Windows;
using System.Windows.Forms;
using RandomizerSharp.UI.ViewModels;
using static System.Windows.Forms.DialogResult;

namespace RandomizerSharp.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RomHandlerModelView _context;
        private readonly OpenFileDialog _openFileDialog = new OpenFileDialog();
        private readonly SaveFileDialog _saveFileDialog = new SaveFileDialog();

        public MainWindow()
        {
            InitializeComponent();

            _context = (RomHandlerModelView) Panel.DataContext;
            _context.OpenRom.CanExecuteChanged += (sender, args) => OpenRomItem.IsEnabled = _context.OpenRom.CanExecute(null);
            _context.SaveRom.CanExecuteChanged += (sender, args) => SaveRomItem.IsEnabled = _context.SaveRom.CanExecute(null);
        }

        private void OpenRomItem_OnClick(object sender, RoutedEventArgs e)
        {
            var result = _openFileDialog.ShowDialog();

            if (result == OK && _context.OpenRom.CanExecute(null))
                _context.OpenRom.Execute(_openFileDialog.FileName);
        }

        private void SaveRomItem_OnClick(object sender, RoutedEventArgs e)
        {
            var result = _saveFileDialog.ShowDialog();

            if (result == OK && _context.SaveRom.CanExecute(null))
                _context.SaveRom.Execute(_saveFileDialog.FileName);
        }
    }
}
