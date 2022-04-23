using HASS.Agent.Shared.Models.HomeAssistant.Commands;
using HASS.Agent.Satellite.Service.Sensors;
using HASS.Agent.Shared.Enums;

namespace HASS.Agent.Satellite.Service.Models.HomeAssistant.Commands.InternalCommands
{
    internal class PublishAllSensorsCommand : InternalCommand
    {
        internal PublishAllSensorsCommand(string name = "PublishAllSensors", CommandEntityType entityType = CommandEntityType.Switch, string? id = default) : base(name, string.Empty, entityType, id)
        {
            State = "OFF";
        }

        public override void TurnOn()
        {
            State = "ON";

            SensorsManager.ResetAllSensorChecks();

            State = "OFF";
        }
    }
}