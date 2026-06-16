#nullable disable
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace num1_Project
{
    public partial class MainForm : Form
    {
        private readonly float[] _fftBuffer = new float[512];
        private readonly Complex[] _complexBuffer = new Complex[512];
        private int _sampleCount;
        private WaveOutEvent _waveOut;
        private AudioFileReader _audioReader;

        private SongInfo _currentSong;
        private List<SongInfo> _playlist = new List<SongInfo>();
        private int _currentIndex = -1;
        private bool _isDragging;

        private readonly Dictionary<int, YearRecord> _db = KoreanMusicDb.Build();
        private int _currentYear = 2000;

        private System.Windows.Forms.Timer _progressTimer;
        private List<CapsuleInfo> capsuleList = new List<CapsuleInfo>();

        public MainForm()
        {
            InitializeComponent();
            DatabaseHelper.InitApi();

            ApplyButtonDesign();
            InitProgressTimer();
            WirePlayerEvents();

            DoubleBuffered = true;

            if (pnlPlaylist != null)
            {
                pnlPlaylist.AutoScroll = true;
                pnlPlaylist.Resize += pnlPlaylist_Resize;
            }

            if (dgvChart != null)
            {
                typeof(DataGridView).InvokeMember(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.SetProperty,
                    null, dgvChart, new object[] { true });

                dgvChart.AutoGenerateColumns = false;
            }

            if (trkYear != null)
            {
                trkYear.Minimum = 1992;
                trkYear.Maximum = 2026;
                trkYear.Value = 2000;
            }

            Load += MainForm_Load;
            Shown += MainForm_Shown;
            Activated += MainForm_Activated;

            LoadYear(2000);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await RefreshCapsulesAsync();
        }

        private async void MainForm_Activated(object sender, EventArgs e)
        {
            await LoadPlaylistAsync();
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            await LoadPlaylistAsync();
        }
    }
}
