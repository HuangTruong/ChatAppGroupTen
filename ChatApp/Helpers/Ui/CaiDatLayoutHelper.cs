using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    /// <summary>
    /// Các hàm layout riêng cho form Cài Đặt (CaiDat).
    /// Dùng để căn chỉnh lại vị trí các control cho gọn, thẳng hàng và cân đối.
    /// </summary>
    public static class CaiDatLayoutHelper
    {
        #region ======== Căn tiêu đề ========

        /// <summary>
        /// Căn giữa tiêu đề theo chiều ngang trong container.
        /// </summary>
        /// <param name="titleLabel">Label tiêu đề cần căn giữa.</param>
        /// <param name="container">Control cha dùng để tính toán vị trí.</param>
        public static void CenterTitle(Label titleLabel, Control container)
        {
            if (titleLabel == null || container == null)
                return;

            titleLabel.Left = (container.Width - titleLabel.Width) / 2;
        }

        #endregion

        #region ======== Căn avatar + nút Đổi avatar ========

        /// <summary>
        /// Căn giữa avatar và nút "Đổi avatar" theo chiều ngang trong cùng một container.
        /// Giữ nguyên vị trí theo trục dọc (Y) hiện tại của từng control.
        /// </summary>
        /// <param name="avatar">Control avatar (thường là <see cref="PictureBox"/> hoặc <see cref="Guna.UI2.WinForms.Guna2CirclePictureBox"/>).</param>
        /// <param name="changeAvatarButton">Nút "Đổi avatar".</param>
        /// <param name="container">Control cha để tính tâm.</param>
        public static void CenterAvatarAndButton(
            Control avatar,
            Control changeAvatarButton,
            Control container)
        {
            if (avatar == null || changeAvatarButton == null || container == null)
                return;

            int centerX = container.Width / 2;

            avatar.Location = new Point(
                centerX - avatar.Width / 2,
                avatar.Location.Y);

            changeAvatarButton.Location = new Point(
                centerX - changeAvatarButton.Width / 2,
                changeAvatarButton.Location.Y);
        }

        #endregion

        #region ======== Căn 2 hàng: Tài khoản & Email ========

        /// <summary>
        /// Căn lại CÁC CỘT cho 2 hàng:
        /// - <paramref name="lblUserName"/> &amp; <paramref name="lblEmail"/> thẳng hàng.
        /// - <paramref name="txtUserName"/> &amp; <paramref name="txtEmail"/> thẳng hàng.
        /// - <paramref name="btnCopyUser"/> &amp; <paramref name="btnCopyEmail"/> thẳng hàng.
        /// Đồng thời cả cụm được căn giữa theo <paramref name="container"/>.
        /// </summary>
        /// <param name="lblUserName">Label "Tên đăng nhập".</param>
        /// <param name="txtUserName">Control textbox tên đăng nhập.</param>
        /// <param name="btnCopyUser">Nút copy tên đăng nhập.</param>
        /// <param name="lblEmail">Label "Email".</param>
        /// <param name="txtEmail">Control textbox email.</param>
        /// <param name="btnCopyEmail">Nút copy email.</param>
        /// <param name="container">Control cha dùng để căn giữa toàn bộ cụm.</param>
        /// <param name="margin">Khoảng cách giữa các cột (label – textbox – button).</param>
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

        #endregion

        #region ======== Căn cụm 3 nút dưới cùng ========

        /// <summary>
        /// Căn giữa 3 nút ở dưới: Đổi mật khẩu – Đổi email – Đóng.
        /// </summary>
        /// <param name="btnChangePassword">Nút "Đổi mật khẩu".</param>
        /// <param name="btnChangeEmail">Nút "Đổi email".</param>
        /// <param name="btnClose">Nút "Đóng".</param>
        /// <param name="container">Control cha để tính tâm.</param>
        /// <param name="margin">Khoảng cách giữa các nút.</param>
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

        #endregion
    }
}
