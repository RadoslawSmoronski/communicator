using ChatCommunicator.Application.Services;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Infrastructure.Services.Interfaces;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;

namespace ChatCommunicator.Tests.Services.UserAvatarServiceTest
{
    public abstract class UserAvatarServiceTest
    {
        protected readonly IFileStorageService _fileStorageService;
        protected readonly IUserAvatarService _userAvatarService;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        protected UserAvatarServiceTest()
        {
            _fileStorageService = A.Fake<IFileStorageService>();
            _httpContextAccessor = A.Fake<IHttpContextAccessor>();

            _userAvatarService = new UserAvatarService(_fileStorageService, _httpContextAccessor);
        }

        protected IFormFile CreateFakeImageFormFile(int width = 100, int height = 100, string fileName = "test.png")
        {
            using var image = new Image<Rgba32>(width, height);
            image.Mutate(ctx => ctx.Fill(Color.Red));

            var ms = new MemoryStream();
            image.SaveAsPng(ms);
            ms.Position = 0;

            return new FormFile(ms, 0, ms.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }

    }
}
