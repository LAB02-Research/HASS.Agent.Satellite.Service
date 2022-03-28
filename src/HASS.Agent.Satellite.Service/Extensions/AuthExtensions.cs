using Serilog;

namespace HASS.Agent.Satellite.Service.Extensions
{
    public static class AuthExtensions
    {
        /// <summary>
        /// Checks whether the provided auth ID corresponds to the stored value (optionally, if any)
        /// </summary>
        /// <param name="authId"></param>
        /// <param name="caller"></param>
        /// <param name="emptyAllowed"></param>
        /// <returns></returns>
        public static bool CheckAuthId(this string authId, string caller, bool emptyAllowed = false)
        {
            var storedAuthId = Variables.ServiceSettings?.AuthId ?? string.Empty;

            switch (emptyAllowed)
            {
                case false when string.IsNullOrEmpty(storedAuthId):
                    Log.Warning("[AUTH] [{method}] Stored ID is empty, auth declined", caller);
                    return false;

                case true when string.IsNullOrEmpty(storedAuthId):
                    return true;
            }

            if (authId != storedAuthId)
            {
                Log.Warning("[RPC] [{method}] Invalid auth ID", caller);
                return false;
            }

            return true;
        }
    }
}
