#nullable disable
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace num1_Project
{
    public partial class MainForm
    {
        private void RenderCapsules()
        {
            flowCards.Controls.Clear();

            foreach (CapsuleInfo capsule in capsuleList)
                flowCards.Controls.Add(CreateCapsuleCard(capsule));

            flowCards.Controls.Add(CreateAddCard());
        }

        private Panel CreateAddCard()
        {
            Panel card = new Panel
            {
                Size = new Size(240, 200),
                BackColor = Color.FromArgb(13, 23, 38),
                Margin = new Padding(10),
                Cursor = Cursors.Hand
            };

            Label plus = new Label
            {
                Text = "+",
                ForeColor = Color.FromArgb(150, 170, 200),
                Font = new Font("맑은 고딕", 28),
                AutoSize = false,
                Size = new Size(240, 60),
                Location = new Point(0, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            Label text = new Label
            {
                Text = "새 캡슐 만들기",
                ForeColor = Color.FromArgb(150, 170, 200),
                Font = new Font("맑은 고딕", 11),
                AutoSize = false,
                Size = new Size(240, 30),
                Location = new Point(0, 130),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            card.Controls.Add(plus);
            card.Controls.Add(text);

            card.Click += OpenCreateCapsuleForm;
            plus.Click += OpenCreateCapsuleForm;
            text.Click += OpenCreateCapsuleForm;

            return card;
        }

        private Panel CreateCapsuleCard(CapsuleInfo capsule)
        {
            Panel card = new Panel
            {
                Size = new Size(240, 200),
                BackColor = Color.FromArgb(13, 23, 38),
                Margin = new Padding(10)
            };

            Panel topPanel = new Panel
            {
                Size = new Size(240, 60),
                Location = new Point(0, 0),
                BackColor = capsule.IsOpenable
                    ? Color.FromArgb(24, 61, 52)
                    : Color.FromArgb(31, 53, 77)
            };

            Label icon = new Label
            {
                AutoSize = false,
                Size = new Size(240, 30),
                Location = new Point(0, 18),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 16, FontStyle.Bold),
                Text = capsule.IsOpenable ? "✉" : "🔒"
            };

            Label title = new Label
            {
                Text = capsule.Title,
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 12, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(240, 30),
                Location = new Point(0, 75),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label songCount = new Label
            {
                Text = $"{capsule.Songs.Count}곡",
                ForeColor = Color.FromArgb(160, 180, 200),
                Font = new Font("맑은 고딕", 9),
                AutoSize = false,
                Size = new Size(240, 20),
                Location = new Point(0, 105),
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.Add(topPanel);
            card.Controls.Add(icon);
            card.Controls.Add(title);
            card.Controls.Add(songCount);

            if (capsule.IsOpenable)
            {
                Button openButton = new Button
                {
                    Text = "캡슐 열기",
                    Size = new Size(170, 40),
                    Location = new Point(35, 145),
                    BackColor = Color.FromArgb(46, 160, 97),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("맑은 고딕", 10, FontStyle.Bold)
                };
                openButton.FlatAppearance.BorderSize = 0;

                openButton.Click += (s, e) =>
                {
                    string songs = string.Join(Environment.NewLine, capsule.Songs);
                    MessageBox.Show(
                        $"캡슐 이름: {capsule.Title}\n\n노래 목록:\n{songs}",
                        "캡슐 열기");
                };

                card.Controls.Add(openButton);
            }
            else
            {
                Panel bottomPanel = new Panel
                {
                    Size = new Size(170, 50),
                    Location = new Point(35, 140),
                    BackColor = Color.FromArgb(24, 41, 63)
                };

                Label bottomLabel = new Label
                {
                    Text = $"개봉까지\r\nD-{capsule.DDay}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.DeepSkyBlue,
                    Font = new Font("맑은 고딕", 10, FontStyle.Bold)
                };

                bottomPanel.Controls.Add(bottomLabel);
                card.Controls.Add(bottomPanel);
            }

            return card;
        }

        private async void OpenCreateCapsuleForm(object sender, EventArgs e)
        {
            using CreateCapsuleForm form = new CreateCapsuleForm();

            if (form.ShowDialog() != DialogResult.OK)
                return;

            capsuleList = await DatabaseHelper.GetMyCapsulesAsync();
            RenderCapsules();
        }

        private void btnCreateTop_Click(object sender, EventArgs e)
        {
            OpenCreateCapsuleForm(sender, e);
        }

        private async Task RefreshCapsulesAsync()
        {
            capsuleList = await DatabaseHelper.GetMyCapsulesAsync();
            RenderCapsules();
        }

        private async void LoadYear(int year)
        {
            _currentYear = year;
            lblYearNum.Text = year.ToString();
            lblFooter.Text = $"WorldBeat · {year}년 한국 가요 HOT 차트 기준";

            lstNews.Items.Clear();
            dgvChart?.Rows.Clear();

            var newsList = await DatabaseHelper.GetNewsByYearAsync(year);
            foreach (var news in newsList)
                lstNews.Items.Add(news);

            YearRecord record = null;
            int bestDistance = int.MaxValue;

            foreach (var pair in _db)
            {
                int distance = Math.Abs(pair.Key - year);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    record = pair.Value;
                }
            }

            if (record != null && dgvChart != null)
            {
                dgvChart.SuspendLayout();

                foreach (var song in record.Songs)
                    dgvChart.Rows.Add(
                        song.Rank.ToString("D2"),
                        song.Title,
                        song.Artist,
                        song.Genre,
                        song.Note);

                dgvChart.ResumeLayout();
            }

            tabMain.Invalidate(true);
        }

        private void pnlYearCtrl_Resize(object sender, EventArgs e)
        {
            int sliderWidth = pnlYearCtrl.Width - 310;
            trkYear.Width = sliderWidth;
            btnNext.Left = trkYear.Right + 6;
            lblMax.Left = trkYear.Right - 4;
        }

        private void trkYear_ValueChanged(object sender, EventArgs e)
        {
            LoadYear(trkYear.Value);
        }

        private void btnpreView_Click(object sender, EventArgs e)
        {
            if (trkYear.Value > trkYear.Minimum)
                trkYear.Value--;
        }

        private void btnNexts_Click(object sender, EventArgs e)
        {
            if (trkYear.Value < trkYear.Maximum)
                trkYear.Value++;
        }

        private void lstNews_MouseClick(object sender, MouseEventArgs e)
        {
            int index = lstNews.IndexFromPoint(e.Location);
            if (index < 0 || index >= lstNews.Items.Count)
                return;

            if (lstNews.Items[index] is NewsItem item)
                new NewsDetailForm(item).Show(this);
        }

        private void MakeControlRound(Control control)
        {
            using GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, control.Width, control.Height);
            control.Region = new Region(path);
        }

        private void ApplyButtonDesign()
        {
            Button[] buttons = { btnShuffle, btnPrev, btnPlay, btnNext, btnRepeat };

            foreach (Button button in buttons)
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
            }

            btnPlay.Size = new Size(45, 45);
            btnPlay.BackColor = Color.FromArgb(52, 152, 219);
            btnPlay.ForeColor = Color.White;
            btnPlay.Text = "▶";
            MakeControlRound(btnPlay);

            btnShuffle.Text = "⇄";
            btnPrev.Text = "⏮";
            btnNext.Text = "⏭";
            btnRepeat.Text = "↻";

            btnShuffle.ForeColor = btnRepeat.ForeColor = Color.Gray;
            btnPrev.ForeColor = btnNext.ForeColor = Color.White;
        }

        private void pnlHeader_Paint(object sender, PaintEventArgs e)
        {
            using Pen pen = new Pen(Color.FromArgb(33, 38, 45));
            Panel panel = (Panel)sender;
            e.Graphics.DrawLine(pen, 0, panel.Height - 1, panel.Width, panel.Height - 1);
        }

        private void pnlYearCtrl_Paint(object sender, PaintEventArgs e)
        {
            using Pen pen = new Pen(Color.FromArgb(33, 38, 45));
            Panel panel = (Panel)sender;
            e.Graphics.DrawLine(pen, 0, 0, panel.Width, 0);
            e.Graphics.DrawLine(pen, 0, panel.Height - 1, panel.Width, panel.Height - 1);
        }

        private void pnlFooter_Paint(object sender, PaintEventArgs e)
        {
            using Pen pen = new Pen(Color.FromArgb(33, 38, 45));
            Panel panel = (Panel)sender;
            e.Graphics.DrawLine(pen, 0, 0, panel.Width, 0);
        }

        private void tabMain_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tab = (TabControl)sender;
            TabPage page = tab.TabPages[e.Index];
            bool selected = e.Index == tab.SelectedIndex;

            using var background = new SolidBrush(
                selected ? Color.FromArgb(22, 27, 34) : Color.FromArgb(13, 17, 23));
            e.Graphics.FillRectangle(background, e.Bounds);

            if (selected)
            {
                using var indicator = new SolidBrush(Color.FromArgb(88, 166, 255));
                e.Graphics.FillRectangle(
                    indicator,
                    new Rectangle(e.Bounds.X, e.Bounds.Bottom - 2, e.Bounds.Width, 2));
            }

            using var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using var font = new Font(
                "맑은 고딕",
                9f,
                selected ? FontStyle.Bold : FontStyle.Regular);

            using var brush = new SolidBrush(
                selected
                    ? Color.FromArgb(88, 166, 255)
                    : Color.FromArgb(139, 148, 158));

            e.Graphics.DrawString(page.Text, font, brush, e.Bounds, format);
        }

        private void lstNews_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            bool selected = (e.State & DrawItemState.Selected) != 0;
            Color background = selected
                ? Color.FromArgb(28, 46, 74)
                : e.Index % 2 == 0
                    ? Color.FromArgb(17, 22, 30)
                    : Color.FromArgb(13, 17, 23);

            using var backgroundBrush = new SolidBrush(background);
            e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

            Rectangle badge = new Rectangle(e.Bounds.X + 14, e.Bounds.Y + 13, 22, 22);
            using var badgeBrush = new SolidBrush(Color.FromArgb(46, 117, 182));
            e.Graphics.FillEllipse(badgeBrush, badge);

            using var center = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using var numberFont = new Font("Consolas", 8f, FontStyle.Bold);
            e.Graphics.DrawString(
                (e.Index + 1).ToString(),
                numberFont,
                Brushes.White,
                badge,
                center);

            NewsItem item = lstNews.Items[e.Index] as NewsItem;
            string headline = item?.Headline ?? lstNews.Items[e.Index].ToString();

            using var left = new StringFormat { LineAlignment = StringAlignment.Center };
            using var headlineFont = new Font("맑은 고딕", 10.5f);
            using var headlineBrush = new SolidBrush(
                selected
                    ? Color.FromArgb(230, 237, 243)
                    : Color.FromArgb(200, 210, 220));

            e.Graphics.DrawString(
                headline,
                headlineFont,
                headlineBrush,
                new RectangleF(
                    e.Bounds.X + 46,
                    e.Bounds.Y,
                    e.Bounds.Width - 60,
                    e.Bounds.Height),
                left);

            using var arrowFont = new Font("맑은 고딕", 16f);
            using var arrowBrush = new SolidBrush(
                selected
                    ? Color.FromArgb(88, 166, 255)
                    : Color.FromArgb(50, 88, 166, 255));

            e.Graphics.DrawString(
                "›",
                arrowFont,
                arrowBrush,
                new RectangleF(e.Bounds.Right - 28, e.Bounds.Y, 22, e.Bounds.Height),
                left);

            using var divider = new Pen(Color.FromArgb(25, 255, 255, 255));
            e.Graphics.DrawLine(
                divider,
                e.Bounds.Left,
                e.Bounds.Bottom - 1,
                e.Bounds.Right,
                e.Bounds.Bottom - 1);
        }

        private void btnPlay1_Click(object sender, EventArgs e)
        {
            const string url = "https://www.youtube.com/";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("링크를 열 수 없습니다: " + ex.Message);
            }
        }

        private void btnPlay1_MouseDown(object sender, MouseEventArgs e)
        {
            btnPlay1.FlatAppearance.BorderSize = 2;
            btnPlay1.FlatAppearance.BorderColor = Color.HotPink;
        }

        private void btnPlay1_MouseUp(object sender, MouseEventArgs e)
        {
            btnPlay1.FlatAppearance.BorderSize = 0;
        }

        private void btnPlay1_Paint(object sender, PaintEventArgs e) { }
        private void pnlVisualizer_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox1_Paint(object sender, PaintEventArgs e) { }
    }
}
