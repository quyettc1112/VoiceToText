using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
using VoiceToText_Repo.Models;
using VoiceToText_Repo.Repo;

namespace VoiceToText
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly UnitOfWork _unitOfWork;
        private readonly VoiceToTextContext _context= new VoiceToTextContext();
        public MainWindow()
        {
            InitializeComponent();
            _unitOfWork = new UnitOfWork(_context);
            TestGetUser();
     
        }

        public void TestGetUser()
        {

            IEnumerable<User> userList = _unitOfWork.UserRepostiory.GetAll();
            UsersListBox.Items.Clear(); // Clear existing items
            UsersListBox.ItemsSource = userList;
        }



        public void GetConversation(int userID)
        {
            IEnumerable<Conversation> conversations =
                _unitOfWork.ConversationRepostiory.GetPagination(
                    filter: cons =>
                (cons.UserId == userID),
                orderBy: null,
                includeProperties: "Messages",
                pageIndex: 1,
                pageSize: 20
            );
            ConverstionListBox.ItemsSource = null;
            ConverstionListBox.ItemsSource = conversations;

        }


        private void UsersListBox_SelectionChanged(object User, SelectionChangedEventArgs e)
        {
            if (UsersListBox.SelectedItem is User selectedUser)
            {
                // MessageBox.Show("Selected User ID: " + selectedUser.UserId.ToString());
                GetConversation(selectedUser.UserId);
            }
        }

        public void GetMessage(int conversationsId)
        {
            IEnumerable<Message> messages = new List<Message>();
            messages = null;
             messages =
                _unitOfWork.MessageRepostiory.GetPagination(
                    filter: cons =>
                (cons.ConversationId == conversationsId),
                orderBy: null,
                includeProperties: "Conversation",
                pageIndex: 1,
                pageSize: 20
            );
            /*List<Message> newList = messages.ToList();
            foreach (var message in newList)
            {
                if (message.SenderType == 1)
                {
                    message.Text = "People:     " + message.Text;
                }
                else message.Text = "Bot:       " + message.Text;
            }*/

            MessagesListBox.ItemsSource = null;
            MessagesListBox.ItemsSource = messages;



        }

        private void ConverstionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConverstionListBox.SelectedItem is Conversation selectedCons)
            {
                // MessageBox.Show("Selected User ID: " + selectedUser.UserId.ToString());
                GetMessage(selectedCons.ConversationId);

            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int senderType)
            {
                return senderType == 1 ? "User" : "Bot";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}