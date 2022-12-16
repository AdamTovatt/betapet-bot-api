using Betapet.Models.Communication.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.Communication
{
    public class RequestResponse
    {
        public bool Success { get; set; }
        public BetapetResponse? InnerResponse { get; set; }

        public RequestResponse(bool success, BetapetResponse innerResponse)
        {
            Success = success;
            InnerResponse = innerResponse;
        }

        public RequestResponse(bool success)
        {
            this.Success = success;
        }
    }
}
