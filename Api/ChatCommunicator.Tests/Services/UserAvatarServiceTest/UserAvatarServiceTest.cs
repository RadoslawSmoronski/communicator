using ChatCommunicator.Application.Services;
using ChatCommunicator.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Tests.Services.UserAvatarServiceTest
{
    public abstract class UserAvatarServiceTest
    {
        protected readonly IUserAvatarService _userAvatarService;

        protected UserAvatarServiceTest()
        {
            _userAvatarService = new UserAvatarService();
        }

        protected IFormFile CreateFakeImage(int width, int height, long sizeInBytes)
        {
            byte[] fakeContent = new byte[sizeInBytes];
            new Random().NextBytes(fakeContent);

            var stream = new MemoryStream(fakeContent);

            var file = new FormFile(stream, 0, sizeInBytes, "Data", "fakeimage.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };

            return file;
        }
    }
}
