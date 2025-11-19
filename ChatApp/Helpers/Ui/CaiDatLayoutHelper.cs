using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    /// <summary>
    /// Các hàm layout riêng cho form Cài Đặt (CatDat).
    /// </summary>
    public static class CaiDatLayoutHelper
    {
        /// <summary>
        /// Căn giữa tiêu đề theo chiều ngang trong container.
        /// </summary>
        public static void CenterTitle(Label titleLabel, Control container)
        {
            if (titleLabel == null || container == null) return;
            titleLabel.Left = (container.Width - titleLabel.Width) / 2;
        }

        /// <summary>
        /// Căn giữa avatar và nút "Đổi avatar" trong cùng một container.
        /// </summary>
        public static void CenterAvatarAndButton(
            Control avatar,
            Control changeAvatarButton,
            Control container)
        {
            if (avatar == null || changeAvatarButton == null || container == null) return;

            int centerX = container.Width / 2;

            avatar.Location = new Point(
                centerX - avatar.Width / 2,
                avatar.Location.Y);

            changeAvatarButton.Location = new Point(
                centerX - changeAvatarButton.Width / 2,
                changeAvatarButton.Location.Y);
        }

        /// <summary>
        /// Căn lại CÁC CỘT cho 2 hàng:
        /// - lblTenDangNhap & lblEmail thẳng hàng
        /// - txtTenDangNhap & txtEmail thẳng hàng
        /// - btnCopyUsername & btnCopyEmail thẳng hàng
        /// Đồng thời cả cụm được căn giữa theo container.
        /// </summary>
        public static void AlignAccountEmailRows(
            Label lblUserName,
            Control txtUserName,
            Control btnCopyUser,
            Label lblEmail,
            Control txtEmail,
            Control btnCopyEmail,
            Control container,
            int margin = 8)
        {
            if (container == null ||
                lblUserName == null || txtUserName == null || btnCopyUser == null ||
                lblEmail == null || txtEmail == null || btnCopyEmail == null)
            {
                return;
            }

            int labelWidth = lblUserName.Width;
            int textWidth = txtUserName.Width;
            int btnWidth = btnCopyUser.Width;

            int groupWidth = labelWidth + margin + textWidth + margin + btnWidth;
            int startX = (container.Width - groupWidth) / 2;

            // Hàng 1: tên đăng nhập
            int yLabel1 = lblUserName.Top;
            int yText1 = txtUserName.Top;
            int yBtn1 = btnCopyUser.Top;

            lblUserName.Location = new Point(startX, yLabel1);
            txtUserName.Location = new Point(lblUserName.Left + labelWidth + margin, yText1);
            btnCopyUser.Location = new Point(txtUserName.Left + textWidth + margin, yBtn1);

            // Hàng 2: email
            int yLabel2 = lblEmail.Top;
            int yText2 = txtEmail.Top;
            int yBtn2 = btnCopyEmail.Top;

            lblEmail.Location = new Point(startX, yLabel2);
            txtEmail.Location = new Point(txtUserName.Left, yText2);
            btnCopyEmail.Location = new Point(btnCopyUser.Left, yBtn2);
        }

        /// <summary>
        /// Căn giữa 3 nút ở dưới: Đổi mật khẩu – Đổi email – Đóng.
        /// </summary>
        public static void CenterBottomButtons(
            Control btnChangePassword,
            Control btnChangeEmail,
            Control btnClose,
            Control container,
            int margin = 20)
        {
            if (container == null ||
                btnChangePassword == null ||
                btnChangeEmail == null ||
                btnClose == null)
            {
                return;
            }

            int totalWidth =
                btnChangePassword.Width +
                margin +
                btnChangeEmail.Width +
                margin +
                btnClose.Width;

            int startX = (container.Width - totalWidth) / 2;
            int y = btnChangePassword.Location.Y;

            btnChangePassword.Location = new Point(startX, y);
            btnChangeEmail.Location = new Point(startX + btnChangePassword.Width + margin, y);
            btnClose.Location = new Point(
                startX + btnChangePassword.Width + margin + btnChangeEmail.Width + margin,
                y);
        }
    }
}
