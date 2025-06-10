using AutoMapper;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using ChatCommunicator.Contracts.Dtos.Service;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Identity;

namespace ChatCommunicator.Application.Managers
{
    public class AccountManager : IAccountManager
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;

        public AccountManager(UserManager<UserAccount> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
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

        public Task<ResultT<string>> ChangeUsernameAsync(string newUsername)
        {
            throw new NotImplementedException();
        }

        public Task<ResultT<LoggedUserDto>> LoginAsync(LoginDto loginDto)
        {
            throw new NotImplementedException();
        }

        public Task<ResultT<RefreshAccessTokenDto>> RefreshAccessTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            throw new NotImplementedException();
        }
    }
}
