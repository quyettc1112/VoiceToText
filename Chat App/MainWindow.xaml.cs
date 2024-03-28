
using Google.Cloud.Speech.V1;
using Microsoft.EntityFrameworkCore.Metadata;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.IO;
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
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
        public int UserId { get; set; }

        private BufferedWaveProvider bwp;
        WaveIn waveIn = new WaveIn();
        WaveOut waveOut = new WaveOut();
        WaveFileReader reader;
        WaveFileWriter writer;
        string output = "audio.raw";

        bool isRecord = false;
        public MainWindow()
        {
            _unitOfWork = new UnitOfWork(_context);
            InitializeComponent();
            DataContext = this;
    
            var messages = _unitOfWork.MessageRepostiory.GetAll(); // Giả sử đây trả về List<Message>
            Messages = new ObservableCollection<Message>(messages);
          
        }

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
                    LanguageCode = "en",
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
                    ConversationId = 1,
                    CreatedOn = DateTime.Now,
                    SenderBy = "User",
                    SenderType = 1,
                    Text = text
                };
                _unitOfWork.MessageRepostiory.Add(newMes);
                Messages.Add(newMes);

                string gptResponse = await GetResponeGPTAsync(text);
                // Giả lập Bot phản hồi ở đây
                Message botMes = new Message
                {
                    ConversationId = 1,
                    CreatedOn = DateTime.Now,
                    SenderBy = "Bot",
                    SenderType = 0,
                    Text = gptResponse 
                };
                _unitOfWork.MessageRepostiory.Add(botMes);
                _unitOfWork.SaveChanges();
                Messages.Add(botMes);
                myScrollViewer.ScrollToEnd();
                txtInput.Clear();
            }
            else {
                MessageBox.Show("Null Input");
            }
            txtInput.Clear();
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
    }
}
