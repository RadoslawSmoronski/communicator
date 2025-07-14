using AutoMapper;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ChatCommunicator.Application.Managers
{
    public class AccountManager : IAccountManager
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IUserAvatarService _userAvatarService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountManager> _logger;

        public AccountManager(UserManager<UserAccount> userManager,
            IMapper mapper,
            SignInManager<UserAccount> signInManager,
            ITokenService tokenService,
            IUserAvatarService userAvatarService,
            ILogger<AccountManager> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _tokenService = tokenService;
            _userAvatarService = userAvatarService;
            _logger = logger;
        }

        public async Task<ResultT<SimpleUserDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with username: {UserName}", registerDto.UserName);

                var user = new UserAccount { UserName = registerDto.UserName };
                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User registered successfully: {UserName}", registerDto.UserName);
                    var dto = _mapper.Map<SimpleUserDto>(user);
                    return dto;
                }

                var conflictError = result.Errors.FirstOrDefault(e => e.Code == "DuplicateUserName");
                if (conflictError != null)
                {
                    _logger.LogWarning("Registration conflict: username already exists - {UserName}", registerDto.UserName);
                    return Error.Conflict("CONFLICT", "A user with this username already exists.");
                }

                _logger.LogError("User registration failed unexpectedly for username: {UserName}", registerDto.UserName);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "User registration failed unexpectedly. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during registration of user: {UserName}", registerDto.UserName);
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }
        }

        public async Task<ResultT<LoggedUserDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Attempting login for username: {UserName}", loginDto.UserName);
                var user = await _userManager.FindByNameAsync(loginDto.UserName);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: user not found - {UserName}", loginDto.UserName);
                    return Error.Unauthorized("UNAUTHORIZED", "Username or password is incorrect.");
                }
                ;

                var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in successfully: {UserName}", loginDto.UserName);

                    var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);
                    var accessToken = await _tokenService.CreateAccessTokenAsync(user);

                    if (refreshToken.IsSuccess && accessToken.IsSuccess)
                    {
                        var resultObj = new LoggedUserDto()
                        {
                            UserName = loginDto.UserName,
                            Id = user.Id,
                            AvatarUrl = _userAvatarService.GetPublicAvatarUrl(user.AvatarUrl),
                            AccessToken = accessToken.Value,
                            RefreshToken = refreshToken.Value
                        };

                        return resultObj;
                    }
                }

                _logger.LogWarning("Login failed for username: {UserName}", loginDto.UserName);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "User logging failed unexpectedly. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during login of user: {UserName}", loginDto.UserName);
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }
        }

        public async Task<ResultT<string>> ChangeUsernameAsync(Guid userId, string newUsername)
        {
            _logger.LogInformation("User {UserId} requested username change to: {NewUsername}", userId, newUsername);

            if (string.IsNullOrWhiteSpace(newUsername))
            {
                _logger.LogWarning("Username change failed: new username is empty or null");
                return Error.Validation("VALIDATION", "Username cannot be empty or null.");
            }
            ;

            if (Guid.Empty == userId)
            {
                _logger.LogWarning("Username change failed: userId is empty or null");
                return Error.Validation("VALIDATION_USERID", "UserId cannot be empty or null.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("Username change failed: user not found - {UserId}", userId);
                    return Error.Unauthorized("UNAUTHORIZED", "The user associated with the access token does not exist. Please log in again.");
                };

                var isUsernameExists = await _userManager.FindByNameAsync(newUsername);

                if (isUsernameExists != null)
                {
                    _logger.LogWarning("Username change conflict: username already taken - {NewUsername}", newUsername);
                    return Error.Conflict("CONFLICT", "The chosen username is already taken. Please choose a different one.");
                }

                var result = await _userManager.SetUserNameAsync(user, newUsername);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Username changed successfully for user {UserId} to {NewUsername}", userId, newUsername);
                    return newUsername;
                }

                _logger.LogError("Username change failed unexpectedly for user {UserId}", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "User registration failed unexpectedly. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during username change for user {UserId}", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }
        }

        public async Task<ResultT<string>> UploadAvatarAsync(Guid userId, IFormFile? file)
        {
            if(Guid.Empty == userId)
            {
                _logger.LogWarning("UploadAvatarAsync failed: userId is empty or null.");
                return Error.Validation("VALIDATION_USERID", "UserId cannot be empty or null.");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                _logger.LogWarning("UploadAvatarAsync failed: user with ID {UserId} not found.", userId);
                return Error.Unauthorized("USER_NOT_FOUND", "User associated with the id does not exist. Please log in again.");
            }
            else if(String.IsNullOrEmpty(user.AvatarUrl) == false)
            {
                _logger.LogWarning("User {UserId} attempted to set avatar via POST but avatar is already set: {AvatarUrl}", userId, user.AvatarUrl);
                return Error.Conflict("AVATAR_ALREADY_SET", "Avatar is already set.");
            }

            var uploadFileResult = await _userAvatarService.UploadAvatarAsync(file);

            if (uploadFileResult.IsSuccess)
            {
                user.AvatarUrl = uploadFileResult.Value;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} avatar updated successfully.", userId);
                    return uploadFileResult.Value;
                }

                _logger.LogError("Failed to update avatar URL for user {UserId}. Errors: {Errors}", userId, string.Join(", ", result.Errors));
                return Error.Unknown("USER_UPDATE_FAILED", "Failed to update user avatar URL in database.");
            }
            else if (uploadFileResult.Error != null)
            {
                _logger.LogError("Avatar upload failed for user {UserId}. Error: {ErrorCode} - {ErrorMessage}",
                    userId, uploadFileResult.Error.Code, uploadFileResult.Error.Description);
                return uploadFileResult.Error;
            }

            _logger.LogError("UploadAvatarAsync ended with unknown error for user {UserId}.", userId);
            return Error.Unknown("AVATAR_UPLOAD_FAILED", "Failed to upload avatar file.");
        }

    }
}
