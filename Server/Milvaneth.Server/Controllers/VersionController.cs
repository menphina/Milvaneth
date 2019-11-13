using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milvaneth.Common;
using Milvaneth.Common.Communication.Version;
using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using Milvaneth.Server.Statics;
using Serilog;
using System;
using System.Linq;
using VersionDownload = Milvaneth.Common.Communication.Version.VersionDownload;

namespace Milvaneth.Server.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/x-msgpack")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        private ITimeService _time;
        private MilvanethDbContext _context;

        public VersionController(ITimeService time, MilvanethDbContext context)
        {
            _time = time;
            _context = context;
        }

        [Route("current/{versions}")]
        [HttpGet]
        public ActionResult<MilvanethProtocol> VersionCurrent(string versions)
        {
            var versionSplit = versions.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var versionSet = versionSplit.Select(x => int.TryParse(x, out var y) ? y : int.MinValue).ToArray();

            if (versionSet.Length != 3 || versionSet.Contains(int.MinValue))
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new VersionInfo
                    {
                        Message = GlobalMessage.DATA_INVALID_INPUT,
                        ReportTime = _time.SafeNow
                    }
                };
            }

            try
            {
                var gameMatches = _context.VersionData.AsNoTracking().Where(x => x.MilVersion == versionSet[0]).ToList();
                if (!gameMatches.Any())
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new VersionInfo
                        {
                            Message = GlobalMessage.ERR_DATA_ERROR,
                            ReportTime = _time.SafeNow
                        }
                    };
                }

                var candidates = gameMatches.OrderByDescending(x => x.DataVersion);
                var updateRecord = candidates.FirstOrDefault(x => x.DataVersion <= versionSet[1]);
                if (updateRecord == null)
                {
                    updateRecord = candidates.Last();
                }
                VersionData targetRecord;
                if (updateRecord.UpdateTo.HasValue)
                {
                    targetRecord = _context.VersionData.AsNoTracking().Single(x => x.VersionId == updateRecord.UpdateTo.Value);
                }
                else
                {
                    targetRecord = _context.VersionData.AsNoTracking().OrderByDescending(x => x.VersionId).First();
                }

                if (targetRecord.MilVersion != versionSet[0] || targetRecord.DataVersion != versionSet[1] ||
                    targetRecord.GameVersion != versionSet[2])
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new VersionInfo
                        {
                            Message = GlobalMessage.OK_UPDATE_AVAILABLE,
                            ReportTime = _time.SafeNow,
                            EligibleMilvanethVersion = targetRecord.MilVersion,
                            EligibleDataVersion = targetRecord.DataVersion,
                            EligibleGameVersion = targetRecord.GameVersion,
                            EligibleBundleKey = targetRecord.BundleKey,
                            ForceUpdate = updateRecord.ForceUpdate,
                            DisplayMessage = targetRecord.CustomMessage
                        }
                    };
                }

                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new VersionInfo
                    {
                        Message = GlobalMessage.OK_SUCCESS,
                        ReportTime = _time.SafeNow,
                    }
                };
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error in VERSION/CURRENT/{versions}");
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new VersionInfo
                    {
                        Message = GlobalMessage.OP_INVALID,
                        ReportTime = _time.SafeNow
                    }
                };
            }
        }

        [Route("download/{bundle}")]
        [HttpGet]
        public ActionResult<MilvanethProtocol> VersionDownload(string bundle)
        {
            try
            {
                var download = _context.VersionDownload.Single(x => x.BundleKey == bundle);

                if (download == null)
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new VersionDownload
                        {
                            Message = GlobalMessage.DATA_INVALID_INPUT,
                            ReportTime = _time.SafeNow
                        }
                    };
                }
                
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new VersionDownload
                    {
                        Message = GlobalMessage.OK_UPDATE_AVAILABLE,
                        ReportTime = _time.SafeNow,
                        Argument = download.Argument,
                        BinaryUpdate = download.BinaryUpdate,
                        DataUpdate = download.DataUpdate,
                        UpdaterUpdate = download.UpdaterUpdate,
                        FileServer = download.FileServer,
                        Files = download.Files
                    }
                };
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error in VERSION/DOWNLOAD/{bundle}");
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new VersionDownload
                    {
                        Message = GlobalMessage.OP_INVALID,
                        ReportTime = _time.SafeNow
                    }
                };
            }
        }
    }
}