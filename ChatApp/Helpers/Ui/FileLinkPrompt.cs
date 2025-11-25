using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    public static class FileLinkPrompt
    {
        /// <summary>
        /// Hiển thị dialog nhập link và trả về chuỗi user nhập.
        /// Trả về null nếu bấm Hủy.
        /// </summary>
        /// <param name="text">Nội dung hướng dẫn (label trên dialog).</param>
        /// <param name="caption">Tiêu đề cửa sổ.</param>
        public static string ShowDialog(string text, string caption)
        {
            // Tạo form nhỏ
            using (var prompt = new Form())
            {
                prompt.Width = 420;
                prompt.Height = 180;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.Text = caption;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;
                prompt.ShowIcon = false;
                prompt.ShowInTaskbar = false;

                // Label hướng dẫn
                var lblText = new Label
                {
                    Left = 12,
                    Top = 12,
                    AutoSize = true,
                    MaximumSize = new Size(380, 0),
                    Text = text
                };

                // Ô nhập link
                var txtInput = new TextBox
                {
                    Left = 12,
                    Top = lblText.Bottom + 10,
                    Width = 380,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                // Nút OK
                var btnOk = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Left = 220,
                    Width = 80,
                    Top = txtInput.Bottom + 15,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };

                // Nút Hủy
                var btnCancel = new Button
                {
                    Text = "Hủy",
                    DialogResult = DialogResult.Cancel,
                    Left = 312,
                    Width = 80,
                    Top = txtInput.Bottom + 15,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };

                // Gắn Accept/Cancel
                prompt.AcceptButton = btnOk;
                prompt.CancelButton = btnCancel;

                // Thêm control vào form
                prompt.Controls.Add(lblText);
                prompt.Controls.Add(txtInput);
                prompt.Controls.Add(btnOk);
                prompt.Controls.Add(btnCancel);

                // Hiển thị dialog
                var result = prompt.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // Trả về link (có thể là rỗng nếu user không nhập gì)
                    return txtInput.Text;
                }

                // User bấm Hủy → trả về null
                return null;
            }
        }
    }
}
