using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using Serilog;

namespace Milvaneth.Server.Controllers
{
    [Route("api/[controller]")]
    [Produces("text/plain")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private IHttpContextAccessor _accessor;
        private ITimeService _time;
        private MilvanethDbContext _context;

        public DebugController(IHttpContextAccessor accessor, ITimeService time, MilvanethDbContext context)
        {
            _accessor = accessor;
            _time = time;
            _context = context;
        }

        [Route("database-connection-status")]
        [HttpGet]
        public ActionResult<string> DatabaseConnectionStatus()
        {
            try
            {
                var discard = _context.AccountData.AsNoTracking().Single(x => x.AccountId == 0);
                return "Database Connection Ready";
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error in DEBUG/DATABASE-CONNECTION-STATUS/");
                return "Database Connection Failed";
            }
        }
    }
}