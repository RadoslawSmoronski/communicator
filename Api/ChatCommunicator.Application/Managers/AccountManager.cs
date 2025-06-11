using AutoMapper;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using ChatCommunicator.Contracts.Dtos.Service;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ChatCommunicator.Application.Managers
{
    public class AccountManager : IAccountManager
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountManager(UserManager<UserAccount> userManager,
            IMapper mapper,
            SignInManager<UserAccount> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<ResultT<SimpleUserDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var user = new UserAccount { UserName = registerDto.UserName };
                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    var dto = _mapper.Map<SimpleUserDto>(user);
                    return dto;
                }

                var conflictError = result.Errors.FirstOrDefault(e => e.Code == "DuplicateUserName");
                if (conflictError != null)
                {
                    return Error.Conflict("CONFLICT", "A user with this username already exists.");
                }

                return Error.Unknown("INTERNAL_SERVER_ERROR", "User registration failed unexpectedly. Please try again later or contact support.");
            }
            catch(Exception ex)
            {
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }
        }

        public async Task<ResultT<LoggedUserDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(loginDto.UserName);

                if (user == null)
                {
                    return Error.Unauthorized("UNAUTHORIZED", "Username or password is incorrect.");
                };

                var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

                if (result.Succeeded)
                {
                    var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);
                    var accessToken = await _tokenService.CreateAccessTokenAsync(user);

                    if (refreshToken.IsSuccess && accessToken.IsSuccess)
                    {
                        var resultObj = new LoggedUserDto()
                        {
                            UserName = loginDto.UserName,
                            Id = user.Id,
                            AccessToken = accessToken.Value,
                            RefreshToken = refreshToken.Value
                        };

                        return resultObj;
                    }

                }

                return Error.Unknown("INTERNAL_SERVER_ERROR", "User logging failed unexpectedly. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }
        }

        public async Task<ResultT<string>> ChangeUsernameAsync(string userId, string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
            {
                return Error.Validation("VALIDATION", "Username cannot be empty or null.");
            };

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Error.Validation("VALIDATION_USERID", "UserId cannot be empty or null.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Error.Unauthorized("UNAUTHORIZED", "The user associated with the access token does not exist. Please log in again.");
                };

                var isUsernameExists = await _userManager.FindByNameAsync(newUsername);

                if (isUsernameExists != null)
                {
                    return Error.Conflict("CONFLICT", "The chosen username is already taken. Please choose a different one.");
                }

                var result = await _userManager.SetUserNameAsync(user, newUsername);

                if (result.Succeeded)
                {
                    return newUsername;
                }

                return Error.Unknown("INTERNAL_SERVER_ERROR", "User registration failed unexpectedly. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }
        }

    }
}
