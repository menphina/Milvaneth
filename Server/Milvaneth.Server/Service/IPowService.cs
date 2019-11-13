using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvaneth.Server.Service
{
    public interface IPowService
    {
        int Difficulty { get; set; }
        bool Verify(byte[] proofOfWork);
        byte[] Generate(byte difficulty);
        bool ConditionalGenerate(out byte[] requirement);
    }
}
