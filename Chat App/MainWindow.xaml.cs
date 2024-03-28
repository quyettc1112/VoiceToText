
using Google.Cloud.Speech.V1;
using Microsoft.EntityFrameworkCore.Metadata;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using VoiceToText_Repo.Models;
using VoiceToText_Repo.Repo;
using static VoiceToText_Repo.Ulity.ChatGptAPI;


namespace Chat_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string apiUrl = "https://api.openai.com/v1/chat/completions";
        private string apiKey = "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";
        private readonly UnitOfWork _unitOfWork;
        private readonly VoiceToTextContext _context = new VoiceToTextContext();
        private int? conversationId = null;
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
        public int UserId { get; set; }

        private BufferedWaveProvider bwp;
        WaveIn waveIn = new WaveIn();
        WaveOut waveOut = new WaveOut();
        WaveFileReader reader;
        WaveFileWriter writer;
        string output = "audio.raw";
        public User user { get; set; }

        bool isRecord = false;
        public MainWindow()
        {
            _unitOfWork = new UnitOfWork(_context);
            InitializeComponent();
            DataContext = this;
            //var messages = _unitOfWork.MessageRepostiory.GetAll(); // Giả sử đây trả về List<Message>
            //Messages = new ObservableCollection<Message>();

        }
        //FETCH DATA
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
               pageSize: 200
           );

            this.Messages = new ObservableCollection<Message>(messages);

            MessageList.ItemsSource = messages;
            myScrollViewer.ScrollToEnd();
        }
        //


        // ================= SPEECH ========================================
        private void InitWaveInAndWaveOut()
        {
            waveOut = new WaveOut();
            waveIn = new WaveIn();

            waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(waveIn_DataAvailable);
            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
            bwp = new BufferedWaveProvider(waveIn.WaveFormat);
            bwp.DiscardOnBufferOverflow = true;
        }

        //CONVERT TO TEXT FUNCTION
        private void ConvertToText()
        {
            if (File.Exists("audio.raw"))
            {
                string projectFolderPath = Directory.GetCurrentDirectory();
                DirectoryInfo parentDirectory = Directory.GetParent(projectFolderPath);

                string apiKeyPath = Path.Combine(parentDirectory.Parent.Parent.Parent.ToString(), "drugbank-417216-10103503001e.json");

                var speechClientBuilder = new SpeechClientBuilder
                {
                    CredentialsPath = apiKeyPath
                };

                var speech = speechClientBuilder.Build();

                /* Tạo SpeechClient để gọi API Speech-to-Text
                var speechClient = SpeechClient.Create(channel);*/

                //var speech = _speechClient.Create();
                var response = speech.Recognize(new RecognitionConfig()
                {
                    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    SampleRateHertz = 16000,
                    LanguageCode = "vi",
                }, RecognitionAudio.FromFile("audio.raw"));


                txtInput.Text = "";

                foreach (var result in response.Results)
                {
                    foreach (var alternative in result.Alternatives)
                    {
                        txtInput.Text = txtInput.Text + " " + alternative.Transcript;
                    }
                }

                if (txtInput.Text.Length == 0)
                    txtInput.Text = "No Data ";
            }
            else
            {

                txtInput.Text = "Audio File Missing ";

            }

        }

        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            bwp.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }
        private void waveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            waveOut.Stop();
            reader.Close();
            reader = null;
        }
        private void startRecording()
        {
            InitWaveInAndWaveOut();

            isRecord = !isRecord;

            if (NAudio.Wave.WaveIn.DeviceCount < 1)
            {
                Console.WriteLine("No microphone!");
                return;
            }

            waveIn.StartRecording();

        }

        private void stopRecording()
        {
            waveIn.StopRecording();

            if (File.Exists("audio.raw"))
                File.Delete("audio.raw");

            writer = new WaveFileWriter(output, waveIn.WaveFormat);

            byte[] buffer = new byte[bwp.BufferLength];
            int offset = 0;
            int count = bwp.BufferLength;

            var read = bwp.Read(buffer, offset, count);
            if (count > 0)
            {
                writer.Write(buffer, offset, read);
            }

            waveIn.Dispose();
            waveIn = null;
            writer.Close();
            writer = null;

            //reader = new WaveFileReader("audio.raw"); // (new MemoryStream(bytes));

            /*waveOut.Init(reader);
            waveOut.PlaybackStopped += new EventHandler<StoppedEventArgs>(waveOut_PlaybackStopped);
            waveOut.Play();*/

        }

        private void GetConversationData()
        {
            PastList.ItemsSource = _unitOfWork.ConversationRepostiory.GetPagination(
                            filter: cons =>
                        (cons.UserId == user.UserId && DateTime.Compare((DateTime)cons.CreatedOn, DateTime.Now) < 0),
                        orderBy: null,
                        includeProperties: "Messages",
                        null,
                        null
                    ).OrderByDescending(i => i.ConversationId);
            TodayList.ItemsSource = _unitOfWork.ConversationRepostiory.GetPagination(
                    filter: cons =>
                (cons.UserId == user.UserId && DateTime.Compare((DateTime)cons.CreatedOn, DateTime.Today) == 0),
                orderBy: null,
                includeProperties: "Messages",
                null,
                null
            ).OrderByDescending(i => i.ConversationId);
        }

        private void NewChat_OnClick(object sender, RoutedEventArgs e)
        {
            Conversation dto = new Conversation();
            dto.NameConversation = "Conversation " + DateTime.Now.ToString();
            dto.CreatedOn = DateTime.Now;
            dto.UserId = user.UserId;
            dto.Status = 1;

            _unitOfWork.ConversationRepostiory.Add(dto);
            _unitOfWork.SaveChanges();

            //conversationId = _unitOfWork.ConversationRepostiory.GetAll().ToList().Max();

            GetConversationData();
        }

        private void Voice_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(isRecord ? "Is recording" : "not recording");

            //Voice is recording
            if ( !isRecord)
            {
                btnSend.IsEnabled = false;
                txtInput.Text = "...Recording...";
                startRecording();
            }

            else
            {
                btnSend.IsEnabled = true;
                txtInput.Text = String.Empty;
                isRecord = false;

                stopRecording();

                ConvertToText();
            }

        }

        // ================ SPEECH ==============================
        private void Item_Click_Past(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("ACCESS ITEM CLICK");

            if (PastList.SelectedItem is Conversation conversation)
            {
                conversationId = conversation.ConversationId;

                if (conversationId != null)
                    GetMessage((int)conversationId);
            }

        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("ACCESS ITEM CLICK");

            if (TodayList.SelectedItem is Conversation conversation)
            {
                conversationId = conversation.ConversationId;

                if (conversationId != null)
                    GetMessage((int)conversationId);
            }

        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
             if(e.ChangedButton == MouseButton.Left )
            {
                this.DragMove();
            }
        }

        bool IsMaximized = false;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2) 
            {
                if (IsMaximized )
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1250;
                    this.Height = 830;

                    IsMaximized = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    IsMaximized = true;
                }
            }
        }
        // ================ Auto Scrool ==============================
     

        private void ItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            myScrollViewer.ScrollToEnd();
        }


        // ================ SEND MESSAGE ==============================
        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string text = txtInput.Text.Trim();


            if (text != null)
            {
                Message newMes = new Message
                {
                    ConversationId = conversationId,
                    CreatedOn = DateTime.Now,
                    SenderBy = user.Username,
                    SenderType = 1,
                    Text = text
                };
                _unitOfWork.MessageRepostiory.Add(newMes);
                /*List<Message> messages = (List<Message>) MessageList.ItemsSource;
                messages.Add(newMes);

                MessageList.ItemsSource = messages;*/
                _unitOfWork.SaveChanges();
                txtInput.Clear();

                GetMessage((int)conversationId);
                string gptResponse = await GetResponeGPTAsync(text);
                // Giả lập Bot phản hồi ở đây
                Message botMes = new Message
                {
                    ConversationId = conversationId,
                    CreatedOn = DateTime.Now,
                    SenderBy = "Bot",
                    SenderType = 0,
                    Text = gptResponse 
                };

                /*messages = (List<Message>)MessageList.ItemsSource;
                messages.Add(botMes);

                MessageList.ItemsSource = messages;*/

                _unitOfWork.MessageRepostiory.Add(botMes);
                _unitOfWork.SaveChanges();
                Messages.Add(botMes);
                myScrollViewer.ScrollToEnd();

                GetMessage((int)conversationId);
            }
            else {
                MessageBox.Show("Null Input");
            }
        }

        // ================ CALL API GPT ==============================
        private async Task<string> GetResponeGPTAsync(string prompt) // Đã sửa đổi để sử dụng tham số prompt
        {
            if (!string.IsNullOrEmpty(prompt))
            {
                ChatGPTCall gptCall = new ChatGPTCall(apiUrl, apiKey);
                string response = await gptCall.GenerateResponse(prompt);
                return response;
            }
            return "Maybe some problem";
        }

        private void OptionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (optionsPopup.IsOpen)
            {
                optionsPopup.IsOpen = false;
            }
            else
            {
                optionsPopup.IsOpen = true;
            }
        }

        private void HistorychatBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
