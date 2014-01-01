using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.Windows;
using Microsoft.Phone.Info;
using Microsoft.Phone.Net.NetworkInformation;
using System.Globalization;

namespace NewsAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        public static string Locale;
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru")
                {
                    Locale = "ru";
                }
                else
                    Locale = "en";
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        protected override void OnInvoke(ScheduledTask task)
        {
            if (DeviceNetworkInformation.IsNetworkAvailable)
            {
                VelesConflictReporting.ReportingServiceClient client = new VelesConflictReporting.ReportingServiceClient();
                client.GetLatestNewsCompleted += new EventHandler<VelesConflictReporting.GetLatestNewsCompletedEventArgs>(client_GetLatestNewsCompleted);
                client.GetLatestNewsAsync(Locale);
            }
            else
            {
                NotifyComplete();
            }
        }

        void client_GetLatestNewsCompleted(object sender, VelesConflictReporting.GetLatestNewsCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault();
                if (tile != null)
                {
                    StandardTileData std = new StandardTileData();
                    std.BackBackgroundImage = new Uri("/Background.png", UriKind.Relative);
                    std.BackTitle = "Veles News";
                    std.BackContent = e.Result;
                    std.Title = " ";
                    tile.Update(std);
                }
            }
            NotifyComplete();
        }
    }
}