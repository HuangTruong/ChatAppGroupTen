using ChatApp.Forms;
using System.Windows.Forms;
﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatApp.Controllers;
using ChatApp.Models.Users;
using Guna.UI2.WinForms;

namespace ChatApp
{
    /// <summary>
    /// Form Nhắn tin:
    /// - Bên trái: danh sách user (flpDanhSachChat).
    /// - Giữa: khung chat (flbKhungChat, txtNhapTinNhan, btnGui).
    /// - Bên phải: thông tin user đang chat.
    /// </summary>
    public partial class NhanTin : Form
    {
        #region ====== BIẾN THÀNH VIÊN ======

        /// <summary>
        /// localId của user hiện tại.
        /// </summary>
        private readonly string idDangNhap;

        /// <summary>
        /// Token đăng nhập (để dành nếu dùng).
        /// </summary>
        private readonly string tokenDangNhap;

        /// <summary>
        /// Controller xử lý logic nhắn tin.
        /// </summary>
        private readonly NhanTinController boDieuKhienNhanTin;

        /// <summary>
        /// Toàn bộ user lấy từ Firebase.
        /// </summary>
        private Dictionary<string, User> tatCaNguoiDung =
            new Dictionary<string, User>();

        /// <summary>
        /// localId user đang được chọn để chat.
        /// </summary>
        private string idNguoiDangChat;

        /// <summary>
        /// Danh sách tin nhắn đã vẽ lên UI.
        /// </summary>
        private readonly List<ChatMessage> danhSachTinNhanDangVe =
            new List<ChatMessage>();

        #endregion

        #region ====== HÀM KHỞI TẠO ======

        /// <summary>
        /// Khởi tạo form Nhắn tin với localId + token hiện tại.
        /// </summary>
        public NhanTin(string localId, string token)
        {
            InitializeComponent();

            idDangNhap = localId;
            tokenDangNhap = token;
            boDieuKhienNhanTin = new NhanTinController(localId, token);

            // Event form
            this.Load += NhanTin_Load;
            this.FormClosed += NhanTin_FormClosed;

            // Event control
            txtTimKiem.TextChanged += txtTimKiem_TextChanged;
            btnGui.Click += btnGui_Click;
            txtNhapTinNhan.KeyDown += TxtNhapTinNhan_KeyDown;

            // Layout danh sách chat (bên trái)
            flpDanhSachChat.WrapContents = false;

            // Layout khung chat (ở giữa)
            flbKhungChat.FlowDirection = FlowDirection.TopDown;
            flbKhungChat.WrapContents = false;
            flbKhungChat.AutoScroll = true;
        }

    
        #endregion

        #region ====== ENTER ĐỂ GỬI ======

