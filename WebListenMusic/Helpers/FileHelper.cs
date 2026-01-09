namespace WebListenMusic.Helpers
{
    /// <summary>
    /// Lớp hỗ trợ xử lý file - Cung cấp các phương thức upload, xóa và định dạng file
    /// </summary>
    public class FileHelper
    {
        // Môi trường web host để lấy đường dẫn wwwroot
        private readonly IWebHostEnvironment _environment;
        
        // Cấu hình ứng dụng để đọc giới hạn kích thước file
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Khởi tạo FileHelper với dependency injection
        /// </summary>
        /// <param name="environment">Môi trường hosting</param>
        /// <param name="configuration">Cấu hình ứng dụng</param>
        public FileHelper(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        /// <summary>
        /// Upload file lên server với validation
        /// - Kiểm tra định dạng file cho phép
        /// - Kiểm tra kích thước file tối đa
        /// - Tạo tên file unique để tránh trùng lặp
        /// </summary>
        /// <param name="file">File cần upload</param>
        /// <param name="folder">Thư mục đích (songs, covers, avatars)</param>
        /// <param name="allowedExtensions">Danh sách phần mở rộng cho phép</param>
        /// <returns>Đường dẫn URL của file đã upload</returns>
        public async Task<string?> UploadFileAsync(IFormFile file, string folder, string[]? allowedExtensions = null)
        {
            // Kiểm tra file rỗng
            if (file == null || file.Length == 0)
                return null;

            // Lấy và kiểm tra phần mở rộng file
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (allowedExtensions != null && !allowedExtensions.Contains(extension))
                throw new InvalidOperationException($"File extension {extension} is not allowed.");

            // Lấy giới hạn kích thước từ cấu hình
            // File nhạc: 50MB (52428800 bytes)
            // File ảnh: 5MB (5242880 bytes)
            var maxSize = folder switch
            {
                "songs" => _configuration.GetValue<long>("FileSettings:MaxAudioSize", 52428800),
                _ => _configuration.GetValue<long>("FileSettings:MaxImageSize", 5242880)
            };

            // Kiểm tra kích thước file
            if (file.Length > maxSize)
                throw new InvalidOperationException($"File size exceeds the maximum allowed size of {maxSize / 1024 / 1024}MB.");

            // Tạo thư mục uploads nếu chưa tồn tại
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Tạo tên file unique bằng GUID để tránh trùng lặp
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Ghi file vào đĩa
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn URL tương đối
            return $"/uploads/{folder}/{uniqueFileName}";
        }

        /// <summary>
        /// Xóa file từ server
        /// - Bỏ qua file mặc định (chứa "default" trong tên)
        /// - Kiểm tra file tồn tại trước khi xóa
        /// </summary>
        /// <param name="fileUrl">Đường dẫn URL của file cần xóa</param>
        public void DeleteFile(string? fileUrl)
        {
            // Bỏ qua nếu URL rỗng hoặc là file mặc định
            if (string.IsNullOrEmpty(fileUrl) || fileUrl.Contains("default"))
                return;

            // Chuyển đổi URL sang đường dẫn vật lý
            var filePath = Path.Combine(_environment.WebRootPath, fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            
            // Xóa file nếu tồn tại
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Phương thức static để lưu file - Sử dụng trong Controllers
        /// Không cần dependency injection, truyền trực tiếp webRootPath
        /// </summary>
        /// <param name="file">File cần lưu</param>
        /// <param name="webRootPath">Đường dẫn thư mục wwwroot</param>
        /// <param name="folder">Thư mục đích</param>
        /// <returns>Đường dẫn URL của file đã lưu</returns>
        public static async Task<string?> SaveFileAsync(IFormFile file, string webRootPath, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uploadsFolder = Path.Combine(webRootPath, folder);
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Tạo tên file unique
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{folder}/{uniqueFileName}";
        }

        /// <summary>
        /// Phương thức static để xóa file - Sử dụng trong Controllers
        /// </summary>
        /// <param name="webRootPath">Đường dẫn thư mục wwwroot</param>
        /// <param name="fileUrl">Đường dẫn URL của file cần xóa</param>
        public static void DeleteFile(string webRootPath, string? fileUrl)
        {
            // Bỏ qua nếu URL rỗng hoặc là file mặc định
            if (string.IsNullOrEmpty(fileUrl) || fileUrl.Contains("default"))
                return;

            var filePath = Path.Combine(webRootPath, fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Định dạng thời lượng từ giây sang chuỗi hiển thị
        /// Ví dụ: 185 giây -> "3:05", 3725 giây -> "1:02:05"
        /// </summary>
        /// <param name="seconds">Thời lượng tính bằng giây</param>
        /// <returns>Chuỗi định dạng m:ss hoặc h:mm:ss</returns>
        public static string FormatDuration(int seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            return timeSpan.Hours > 0 
                ? timeSpan.ToString(@"h\:mm\:ss") 
                : timeSpan.ToString(@"m\:ss");
        }

        /// <summary>
        /// Định dạng số lớn thành chuỗi rút gọn
        /// Ví dụ: 1500 -> "1.5K", 2500000 -> "2.5M", 1000000000 -> "1B"
        /// Dùng để hiển thị lượt nghe, lượt thích, số người theo dõi
        /// </summary>
        /// <param name="number">Số cần định dạng</param>
        /// <returns>Chuỗi rút gọn với đơn vị K/M/B</returns>
        public static string FormatNumber(int number)
        {
            // Tỷ (Billion)
            if (number >= 1000000000)
                return (number / 1000000000D).ToString("0.#") + "B";
            // Triệu (Million)
            if (number >= 1000000)
                return (number / 1000000D).ToString("0.#") + "M";
            // Nghìn (Thousand)
            if (number >= 1000)
                return (number / 1000D).ToString("0.#") + "K";
            return number.ToString();
        }
    }
}
