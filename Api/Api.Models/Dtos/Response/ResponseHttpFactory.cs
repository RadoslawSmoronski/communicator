using Api.Models.Dtos.Responses.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Responses
{
    public class ResponseHttpFactory
    {
        public IResponse Create(ResponseHttpType responseType, string title)
        {
            switch(responseType)
            {
                case ResponseHttpType.Success:
                    return new SuccessResponseDto() { Title = title };
                case ResponseHttpType.BadRequest:
                    return new Error400ResponseDto() { Title = title };
                case ResponseHttpType.Unauthorized:
                    return new Error401ResponseDto() { Title = title };
                case ResponseHttpType.NotFound:
                    return new Error404ResponseDto() { Title = title };
                case ResponseHttpType.Conflict:
                    return new Error409ResponseDto() { Title = title };
                case ResponseHttpType.InternalServerError:
                    return new Error500ResponseDto() { Title = title };
                default:
                    throw new NotSupportedException();
            }
        }

        public IResponse Create(ResponseHttpType responseType, string title, Dictionary<string, IEnumerable<string>> errors)
        {
            switch (responseType)
            {
                case ResponseHttpType.BadRequest:
                    return new Error400ResponseWithErrorsDto() { Title = title, Errors = errors};
                case ResponseHttpType.Unauthorized:
                    return new Error401ResponseWithErrorsDto() { Title = title, Errors = errors };
                case ResponseHttpType.NotFound:
                    return new Error404ResponseWithErrorsDto() { Title = title, Errors = errors };
                case ResponseHttpType.Conflict:
                    return new Error409ResponseWithErrorsDto() { Title = title, Errors = errors };
                case ResponseHttpType.InternalServerError:
                    return new Error500ResponseWithErrorsDto() { Title = title, Errors = errors };
                default:
                    throw new NotSupportedException();
            }
        }

        public IResponse Create<T>(ResponseHttpType responseType, string title, T data)
        {
            switch (responseType)
            {
                case ResponseHttpType.NotFound:
                    return new SuccessResponseWithResultDataDto<T>() { Title = title, ResultData = data };
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
