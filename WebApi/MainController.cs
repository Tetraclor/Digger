using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi
{
    [Route("api")]
    [ApiController]
    public class MainController : ControllerBase
    {
        [HttpGet]
        [Route("test")]
        public string Test()
        {
            return "Ok Test";
        }
    }
}
