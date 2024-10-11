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
        public static IResponse Create(ResponseType responseType, string title)
        {
            switch(responseType)
            {
                case ResponseType.Success:
                    return new SuccessResponseDto() { Title = title };
                case ResponseType.BadRequest:
                    return new Error400ResponseDto() { Title = title };
                case ResponseType.Unauthorized:
                    return new Error401ResponseDto() { Title = title };
                case ResponseType.NotFound:
                    return new Error404ResponseDto() { Title = title };
                case ResponseType.Conflict:
                    return new Error409ResponseDto() { Title = title };
                case ResponseType.InternalServerError:
                    return new Error500ResponseDto() { Title = title };
                default:
                    throw new NotSupportedException();
            }
        }

        public static IResponse Create(ResponseType responseType, string title, Dictionary<string, IEnumerable<string>> errors)
        {
            switch (responseType)
            {
                case ResponseType.BadRequest:
                    return new Error400ResponseWithErrorsDto() { Title = title };
                case ResponseType.Unauthorized:
                    return new Error401ResponseWithErrorsDto() { Title = title };
                case ResponseType.NotFound:
                    return new Error404ResponseWithErrorsDto() { Title = title };
                case ResponseType.Conflict:
                    return new Error409ResponseWithErrorsDto() { Title = title };
                case ResponseType.InternalServerError:
                    return new Error500ResponseWithErrorsDto() { Title = title };
                default:
                    throw new NotSupportedException();
            }
        }

        public static IResponse Create<T>(ResponseType responseType, Dictionary<string, T> data)
        {
            switch (responseType)
            {
                case ResponseType.NotFound:
                    return new SuccessResponseWithResultDataDto<T>() { ResultData = data };
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
