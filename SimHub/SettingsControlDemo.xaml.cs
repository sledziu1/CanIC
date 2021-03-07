using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace CanIC
{
    /// <summary>
    /// Logique d'interaction pour SettingsControlDemo.xaml
    /// </summary>
    public partial class SettingsControlDemo : UserControl
    {
        private readonly PluginMain parent;

        public SettingsControlDemo(PluginMain p)
        {
            parent = p;
            InitializeComponent();

            checkboxGearPrefix.IsChecked = parent.settingHandler.GetDefaultPrefix();
            checkboxEnableInGame.IsChecked = parent.settingHandler.GetDefaultEnableInGame();

            CheckboxGearPrefix_Checked(null, null);
            CheckboxEnableInGame_Checked(null, null);

            datagr.ItemsSource = parent.serialHandler.customPar;
        }

        private void SHButtonPrimary_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                label1.Content = "test";
            }));
        }

        private void CheckboxGearPrefix_Checked(object sender, RoutedEventArgs e)
        {
            parent.serialHandler.SetGearPrefixD(checkboxGearPrefix.IsChecked.GetValueOrDefault());

            parent.settingHandler.SetDefaultPrefix(checkboxGearPrefix.IsChecked.GetValueOrDefault());
        }

        private void CheckboxEnableInGame_Checked(object sender, RoutedEventArgs e)
        {
            //parent.serialHandler.SetGearPrefixD(checkboxGearPrefix.IsChecked.GetValueOrDefault());

            parent.serialHandler.enableInGameOnly = checkboxEnableInGame.IsChecked.GetValueOrDefault();

            parent.settingHandler.SetEnableInGame(checkboxEnableInGame.IsChecked.GetValueOrDefault());
        }

        private void SHButtonPrimary_Click_1(object sender, RoutedEventArgs e)
        {
            parent.serialHandler.button.ShortPress();
        }

        private void SHButtonPrimary_Click_2(object sender, RoutedEventArgs e)
        {
            parent.serialHandler.button.LongPress();
        }

        private void SHButtonPrimary_Click_3(object sender, RoutedEventArgs e)
        {
            parent.serialHandler.Test();
        }

        private void CheckboxHandbrake_Checked(object sender, RoutedEventArgs e)
        {
            parent.serialHandler.handbrake.OverrideEnable();
        }

        private void CheckboxHandbrake_Unchecked(object sender, RoutedEventArgs e)
        {
            parent.serialHandler.handbrake.OverrideDisable();
        }

        private void Datagr_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //Debug.WriteLine("xx");
            ((CustomParameter)e.Row.Item).SaveSettings();
        }
    }
}
