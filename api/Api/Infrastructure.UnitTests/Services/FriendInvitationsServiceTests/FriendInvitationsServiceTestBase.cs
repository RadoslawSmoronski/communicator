using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using Application.Repositories;
using Domain.Entities;
using FakeItEasy;
using Infrastructure.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.UnitTests.Services.FriendInvitationsServiceTests
{
    public abstract class FriendInvitationsServiceTestBase
    {
        protected readonly UserManager<UserAccount> UserManager;
        protected readonly IUnitOfWork UnitOfWork;
        protected readonly ILogger<FriendInvitationsService> Logger;
        protected readonly ICurrentUser CurrentUser;
        protected readonly IUserAvatarService UserAvatarService;

        protected readonly Guid SampleSenderId = Guid.NewGuid();
        protected readonly Guid SampleRecipientId = Guid.NewGuid();
        protected readonly UserAccount SampleSender;
        protected readonly UserAccount SampleRecipient;
        protected readonly FriendshipInvitation SampleInvitation;

        protected FriendInvitationsServiceTestBase()
        {
            UserManager = A.Fake<UserManager<UserAccount>>();
            UnitOfWork = A.Fake<IUnitOfWork>();
            Logger = A.Fake<ILogger<FriendInvitationsService>>();
            CurrentUser = A.Fake<ICurrentUser>();
            UserAvatarService = A.Fake<IUserAvatarService>();

            SampleSender = new UserAccount
            {
                Id = SampleSenderId,
                UserName = "sender",
                EmailConfirmed = true
            };

            SampleRecipient = new UserAccount
            {
                Id = SampleRecipientId,
                UserName = "recipient",
                EmailConfirmed = true
            };

            SampleInvitation = new FriendshipInvitation
            {
                Id = Guid.NewGuid(),
                SenderId = SampleSenderId,
                RecipientId = SampleRecipientId
            };
        }

        protected FriendInvitationsService CreateService()
        {
            return new FriendInvitationsService(UserManager, UnitOfWork, Logger, CurrentUser, UserAvatarService);
        }
    }
}
