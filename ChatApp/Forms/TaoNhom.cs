using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChatApp.Forms
{
    public partial class TaoNhom : Form
    {
        #region ====== DATA ======
        /// <summary>
        /// localId của user hiện tại.
        /// </summary>
        private readonly string LocalId;

        /// <summary>
        /// Token đăng nhập (để dành nếu dùng).
        /// </summary>
        private readonly string Token;

        private readonly List<KeyValuePair<string, User>> _friends;

        public string GroupName { get; private set; }
        public List<string> SelectedMemberIds { get; private set; }

        /// <summary>
        /// Dịch vụ để cập nhật chế độ ngày đêm (dark/light).
        /// </summary>
        private readonly ThemeService _themeService = new ThemeService();
        #endregion

        #region ====== CTOR ======

        public TaoNhom(Dictionary<string, User> friends, string localId, string token)
        {
            InitializeComponent();

            LocalId = localId;
            Token = token;

            _friends = (friends ?? new Dictionary<string, User>())
                .Where(x => x.Value != null)
                .OrderBy(x => x.Value.FullName ?? x.Value.DisplayName ?? x.Value.Email ?? "")
                .ToList();

            SelectedMemberIds = new List<string>();

            LoadFriends();
            LoadTheme();
        }

        #endregion

        #region ====== LOAD DATA ======

        private async void LoadTheme()
        {
            // Load chế độ ngày đêm
            bool isDark = await _themeService.GetThemeAsync(LocalId);
            ThemeManager.ApplyTheme(this, isDark);
        }

        private void LoadFriends()
        {
            pnlMembers.Controls.Clear();

            int y = 12;

            foreach (var kv in _friends)
            {
                string id = kv.Key;
                User u = kv.Value;

                var chk = new Guna2CheckBox
                {
                    Text = GetName(u, id),
                    Tag = id,                // lưu localId
                    AutoSize = true,
                    Dock = DockStyle.Top,
                    Font = new Font("Segoe UI", 10F),
                };

                pnlMembers.Controls.Add(chk);
                y += chk.Height + 12;
            }

            //clbThanhVien.Items.Clear();

            //foreach (var kv in _friends)
            //{
            //    string id = kv.Key;
            //    User u = kv.Value;

            //    clbThanhVien.Items.Add(
            //        new Item { Id = id, Text = GetName(u, id) },
            //        false
            //    );
            //}
        }

        private static string GetName(User u, string id)
        {
            if (u == null) return id ?? "Người dùng";

            return (u.FullName
                ?? u.DisplayName
                ?? u.Email
                ?? id).Trim();
        }

        #endregion

        #region ====== EVENTS ======

        private void btnTao_Click(object sender, EventArgs e)
        {
            string ten = txtTenNhom.Text.Trim();
            if (string.IsNullOrEmpty(ten))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm.");
                txtTenNhom.Focus();
                return;
            }

            var selected = pnlMembers.Controls
            .OfType<Guna2CheckBox>()
            .Where(c => c.Checked)
            .Select(c => c.Tag?.ToString())
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();

            //var selected = new List<string>();

            //foreach (var obj in clbThanhVien.CheckedItems)
            //{
            //    if (obj is Item it && !string.IsNullOrEmpty(it.Id))
            //        selected.Add(it.Id);
            //}

            if (selected.Count == 0)
            {
                MessageBox.Show("Chọn ít nhất 1 thành viên.");
                return;
            }

            GroupName = ten;
            SelectedMemberIds = selected;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region ====== INTERNAL ITEM ======

        private class Item
        {
            public string Id;
            public string Text;
            public override string ToString() => Text;
        }

        #endregion
    }
}
