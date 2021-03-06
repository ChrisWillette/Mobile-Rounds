﻿using Mobile_Rounds.ViewModels.Models;
using Mobile_Rounds.ViewModels.Platform;
using Mobile_Rounds.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobile_Rounds.ViewModels.Shared
{
    /// <summary>
    /// A singleton class for operating on the current <see cref="RoundModel"/>.
    /// </summary>
    public static class RoundManager
    {
        private static RoundModel Singleton { get; set; }

        static RoundManager()
        {
           FileHandler = ServiceResolver.Resolve<IFileHandler>();
        }

        /// <summary>
        /// Returns the current round.
        /// </summary>
        public static RoundModel CurrentRound { get { return Singleton; } }

        /// <summary>
        /// Loads the current round from the file system. If none exists, returns null.
        /// </summary>
        public static async Task<RoundModel> LoadCurrentRoundAsync()
        {
            try
            {
                var currentRound = await FileHandler.GetFileAsync<RoundModel>(Constants.FileNames.CurrentRound);
                if (currentRound != null)
                {
                    Singleton = currentRound;
                }
                return Singleton;
            }
            catch
            {
                Singleton = null;
            }
            return Singleton;
        }

        /// <summary>
        /// Starts a new round. Populates the basic round info based on the settings and time.
        /// </summary>
        /// <returns>The newly started round.</returns>
        public static RoundModel StartNewRound()
        {
            if (Singleton != null) return null;
            var settings = ServiceResolver.Resolve<ISettings>();

            return Singleton = new RoundModel()
            {
                StartTime = DateTime.UtcNow,
                AssignedTo = settings.GetValue<string>(Constants.UserDomainName)
            };
        }

        /// <summary>
        /// Uploads the round to the server and clears the current round. Returns null if it could not upload the round correctly.
        /// </summary>
        public static async Task<RoundModel> CompleteRoundAsync()
        {
            //Upload the current round to the server.
            var request = ServiceResolver.Resolve<IApiRequest>();

            // Note the time that the round is ending.
            CurrentRound.EndTime = DateTime.UtcNow;

            //upload the round to the server.
            var completedRound = await request.PostAsync<RoundModel>(
                    Constants.Endpoints.Rounds, CurrentRound);

            if(completedRound?.Id != Guid.Empty)
            {
                //TODO: Modify the logic here to NOT complete the round if we could not delete
                //the file.

                //means the upload was a success, so continue with the deletion of the round.
                if(await DeleteCurrentRoundAsync())
                {
                    return completedRound;
                }
            }

            //upload to the server failed, so return null.
            return null;
        }

        /// <summary>
        /// Deletes the currently stored round. This is a permenent thing, so make sure it
        /// is REALLY safe to delete.
        /// </summary>
        private static async Task<bool> DeleteCurrentRoundAsync()
        {
            var success = await FileHandler.DeleteFileAsync(Constants.FileNames.CurrentRound);

            Singleton = null;
            return success;
        }

        /// <summary>
        /// Deletes the current round and redirects to the home screen without uploading anything.
        /// This gets invoked if the user has exceeded the time window for completing a round.
        /// </summary>
        /// <returns></returns>
        public static async Task CancelRound()
        {
            await DeleteCurrentRoundAsync();
            BaseViewModel.IsRoundLocked = false;
        }

        /// <summary>
        /// Saves the current round to disk. Should be called whenever you want to ensure
        /// that it persists per app instance.
        /// </summary>
        public static async Task SaveRoundToDiskAsync()
        {
            if(Singleton == null)
            {
                throw new InvalidOperationException("No round to save.");
            }

            await FileHandler.SaveFileAsync(Constants.FileNames.CurrentRound, Singleton);
        }

        /// <summary>
        /// Determines if the current round needs to be timed out due to time constraints.
        /// </summary>
        /// <returns></returns>
        public static async Task CheckTimeout()
        {
            var currentDate = DateTime.Now;
            var hourDiff = Math.Abs(currentDate.Hour - CurrentRound.RoundHour);

            if (hourDiff > 3)
            {
                BaseViewModel.IsRoundLocked = true;
            }
            else if (hourDiff == 3)
            {
                var temp = currentDate.AddHours(-3);
                if (temp.Minute > 0)
                {
                    BaseViewModel.IsRoundLocked = true;
                }
            }
        }

        private static IFileHandler FileHandler;
    }
}