        /// <summary>
        /// Nhấn Enter để gửi, Shift+Enter để xuống dòng.
        /// </summary>
        private void TxtNhapTinNhan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                btnGui.PerformClick();
            }
        }

        #endregion

        #region ====== FORM EVENTS ======

        private async void NhanTin_Load(object sender, EventArgs e)
        {
            await LoadUsersAsync();
        }

        private void NhanTin_FormClosed(object sender, FormClosedEventArgs e)
        {
            boDieuKhienNhanTin.Dispose();
        }

        #endregion

        #region ====== LOAD & LỌC DANH SÁCH USER ======

        /// <summary>
        /// Load toàn bộ user từ Firebase và đưa lên flpDanhSachChat.
        /// </summary>
        private async Task LoadUsersAsync()
        {
            flpDanhSachChat.Controls.Clear();

            try
            {
                tatCaNguoiDung = await boDieuKhienNhanTin.GetAllUsersAsync();

                foreach (KeyValuePair<string, User> cap in tatCaNguoiDung)
                {
                    string idNguoiDung = cap.Key;
                    User nguoiDung = cap.Value;

                    // Bỏ qua chính mình
                    if (string.Equals(idNguoiDung, idDangNhap, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    AddUserItem(idNguoiDung, nguoiDung);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi tải danh sách người dùng: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tạo 1 nút user trong flpDanhSachChat.
        /// </summary>
        private void AddUserItem(string userId, User user)
        {
            Guna2Button nutNguoiDung = new Guna2Button();
            nutNguoiDung.Width = flpDanhSachChat.ClientSize.Width - 10;
            nutNguoiDung.Height = 48;
            nutNguoiDung.Margin = new Padding(3);
            nutNguoiDung.TextAlign = HorizontalAlignment.Left;
            nutNguoiDung.Cursor = Cursors.Hand;
            nutNguoiDung.Tag = userId;
            nutNguoiDung.Text = GetUserFullName(user);

            nutNguoiDung.Click += UserItem_Click;

            flpDanhSachChat.Controls.Add(nutNguoiDung);
        }

        /// <summary>
        /// Lấy tên hiển thị: FullName, nếu trống thì "Unknown".
        /// </summary>
        private static string GetUserFullName(User user)
        {
            if (user == null)
            {
                return "Unknown";
            }

            if (!string.IsNullOrEmpty(user.FullName))
            {
                return user.FullName;
            }

            return "Unknown";
        }

        /// <summary>
        /// Lọc danh sách user khi gõ vào ô txtTimKiem.
        /// </summary>
        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            string tuKhoa = string.Empty;

            if (txtTimKiem.Text != null)
            {
                tuKhoa = txtTimKiem.Text.Trim().ToLowerInvariant();
            }

            flpDanhSachChat.SuspendLayout();
            flpDanhSachChat.Controls.Clear();

            foreach (KeyValuePair<string, User> cap in tatCaNguoiDung)
            {
                string idNguoiDung = cap.Key;
                User nguoiDung = cap.Value;

                if (string.Equals(idNguoiDung, idDangNhap, StringComparison.Ordinal))
                {
                    continue;
                }

                string ten = GetUserFullName(nguoiDung);

            }

            flpDanhSachChat.ResumeLayout();
        }

        #endregion

        #region ====== CHỌN USER & MỞ CUỘC TRÒ CHUYỆN ======

        private void UserItem_Click(object sender, EventArgs e)
        {
            Guna2Button nutNguoiDung = sender as Guna2Button;
            if (nutNguoiDung == null)
            {
                return;
            }

            string idNguoiDung = nutNguoiDung.Tag as string;
            if (string.IsNullOrEmpty(idNguoiDung))
            {
                return;
            }

            OpenConversation(idNguoiDung);
        }

        /// <summary>
        /// Mở cuộc trò chuyện với 1 user:
        /// - Cập nhật tên ở panel giữa/phải.
        /// - Xoá khung chat cũ.
        /// - Lắng nghe realtime cuộc trò chuyện đó.
        /// </summary>
        private void OpenConversation(string otherUserId)
        {
            idNguoiDangChat = otherUserId;

            // Cập nhật label tên người đang chat
            User nguoiDung;
            if (tatCaNguoiDung != null && tatCaNguoiDung.TryGetValue(otherUserId, out nguoiDung))
            {
                string ten = GetUserFullName(nguoiDung);
                lblTenDangNhapGiua.Text = ten;
                lblTenDangNhapPhai.Text = ten;
            }

            flbKhungChat.Controls.Clear();
            danhSachTinNhanDangVe.Clear();

            boDieuKhienNhanTin.StartListenConversation(
                otherUserId,
                delegate (List<ChatMessage> danhSachTinNhan)
                {
                    // Callback này chạy ở thread khác, phải Invoke về UI.
                    if (!IsHandleCreated)
                    {
                        return;
                    }

                    try
                    {
                        BeginInvoke(
                            new Action(
                                delegate
                                {
                                    RenderMessages(danhSachTinNhan, otherUserId);
                                }));
                    }
                    catch
                    {
                        
                    }
                });
        }

        #endregion

        #region ====== VẼ TIN NHẮN LÊN KHUNG CHAT ======

        /// <summary>
        /// Vẽ tin nhắn:
        /// - Nếu lần đầu: vẽ toàn bộ.
        /// - Nếu thêm mới: chỉ append phần mới.
        /// - Nếu số lượng giảm: vẽ lại toàn bộ.
        /// </summary>
        private void RenderMessages(IList<ChatMessage> messages, string ownerUserId)
        {
            // Nếu user đã đổi cuộc chat thì bỏ qua batch này.
            if (!string.Equals(idNguoiDangChat, ownerUserId, StringComparison.Ordinal))
            {
                return;
            }

            if (messages == null || messages.Count == 0)
            {
                flbKhungChat.SuspendLayout();
                flbKhungChat.Controls.Clear();
                flbKhungChat.ResumeLayout();
                danhSachTinNhanDangVe.Clear();
                return;
            }

            int soLuongCu = danhSachTinNhanDangVe.Count;
            int soLuongMoi = messages.Count;

            flbKhungChat.SuspendLayout();

            try
            {
                if (soLuongCu == 0 || soLuongMoi < soLuongCu)
                {
                    // Lần đầu hoặc có thay đổi bất thường -> vẽ lại toàn bộ
                    flbKhungChat.Controls.Clear();
                    danhSachTinNhanDangVe.Clear();

                    int i = 0;
                    while (i < soLuongMoi)
                    {
                        ChatMessage tinNhan = messages[i];
                        AddMessageBubble(tinNhan);
                        danhSachTinNhanDangVe.Add(tinNhan);
                        i++;
                    }
                }
                else
                {
                    // Bình thường thì chỉ vẽ thêm phần mới
                    int i = soLuongCu;
                    while (i < soLuongMoi)
                    {
                        ChatMessage tinNhan = messages[i];
                        AddMessageBubble(tinNhan);
                        danhSachTinNhanDangVe.Add(tinNhan);
                        i++;
                    }
                }

                if (flbKhungChat.Controls.Count > 0)
                {
                    Control controlCuoi =
                        flbKhungChat.Controls[flbKhungChat.Controls.Count - 1];
                    flbKhungChat.ScrollControlIntoView(controlCuoi);
                }
            }
            finally
            {
                flbKhungChat.ResumeLayout();
            }
        }

        /// <summary>
        /// Thêm 1 bong bóng tin nhắn (trái/phải) vào khung chat.
        /// </summary>
        private void AddMessageBubble(ChatMessage msg)
        {
            if (msg == null)
            {
                return;
            }

            bool laTinCuaToi = msg.IsMine;

            // Bong bóng: chứa TÊN NGƯỜI GỬI + TEXT + THỜI GIAN
            FlowLayoutPanel bongBong = new FlowLayoutPanel();
            bongBong.AutoSize = true;
            bongBong.WrapContents = false;
            bongBong.FlowDirection = FlowDirection.TopDown;
            bongBong.MaximumSize = new Size(flbKhungChat.ClientSize.Width - 120, 0);
            bongBong.Padding = new Padding(8);
            bongBong.Margin = new Padding(0, 3, 0, 10);

            if (laTinCuaToi)
            {
                bongBong.BackColor = Color.FromArgb(180, 216, 255); // xanh nhạt cho mình
            }
            else
            {
                bongBong.BackColor = Color.White; // trắng cho người kia
            }

            // ===== Label TÊN NGƯỜI GỬI =====
            Label lblNameUser = new Label();
            lblNameUser.AutoSize = true;
            lblNameUser.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblNameUser.ForeColor = Color.Black;

            string tenNguoiGui = "Unknown";
            User nguoiGui;

            if (laTinCuaToi)
            {
                // Tin của mình -> lấy tên mình từ tatCaNguoiDung (nếu có)
                if (tatCaNguoiDung != null &&
                    tatCaNguoiDung.TryGetValue(idDangNhap, out nguoiGui))
                {
                    tenNguoiGui = GetUserFullName(nguoiGui);
                }
                else
                {
                    tenNguoiGui = "Bạn";
                }
            }
            else
            {
                // Tin của người kia -> lấy theo SenderId
                if (tatCaNguoiDung != null &&
                    !string.IsNullOrEmpty(msg.SenderId) &&
                    tatCaNguoiDung.TryGetValue(msg.SenderId, out nguoiGui))
                {
                    tenNguoiGui = GetUserFullName(nguoiGui);
                }
            }

            lblNameUser.Text = tenNguoiGui;

            // ===== Label NỘI DUNG =====
            Label lblText = new Label();
            lblText.AutoSize = true;
            lblText.MaximumSize = new Size(flbKhungChat.ClientSize.Width - 140, 0);
            lblText.Font = new Font("Segoe UI", 10F);
            lblText.ForeColor = Color.Black;
            if (msg.Text != null)
            {
                lblText.Text = msg.Text;
            }
            else
            {
                lblText.Text = string.Empty;
            }

            // ===== Label THỜI GIAN =====
            Label lblTime = new Label();
            lblTime.AutoSize = true;
            lblTime.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            lblTime.ForeColor = Color.Gray;
            lblTime.Text = FormatTimestamp(msg.Timestamp);

            // Thêm vào bong bóng theo thứ tự: Tên -> Nội dung -> Thời gian
            bongBong.Controls.Add(lblNameUser);
            bongBong.Controls.Add(lblText);
            bongBong.Controls.Add(lblTime);

            // ===== Container để căn trái / phải =====
            FlowLayoutPanel khungCan = new FlowLayoutPanel();
            khungCan.AutoSize = false;
            khungCan.WrapContents = false;
            khungCan.Width = flbKhungChat.ClientSize.Width;
            khungCan.Padding = new Padding(16, 0, 16, 0);
            khungCan.Margin = new Padding(0, 0, 0, 0);

            if (laTinCuaToi)
            {
                khungCan.FlowDirection = FlowDirection.RightToLeft;
            }
            else
            {
                khungCan.FlowDirection = FlowDirection.LeftToRight;
            }

            khungCan.Controls.Add(bongBong);
            flbKhungChat.Controls.Add(khungCan);
        }

        /// <summary>
        /// Chuyển timestamp (ms) thành chuỗi dd/MM/yyyy HH:mm.
        /// </summary>
        private static string FormatTimestamp(long timestamp)
        {
            if (timestamp <= 0)
            {
                return string.Empty;
            }

            try
            {
                DateTime thoiGian =
                    DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
                return thoiGian.ToString("dd/MM/yyyy HH:mm");
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region ====== GỬI TIN NHẮN ======

        private async void btnGui_Click(object sender, EventArgs e)
        {
            string noiDungTin = string.Empty;

            if (txtNhapTinNhan.Text != null)
            {
                noiDungTin = txtNhapTinNhan.Text.Trim();
            }

            if (string.IsNullOrEmpty(noiDungTin))
            {
                return;
            }

            if (string.IsNullOrEmpty(idNguoiDangChat))
            {
                MessageBox.Show(
                    "Vui lòng chọn người cần nhắn tin ở danh sách bên trái.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            txtNhapTinNhan.Clear();

            try
            {
                await boDieuKhienNhanTin.SendMessageAsync(idNguoiDangChat, noiDungTin);
                // Tin sẽ tự hiện lên nhờ realtime và RenderMessages.
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi gửi tin nhắn: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}

