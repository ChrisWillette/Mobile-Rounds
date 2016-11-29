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
using Windows.UI.Xaml;

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
        public ICommand Sync { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating if the admin tile should be visible or not.
        /// </summary>
        public Visibility AdminTileVisibility { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HomePageViewModel"/> class.
        /// Creates and sets defaults for the view model.
        /// </summary>
        public HomePageViewModel()
        {
#if DEBUG
            // only make us admin if debugging.
            this.IsAdmin = true;
#endif
            this.AdminTileVisibility = this.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            this.Sync = new SyncCommand();
            this.StartRound = new StartRoundCommand();
        }
    }
}
