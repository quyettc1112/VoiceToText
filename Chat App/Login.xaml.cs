using Microsoft.VisualBasic.ApplicationServices;
using NAudio.CoreAudioApi;
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
using System.Windows.Shapes;
using VoiceToText_Repo.Models;
using VoiceToText_Repo.Repo;

namespace Chat_App
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly VoiceToTextContext _context = new VoiceToTextContext();
        public Login()
        {
            _unitOfWork = new UnitOfWork(_context);
            InitializeComponent();
        }
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(passwordBox.Password) && passwordBox.Password.Length > 0)
                textPassword.Visibility = Visibility.Collapsed;
            else
                textPassword.Visibility = Visibility.Visible;
        }

        private void textPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            passwordBox.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtEmail.Text) && !string.IsNullOrEmpty(passwordBox.Password))
            {
                VoiceToText_Repo.Models.User user = _unitOfWork.UserRepostiory.GetPagination().FirstOrDefault(o=> o.Username.Equals(txtEmail.Text) && o.Password.Equals(passwordBox.Password));
               
                if (user != null)
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.user = user;

                    mainWindow.PastList.ItemsSource = _unitOfWork.ConversationRepostiory.GetPagination(
                            filter: cons => 
                        (cons.UserId == mainWindow.user.UserId && DateTime.Compare((DateTime)cons.CreatedOn, DateTime.Now) < 0),
                        orderBy: null,
                        includeProperties: "Messages",
                        null,
                        null
                    );
                    mainWindow.TodayList.ItemsSource = _unitOfWork.ConversationRepostiory.GetPagination(
                            filter: cons =>
                        (cons.UserId == mainWindow.user.UserId && DateTime.Compare((DateTime)cons.CreatedOn, DateTime.Today) == 0),
                        orderBy: null,
                        includeProperties: "Messages",
                        null,
                        null
                    );

                    mainWindow.Show();
                }
                else
                {
                    MessageBox.Show("Login fail");
                }
               
            }
        }

        private void txtEmail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtEmail.Text) && txtEmail.Text.Length > 0)
                textEmail.Visibility = Visibility.Collapsed;
            else
                textEmail.Visibility = Visibility.Visible;
        }

        private void textEmail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtEmail.Focus();
        }
    }
}
