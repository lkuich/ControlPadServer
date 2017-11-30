using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Service;
using WindowsInput;
using WindowsInput.Native;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ControlHubServer
{
    class ConnectionHelpersImpl : ConnectionHelpers.ConnectionHelpersBase
    {
        public override Task<Response> VerifyStandardConnection(Response request, ServerCallContext context)
        {
            var test = request.Received;

            return base.VerifyStandardConnection(request, context);
        }

        public override Task<Response> VerifyXboxConnection(XboxButton request, ServerCallContext context)
        {


            return base.VerifyXboxConnection(request, context);
        }
    }
}
