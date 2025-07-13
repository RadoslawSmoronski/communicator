using ChatCommunicator.Application.Services;
using ChatCommunicator.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

        protected IFormFile CreateFakeImage(int width, int height, ImageFormat imageFormat, string imagepath)
        {
            var ms = new MemoryStream(); // bez using!

            using (Bitmap image = new Bitmap(width, height))
            using (Graphics graphics = Graphics.FromImage(image))
            using (Pen pen = new Pen(Color.Red))
            {
                Rectangle rectangle = new Rectangle(50, 50, 200, 100);
                graphics.DrawRectangle(pen, rectangle);

                image.Save(ms, imageFormat);
            }

            ms.Position = 0;

            IFormFile file = new FormFile(ms, 0, ms.Length, "fakeimage", imagepath)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };

            return file;
        }

    }
}
