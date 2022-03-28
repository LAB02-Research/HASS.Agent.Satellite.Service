using HASS.Agent.Shared.Models.HomeAssistant.Commands;
using HASS.Agent.Satellite.Service.Sensors;

namespace HASS.Agent.Satellite.Service.Models.HomeAssistant.Commands.InternalCommands
{
    internal class PublishAllSensorsCommand : InternalCommand
    {
        internal PublishAllSensorsCommand(string name = "PublishAllSensors", string? id = default) : base(name, "", id)
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