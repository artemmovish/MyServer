using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFApplication.ViewModels;

namespace WPFApplication.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = this.DataContext as MainViewModel;
            vm.CloseAction = this.Close;
        }

        // В этом случае решил не использовать mvvm
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null )
            {
                if (textBox.Text == "Логин" || textBox.Text == "Пароль")
                {
                    textBox.Text = "";
                }
            }
        }
        private void loginBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (LoginBox.Text == "")
            {
                LoginBox.Text = "Логин";
            }           
        }
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Text == "")
            {
                PasswordBox.Text = "Пароль";
            }
        }
    }
}