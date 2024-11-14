using Api.Models.Dtos.Controllers.UserController;
using Api.Models.Dtos.Responses;
using Api.Models.Dtos.Responses.Interfaces;
using Api.Utilities.Result;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class BasicController : Controller
    {
        private readonly ResponseHttpFactory _responseFactory;
        private readonly IMapper _mapper;
        public BasicController(ResponseHttpFactory responseHttpFactory, IMapper mapper)
        {
            _responseFactory = responseHttpFactory;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> testApi()
        {
            var resultData = new Dictionary<string, RegisteredUserDto>
                        {
                            { "user", new RegisteredUserDto()
                                {
                                    UserName = "test"
                                }
                            }
                        };

            var response = _responseFactory.Create<Dictionary<string, RegisteredUserDto>>
                (ResponseHttpType.Success, "User has been successfully created.", resultData);

            return Ok(response);
        }
    }
}
