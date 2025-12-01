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
    }
}
