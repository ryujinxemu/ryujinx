using Ryujinx.Horizon.Bcat.Types;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Bcat;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Bcat.Ipc
{
    partial class BcatService : IBcatService
    {
        public BcatService(BcatServicePermissionLevel permissionLevel) { }

        [CmifCommand(10100)]
        public Result RequestSyncDeliveryCache(out IDeliveryCacheProgressService deliveryCacheProgressService)
        {
            deliveryCacheProgressService = new DeliveryCacheProgressService();

            return Result.Success;
        }

        [CmifCommand(10101)]
        public Result RequestSyncDeliveryCacheWithDirectoryName(out IDeliveryCacheProgressService deliveryCacheProgressService, LibHac.Bcat.DirectoryName arg1)
        {
            // Just have the network request fail and pretend that everything is fine.
            deliveryCacheProgressService = new DeliveryCacheProgressService();

            return BcatResult.InternetRequestDenied;
        }
    }
}
