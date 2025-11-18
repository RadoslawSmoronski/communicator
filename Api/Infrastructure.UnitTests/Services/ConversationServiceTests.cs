using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using FluentAssertions;
using Shared.Result;

namespace Infrastructure.UnitTests.Services;

public class ConversationServiceTests
{
    private readonly UserManager<UserAccount> _userManager;
    private readonly ILogger<ConversationService> _logger = A.Fake<ILogger<ConversationService>>();
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IConversationRepository _conversationRepo = A.Fake<IConversationRepository>();
    private readonly IMapper _mapper = A.Fake<IMapper>();

    private readonly ConversationService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _friendId = Guid.NewGuid();

    public ConversationServiceTests()
    {
        A.CallTo(() => _unitOfWork.Conversations).Returns(_conversationRepo);
        
        _userManager = A.Fake<UserManager<UserAccount>>();

        _service = new ConversationService(
            _userManager,
            _logger,
            _unitOfWork,
            _mapper
        );
    }
    
    [Fact]
    public async Task GetOrCreateAsync_Should_ReturnValidationError_WhenUserEqualsFriend()
    {
        // Act
        var result = await _service.GetOrCreateAsync(_userId, _userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.Validation);
    }
    
    [Fact]
    public async Task GetOrCreateAsync_Should_ReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        A.CallTo(() => _userManager.FindByIdAsync(_userId.ToString()))
            .Returns((UserAccount?)null);
    
        // Act
        var result = await _service.GetOrCreateAsync(_userId, _friendId);
    
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetOrCreateAsync_Should_ReturnNotFound_WhenFriendNotFound()
    {
        // Arrange
        var user = new UserAccount { Id = _userId, UserName = "user" };
    
        A.CallTo(() => _userManager.FindByIdAsync(_userId.ToString()))
            .Returns(user);
    
        A.CallTo(() => _userManager.FindByIdAsync(_friendId.ToString()))
            .Returns((UserAccount?)null);
    
        // Act
        var result = await _service.GetOrCreateAsync(_userId, _friendId);
    
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error!.Code.Should().Be("Friend.NotFound");
    }

    [Fact]
    public async Task GetOrCreateAsync_Should_ReturnExistingConversation_WhenExists()
    {
        // Arrange
        var user = new UserAccount { Id = _userId, UserName = "user" };
        var friend = new UserAccount { Id = _friendId, UserName = "friend" };
    
        A.CallTo(() => _userManager.FindByIdAsync(_userId.ToString())).Returns(user);
        A.CallTo(() => _userManager.FindByIdAsync(_friendId.ToString())).Returns(friend);
    
        var existing = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = _userId,
            User2Id = _friendId
        };
    
        A.CallTo(() => _conversationRepo.GetConversationByUsersIdAsync(_userId, _friendId))
            .Returns(existing);
    
        // Act
        var result = await _service.GetOrCreateAsync(_userId, _friendId);
    
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existing);
    }

    [Fact]
    public async Task GetOrCreateAsync_Should_CreateConversation_WhenNotExists()
    {
        // Arrange
        var user = new UserAccount { Id = _userId, UserName = "user" };
        var friend = new UserAccount { Id = _friendId, UserName = "friend" };
    
        A.CallTo(() => _userManager.FindByIdAsync(_userId.ToString())).Returns(user);
        A.CallTo(() => _userManager.FindByIdAsync(_friendId.ToString())).Returns(friend);
    
        A.CallTo(() => _conversationRepo.GetConversationByUsersIdAsync(_userId, _friendId))
            .Returns((Conversation?)null);
    
        Conversation? createdConversation = null;
    
        A.CallTo(() => _conversationRepo.AddAsync(A<Conversation>.Ignored))
            .Invokes((Conversation c) => createdConversation = c)
            .Returns(Task.CompletedTask);
    
        // Act
        var result = await _service.GetOrCreateAsync(_userId, _friendId);
    
        // Assert
        result.IsSuccess.Should().BeTrue();
        createdConversation.Should().NotBeNull();
        createdConversation!.User1Id.Should().Be(_userId);
        createdConversation.User2Id.Should().Be(_friendId);
    
        A.CallTo(() => _unitOfWork.SaveAsync())
            .MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task GetOrCreateAsync_Should_ReturnFailure_WhenExceptionThrown()
    {
        // Arrange
        A.CallTo(() => _userManager.FindByIdAsync(_userId.ToString()))
            .ThrowsAsync(new Exception("DB failed"));
    
        // Act
        var result = await _service.GetOrCreateAsync(_userId, _friendId);
    
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.Failure);
        result.Error!.Code.Should().Be("Conversation.Failure");
    }
}
