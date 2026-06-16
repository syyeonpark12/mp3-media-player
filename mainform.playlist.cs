#nullable disable
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace num1_Project
{
    public partial class MainForm
    {
        private async Task LoadPlaylistAsync()
        {
            try
            {
                _playlist = await DatabaseHelper.GetSongsByGenreAsync("전체");
                RenderPlaylist();
            }
            catch (Exception ex)
            {
                pnlPlaylist.Controls.Clear();

                Label label = new Label
                {
                    Text = "서버에서 음악 목록을 불러오지 못했습니다.\n" + ex.Message,
                    ForeColor = Color.FromArgb(220, 120, 120),
                    Font = new Font("맑은 고딕", 10f),
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                pnlPlaylist.Controls.Add(label);
            }
        }

        private void RenderPlaylist()
        {
            pnlPlaylist.SuspendLayout();
            pnlPlaylist.Controls.Clear();

            if (_playlist.Count == 0)
            {
                Label label = new Label
                {
                    Text = "서버에 등록된 음악이 없습니다.",
                    ForeColor = Color.FromArgb(139, 148, 158),
                    Font = new Font("맑은 고딕", 10f),
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                pnlPlaylist.Controls.Add(label);
                pnlPlaylist.ResumeLayout();
                return;
            }

            for (int i = 0; i < _playlist.Count; i++)
                pnlPlaylist.Controls.Add(CreateSongRow(_playlist[i], i));

            RelayoutPlaylistRows();
            HighlightCurrentRow();
            pnlPlaylist.ResumeLayout();
        }

        private Panel CreateSongRow(SongInfo song, int index)
        {
            Panel row = new Panel
            {
                Size = new Size(GetPlaylistRowWidth(), 60),
                Location = new Point(0, index * 62),
                BackColor = index % 2 == 0
                    ? Color.FromArgb(22, 27, 34)
                    : Color.FromArgb(17, 22, 30),
                Cursor = Cursors.Hand,
                Tag = index,
                Padding = new Padding(8, 6, 8, 6),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            PictureBox albumPicture = new PictureBox
            {
                Name = "picAlbum",
                Size = new Size(46, 46),
                Location = new Point(8, 7),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(46, 117, 182),
                Tag = index
            };
            SetAlbumArtImage(albumPicture, song.AlbumArtUrl);

            Label titleLabel = new Label
            {
                Name = "lblTitle",
                Text = song.Title,
                Font = new Font("맑은 고딕", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(230, 237, 243),
                Location = new Point(64, 8),
                Size = new Size(Math.Max(120, row.Width - 230), 22),
                AutoEllipsis = true,
                Tag = index
            };

            Label artistLabel = new Label
            {
                Name = "lblArtist",
                Text = song.Artist,
                Font = new Font("맑은 고딕", 8.5f),
                ForeColor = Color.FromArgb(139, 148, 158),
                Location = new Point(64, 32),
                Size = new Size(Math.Max(120, row.Width - 230), 18),
                AutoEllipsis = true,
                Tag = index
            };

            Label durationLabel = new Label
            {
                Name = "lblDur",
                Text = song.DurationText,
                Font = new Font("Consolas", 9f),
                ForeColor = Color.FromArgb(139, 148, 158),
                TextAlign = ContentAlignment.MiddleRight,
                Size = new Size(60, 46),
                Location = new Point(row.Width - 72, 7),
                Tag = index
            };

            row.Controls.AddRange(new Control[]
            {
                albumPicture,
                titleLabel,
                artistLabel,
                durationLabel
            });

            EventHandler doubleClick = (s, e) =>
            {
                int selectedIndex = (int)((Control)s).Tag;
                PlaySong(selectedIndex);
            };

            row.DoubleClick += doubleClick;
            albumPicture.DoubleClick += doubleClick;
            titleLabel.DoubleClick += doubleClick;
            artistLabel.DoubleClick += doubleClick;
            durationLabel.DoubleClick += doubleClick;

            EventHandler mouseEnter = (s, e) =>
                row.BackColor = Color.FromArgb(28, 46, 74);

            EventHandler mouseLeave = (s, e) =>
                row.BackColor = index == _currentIndex
                    ? Color.FromArgb(28, 46, 74)
                    : index % 2 == 0
                        ? Color.FromArgb(22, 27, 34)
                        : Color.FromArgb(17, 22, 30);

            row.MouseEnter += mouseEnter;
            albumPicture.MouseEnter += mouseEnter;
            titleLabel.MouseEnter += mouseEnter;
            artistLabel.MouseEnter += mouseEnter;
            durationLabel.MouseEnter += mouseEnter;

            row.MouseLeave += mouseLeave;
            albumPicture.MouseLeave += mouseLeave;
            titleLabel.MouseLeave += mouseLeave;
            artistLabel.MouseLeave += mouseLeave;
            durationLabel.MouseLeave += mouseLeave;

            return row;
        }

        private int GetPlaylistRowWidth()
        {
            int width =
                pnlPlaylist.ClientSize.Width -
                (pnlPlaylist.VerticalScroll.Visible
                    ? SystemInformation.VerticalScrollBarWidth
                    : 0) - 4;

            return Math.Max(260, width);
        }

        private void RelayoutPlaylistRows()
        {
            if (pnlPlaylist == null)
                return;

            int y = 0;
            int rowWidth = GetPlaylistRowWidth();

            foreach (Control control in pnlPlaylist.Controls)
            {
                if (control is not Panel row || row.Tag is not int)
                    continue;

                row.SuspendLayout();
                row.Location = new Point(0, y);
                row.Size = new Size(rowWidth, 60);

                foreach (Control child in row.Controls)
                {
                    if (child.Name == "picAlbum")
                    {
                        child.Location = new Point(8, 7);
                        child.Size = new Size(46, 46);
                    }
                    else if (child.Name == "lblTitle")
                    {
                        child.Location = new Point(64, 8);
                        child.Size = new Size(Math.Max(120, row.Width - 230), 22);
                    }
                    else if (child.Name == "lblArtist")
                    {
                        child.Location = new Point(64, 32);
                        child.Size = new Size(Math.Max(120, row.Width - 230), 18);
                    }
                    else if (child.Name == "btnDelete")
                    {
                        child.Location = new Point(row.Width - 132, 16);
                        child.Size = new Size(52, 28);
                    }
                    else if (child.Name == "lblDur")
                    {
                        child.Location = new Point(row.Width - 72, 7);
                        child.Size = new Size(60, 46);
                    }
                }

                row.ResumeLayout();
                y += 62;
            }

            pnlPlaylist.AutoScrollMinSize = new Size(0, y + 4);
        }

        private void pnlPlaylist_Resize(object sender, EventArgs e)
        {
            RelayoutPlaylistRows();
        }

        private void HighlightCurrentRow()
        {
            foreach (Control control in pnlPlaylist.Controls)
            {
                if (control is Panel row && row.Tag is int index)
                {
                    row.BackColor = index == _currentIndex
                        ? Color.FromArgb(28, 46, 74)
                        : index % 2 == 0
                            ? Color.FromArgb(22, 27, 34)
                            : Color.FromArgb(17, 22, 30);
                }
            }
        }

        private void SetAlbumArtImage(PictureBox pictureBox, string imageUrl)
        {
            pictureBox.Image = GetDefaultAlbumArt();
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            pictureBox.LoadCompleted += PictureBox_LoadCompleted;

            try
            {
                pictureBox.LoadAsync(imageUrl);
            }
            catch
            {
                pictureBox.Image = GetDefaultAlbumArt();
            }
        }

        private void PictureBox_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (sender is not PictureBox pictureBox)
                return;

            if (e.Error != null || pictureBox.Image == null)
                pictureBox.Image = GetDefaultAlbumArt();

            pictureBox.LoadCompleted -= PictureBox_LoadCompleted;
        }

        private Image GetDefaultAlbumArt()
        {
            Bitmap bitmap = new Bitmap(46, 46);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.FromArgb(46, 117, 182));

            using Font font = new Font("Segoe UI Emoji", 18f);
            graphics.DrawString(
                "🎵",
                font,
                Brushes.White,
                new RectangleF(0, 0, 46, 46),
                new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                });

            return bitmap;
        }
    }
}
