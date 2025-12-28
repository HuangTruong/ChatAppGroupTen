# ChatAppGroupTen (WinForms) — Realtime Chat using Firebase

ChatApp là ứng dụng chat realtime viết bằng **C# WinForms**, sử dụng **Firebase Authentication** và **Firebase Realtime Database** để đăng nhập, quản lý bạn bè, chat **1-1** & **nhóm** theo thời gian thực.  
Hỗ trợ gửi **tin nhắn văn bản**, **file đính kèm** *(upload lên Catbox để lấy URL)*, **Theme Light/Dark**, và một số tính năng UX như danh sách hội thoại, bong bóng chat, emoji,...

---

## ✨ Tính năng chính

- ✅ Đăng ký / đăng nhập bằng Firebase Auth (Email/Password)
- ✅ Xác thực email (OTP qua SMTP)
- ✅ Quên mật khẩu (Firebase reset email)
- ✅ Tìm kiếm người dùng, gửi lời mời kết bạn
- ✅ Chấp nhận / từ chối / hủy lời mời, hủy kết bạn
- ✅ Chat 1-1 realtime (listener)
- ✅ Chat nhóm realtime
- ✅ Gửi file (upload Catbox → lưu URL vào Firebase)
- ✅ Hiển thị bong bóng tin nhắn (MessageBubbles)
- ✅ Theme Light/Dark (lưu theo user)
- ✅ Quản lý nhóm: tạo nhóm, thêm thành viên, đổi tên nhóm, avatar nhóm

---

## 🧰 Công nghệ sử dụng

- **C# WinForms (.NET)**
- **Firebase Authentication**
- **Firebase Realtime Database**
- **FireSharp** (realtime listener)
- **HttpClient (REST)** cho một số nghiệp vụ
- **Catbox.moe** (file hosting) để upload file → lưu URL
- **SMTP (Gmail App Password)** để gửi OTP
- **Guna.UI2 WinForms** (UI Controls)

---

## 🏗️ Kiến trúc & tổ chức thư mục (tổng quan)

- **Forms**: giao diện WinForms (DangNhap, DangKy, TrangChu, NhanTin,...)
- **Controllers**: xử lý logic cho từng Form (NhanTinController, FriendController,...)
- **Services**: thao tác Firebase / Email / Upload / Theme (AuthService, FriendService, GroupService, MessageService,...)
- **Models**: User, ChatMessage, GroupInfo, FriendRequest, ThemeSetting,...
- **Controls**: UserControl (MessageBubbles, Conversations, UserListItem,...)
- **Helpers**: tiện ích (KeySanitizer, ImageBase64,...)

---

## 🗄️ Firebase Database Schema 

Các node chính:

- `users/{localId}`: thông tin user *(userName, displayName, avatar, gender, birthday, ...)*
- `emails/{base64(email)}`: map kiểm tra email tồn tại
- `status/{localId}/Status`: `online/offline`
- `friends/{userId}/{friendId} = true`
- `friendRequests/{receiverId}/{senderId}`: lời mời nhận được
- `outgoingRequests/{senderId}/{receiverId}`: lời mời đã gửi
- `groups/{groupId}`: metadata nhóm + `members` + `avatar`
- `groupsByUser/{userId}/{groupId} = true`
- `messages/{conversationId}/{messageId}`: tin nhắn 1-1
- `groupMessages/{groupId}/{messageId}`: tin nhắn nhóm

---

## 🚀 Hướng dẫn sử dụng nhanh

1. **Đăng ký** → nhận OTP → xác thực email  
2. **Đăng nhập**  
3. **Tìm bạn bè** → gửi lời mời → chấp nhận  
4. Chọn người dùng để **chat 1-1 realtime**  
5. Tạo **nhóm chat** → thêm thành viên → **chat nhóm**  
6. Gửi **file**: chọn file → upload Catbox → gửi URL  
7. Bật/tắt **Light/Dark mode** trong cài đặt

---

## 👤 Tác giả
- **Lê Minh Hoàng** — UIT (VNUHCM)
- **Trương Việt Hoàng** — UIT (VNUHCM)
- **Trần Phước Hoàng** — UIT (VNUHCM)
- **Huỳnh Vũ Khánh Hưng** — UIT (VNUHCM)  
- Project: **ChatApp** (WinForms + Firebase)

