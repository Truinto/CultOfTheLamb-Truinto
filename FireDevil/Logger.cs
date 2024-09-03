using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;
using BepInEx;
using BepInEx.Logging;

namespace FireDevil
{
    public class Logger_UMM : ILogger
    {
        private UnityModManager.ModEntry.ModLogger logger;

        public Logger_UMM(UnityModManager.ModEntry.ModLogger logger)
        {
            this.logger = logger;
        }

        public void Log(string message)
        {
            logger.Log(message);
        }
    }

    public class Logger_Bep : ILogger
    {
        private ManualLogSource logger;

        public Logger_Bep(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public void Log(string message)
        {
            logger.Log(LogLevel.Debug, message);
        }
    }

    public interface ILogger
    {
        public abstract void Log(string message);
    }
}
