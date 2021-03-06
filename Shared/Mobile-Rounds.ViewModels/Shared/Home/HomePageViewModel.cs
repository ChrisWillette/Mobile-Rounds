﻿// <copyright file="HomePageViewModel.cs" company="SolarWorld Capstone Team">
// Copyright (c) SolarWorld Capstone Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Mobile_Rounds.ViewModels.Shared.Commands;
using Mobile_Rounds.ViewModels.Platform;
using Mobile_Rounds.ViewModels.Models;
using Newtonsoft.Json;
using Mobile_Rounds.ViewModels.Shared.DbModels;

namespace Mobile_Rounds.ViewModels.Shared.Home
{
    /// <summary>
    /// Represents the bsaic data for the home screens as exposed to XAML.
    /// </summary>
    public class HomePageViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets the property used to handle the start of rounds.
        /// </summary>
        public ICommand StartRound { get; private set; }

        /// <summary>
        /// Gets the property used to handle the syncing of data.
        /// </summary>
        public AsyncCommand Sync { get; private set; }

        public bool HasConfiguration
        {
            get
            {
                var settings = ServiceResolver.Resolve<ISettings>();
                return !string.IsNullOrEmpty(settings.GetValue<string>(Constants.APIHostConfigKey));
            }
        }

        public bool IsSyncing
        {
            get
            {
                return this.isSyncing;
            }

            set
            {
                this.isSyncing = value;
                this.RaisePropertyChanged(nameof(this.IsSyncing));
                this.RaisePropertyChanged(nameof(this.CanProgress));
                this.RaisePropertyChanged(nameof(this.CanGoToSettings));
                this.Sync.RaiseExecuteChanged();
            }
        }

        public bool CanGoToSettings { get { return !this.IsSyncing; } }

        public bool CanProgress { get { return HasConfiguration && !this.IsSyncing; } }


        /// <summary>
        /// Initializes a new instance of the <see cref="HomePageViewModel"/> class.
        /// Creates and sets defaults for the view model.
        /// </summary>
        public HomePageViewModel()
        {
            this.GoHome = null;

            this.Sync = new AsyncCommand(async (obj) =>
            {
                this.IsSyncing = true;
                IApiRequest request = ServiceResolver.Resolve<IApiRequest>();
                var handler = ServiceResolver.Resolve<IFileHandler>();

                var regions = await request.GetAsync<List<RegionModel>>(
                    $"{Constants.Endpoints.Regions}?{Constants.ApiOptions.ExcludeDeleted}");
                var regionResult = new RegionHandler() { Regions = regions };
                await handler.SaveFileAsync(Constants.FileNames.Regions, regionResult);

                var stations = await request.GetAsync<List<StationModel>>(
                    $"{Constants.Endpoints.Stations}?{Constants.ApiOptions.ExcludeDeleted}");

                foreach (var station in stations)
                {
                    var stationItems = await request.GetAsync<List<ItemModel>>(
                        $"{Constants.Endpoints.Stations}/{station.Id}/items?{Constants.ApiOptions.ExcludeDeleted}");
                    station.Items = stationItems;
                }

                var stationResult = new StationHandler() { Stations = stations };
                await handler.SaveFileAsync(Constants.FileNames.Stations, stationResult);

                var units = await request.GetAsync<List<UnitOfMeasureModel>>(
                    $"{Constants.Endpoints.Units}?{Constants.ApiOptions.ExcludeDeleted}");
                var unitResult = new UnitHandler() { Units = units };
                await handler.SaveFileAsync(Constants.FileNames.Units, unitResult);

                //upload the current round. This needs to hapen first, so we can set all RoundId values on the readings.
                //var round = await UploadRoundAsync();
                //if(round != null && round.Id != Guid.Empty)
                //{
                //    //update the RoundId on all the readings PRIOR to sending to the server.
                //    await UploadReadingsAsync(round.Id);
                //}
                this.IsSyncing = false;
            }, this.CanSync);

            this.StartRound = new StartRoundCommand();
            var settings = ServiceResolver.Resolve<ISettings>();
            IsAdmin = settings.GetValue<bool>(Constants.UserAdminKey);

#if DEBUG
            IsAdmin = true;
#endif
        }

        private bool CanSync(object sender)
        {
            //ensure that we have a valid configuration for the app.
            if (!this.HasConfiguration) return false;

            return !this.IsSyncing;
        }

        //private async Task<RoundModel> UploadRoundAsync()
        //{
        //    // Check if we have a round to save or not.
        //    if (RoundManager.CurrentRound != null)
        //    {
        //        //Tell the server that we completed the round.
        //        return await RoundManager.CompleteRoundAsync();

        //        //TODO: Do something with the didComplete. Perhaps this happens before the other syncs
        //        //so we can skip overwriting the existing files since the round couldn't be sync'ed.
        //    }

        //    return null;
        //}

        //private async Task UploadReadingsAsync(Guid roundId)
        //{
        //    foreach (var reading in ReadingManager.Readings)
        //    {
        //        //skip anything that was added, but not really entered.
        //        if (string.IsNullOrEmpty(reading.Value)) continue;

        //        //set the round id for all the readings.
        //        reading.RoundId = roundId;

        //        //for each reading that we have, upload it to the server.
        //        var readingResult = await this.Api.PostAsync<ReadingModel>(Constants.Endpoints.Readings, reading);

        //        //TODO: Figure out what to do if one of the uploads fails...
        //    }

        //    //all good, so remove the items.
        //    await ReadingManager.Reset();
        //}

        private bool isSyncing;
    }
}
