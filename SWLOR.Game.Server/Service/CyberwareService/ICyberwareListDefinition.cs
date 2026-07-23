using System.Collections.Generic;

namespace SWLOR.Game.Server.Service.CyberwareService
{
    /// <summary>
    /// Implemented by each file that defines cyberware. Discovered by reflection at load, exactly as
    /// <see cref="SpaceService.IShipModuleListDefinition"/> is for ship modules.
    /// </summary>
    public interface ICyberwareListDefinition
    {
        Dictionary<string, CyberwareDetail> BuildCyberware();
    }
}
