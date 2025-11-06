namespace ChatApp.Models.Users
{
    public class UserDto  // dùng khi DangNhap -> ResultAs<UserDto>()
    {
        public string Ten { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        // ... bổ sung nếu cần
    }
}
