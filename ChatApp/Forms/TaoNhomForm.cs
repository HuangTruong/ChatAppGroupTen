using ChatApp.Models.Users;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChatApp.Forms
{
    /// <summary>
    /// Form tạo nhóm đơn giản:
    /// - Nhập tên nhóm
    /// - Chọn thành viên (từ danh sách bạn bè)
    /// </summary>
    public class TaoNhomForm : Form
    {
        #region ====== UI CONTROLS ======

        private TextBox _txtTenNhom;
        private CheckedListBox _clbThanhVien;
        private Button _btnTao;
        private Button _btnHuy;

        #endregion

        #region ====== DATA ======

        private readonly List<KeyValuePair<string, User>> _friends;

        /// <summary>
        /// Tên nhóm sau khi OK.
        /// </summary>
        public string GroupName { get; private set; }

        /// <summary>
        /// Danh sách memberId được chọn (safeId).
        /// </summary>
        public List<string> SelectedMemberIds { get; private set; }

        #endregion

        #region ====== CTOR ======

        public TaoNhomForm(Dictionary<string, User> friends)
        {
            _friends = (friends ?? new Dictionary<string, User>())
                .Where(x => x.Value != null)
                .OrderBy(x => (x.Value.FullName ?? x.Value.DisplayName ?? x.Value.Email ?? string.Empty))
                .ToList();

            SelectedMemberIds = new List<string>();

            BuildUi();
        }

        #endregion

        #region ====== UI BUILD ======

        private void BuildUi()
        {
            Text = "Tạo nhóm";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 460;
            Height = 520;

            Label lbl1 = new Label();
            lbl1.Text = "Tên nhóm:";
            lbl1.AutoSize = true;
            lbl1.Left = 12;
            lbl1.Top = 15;

            _txtTenNhom = new TextBox();
            _txtTenNhom.Left = 12;
            _txtTenNhom.Top = 38;
            _txtTenNhom.Width = 420;

            Label lbl2 = new Label();
            lbl2.Text = "Chọn thành viên:";
            lbl2.AutoSize = true;
            lbl2.Left = 12;
            lbl2.Top = 75;

            _clbThanhVien = new CheckedListBox();
            _clbThanhVien.Left = 12;
            _clbThanhVien.Top = 98;
            _clbThanhVien.Width = 420;
            _clbThanhVien.Height = 320;
            _clbThanhVien.CheckOnClick = true;

            foreach (var kv in _friends)
            {
                string id = kv.Key;
                User u = kv.Value;
                string name = GetName(u, id);
                _clbThanhVien.Items.Add(new Item { Id = id, Text = name }, false);
            }

            _btnTao = new Button();
            _btnTao.Text = "Tạo";
            _btnTao.Width = 90;
            _btnTao.Left = 342;
            _btnTao.Top = 430;
            _btnTao.Click += BtnTao_Click;

            _btnHuy = new Button();
            _btnHuy.Text = "Hủy";
            _btnHuy.Width = 90;
            _btnHuy.Left = 242;
            _btnHuy.Top = 430;
            _btnHuy.DialogResult = DialogResult.Cancel;

            Controls.Add(lbl1);
            Controls.Add(_txtTenNhom);
            Controls.Add(lbl2);
            Controls.Add(_clbThanhVien);
            Controls.Add(_btnHuy);
            Controls.Add(_btnTao);

            AcceptButton = _btnTao;
            CancelButton = _btnHuy;
        }

        private static string GetName(User u, string id)
        {
            if (u == null) return id ?? "Người dùng";

            string name = u.FullName;
            if (string.IsNullOrWhiteSpace(name)) name = u.DisplayName;
            if (string.IsNullOrWhiteSpace(name)) name = u.Email;

            if (string.IsNullOrWhiteSpace(name)) name = id;

            return name.Trim();
        }

        #endregion

        #region ====== EVENTS ======

        private void BtnTao_Click(object sender, EventArgs e)
        {
            string ten = (_txtTenNhom.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(ten))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _txtTenNhom.Focus();
                return;
            }

            var selected = new List<string>();

            foreach (var obj in _clbThanhVien.CheckedItems)
            {
                Item it = obj as Item;
                if (it != null && !string.IsNullOrEmpty(it.Id))
                {
                    selected.Add(it.Id);
                }
            }

            if (selected.Count == 0)
            {
                MessageBox.Show("Chọn ít nhất 1 thành viên.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            GroupName = ten;
            SelectedMemberIds = selected;

            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        #region ====== INTERNAL ITEM ======

        private class Item
        {
            public string Id;
            public string Text;

            public override string ToString()
            {
                return Text;
            }
        }

        #endregion
    }
}
