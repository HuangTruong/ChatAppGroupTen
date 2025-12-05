using ChatApp.Forms;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class NhanTin : Form
    {
        private readonly string _localId;
        private readonly string _token;
        public NhanTin(string localId, string token)
        {
            InitializeComponent();
            _localId = localId;
            _token = token;
        }

        private void btnSearchFriends_Click(object sender, System.EventArgs e)
        {
            var timKiemBanBeForm = new TimKiemBanBe(_localId, _token);
            timKiemBanBeForm.FormClosed += (s, args) => this.Show();
            timKiemBanBeForm.Show();
        }

        private void btnRequest_Click(object sender, System.EventArgs e)
        {
            var loiMoiKetBanForm = new FormLoiMoiKetBan(_localId, _token);
            loiMoiKetBanForm.FormClosed += (s, args) => this.Show();
            loiMoiKetBanForm.Show();
        }
    }
}
