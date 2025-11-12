using Application.Common.Authorization;
using Application.Common.Interfaces;
using FakeItEasy;
using MediatR;

public abstract class AuthorizationBehaviorTestBase
{
    protected readonly ICurrentUser CurrentUser = A.Fake<ICurrentUser>();
    protected readonly IChatAccess ChatAccess = A.Fake<IChatAccess>();
    protected readonly IFriendInvitationAccess InvitationAccess = A.Fake<IFriendInvitationAccess>();
    protected readonly IFriendshipAccess FriendshipAccess = A.Fake<IFriendshipAccess>();

    protected readonly Guid _sampleGuid1 = Guid.NewGuid();
    protected readonly Guid _sampleGuid2 = Guid.NewGuid();

    protected static readonly RequestHandlerDelegate<Unit> Next = (ct) => Task.FromResult(Unit.Value);

    protected record Unit
    {
        public static readonly Unit Value = new();
    }
}