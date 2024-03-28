
using Google.Cloud.Speech.V1;
using NAudio.Wave;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Chat_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BufferedWaveProvider bwp;
        WaveIn waveIn = new WaveIn();
        WaveOut waveOut = new WaveOut();
        WaveFileReader reader;
        WaveFileWriter writer;
        string output = "audio.raw";

        bool isRecord = false;
        public MainWindow()
        {
            InitializeComponent();
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
    }
}
