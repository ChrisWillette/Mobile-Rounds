﻿using Mobile_Rounds.ViewModels.Models;
using Mobile_Rounds.ViewModels.Platform;
using Mobile_Rounds.ViewModels.Shared;
using Mobile_Rounds.ViewModels.Shared.Commands;
using Mobile_Rounds.ViewModels.Shared.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobile_Rounds.ViewModels.Admin.Regions
{
    public class RegionScreenViewModel : BaseViewModel
    {
        /// Gets or sets the list of regions that are displayed to the user.
        /// </summary>
        public ObservableCollection<RegionViewModel> Regions { get; set; }

        /// <summary>
        /// Gets the save method to call when the users taps save.
        /// </summary>
        public AsyncCommand Save { get; private set; }

        /// <summary>
        /// Gets or sets the currently selected region in the list.
        /// If set to null, it clears out the selection.
        /// </summary>
        public RegionViewModel Selected
        {
            get
            {
                return this.selected;
            }

            set
            {
                this.selected = value;
                if (this.selected != null)
                {
                    this.CurrentRegion = this.selected;
                }

                this.RaisePropertyChanged(nameof(this.Selected));
            }
        }

        /// <summary>
        /// Gets or sets the region that is currently being modified or added. Used
        /// for data binding to the input fields.
        /// </summary>
        public RegionViewModel CurrentRegion
        {
            get
            {
                return this.currentRegion;
            }

            set
            {
                if (this.currentRegion == null && value != null)
                {
                    this.currentRegion = value;
                }

                this.currentRegion.Id = value.Id;
                this.currentRegion.Name = value.Name;
                this.currentRegion.IsDeleted = value.IsDeleted;

                if (this.currentRegion.Id == Guid.Empty)
                {
                    this.currentRegion.SetModificationType(ModificationType.Create);
                }
                else
                {
                    this.currentRegion.SetModificationType(ModificationType.Update);
                }

                this.RaisePropertyChanged(nameof(this.CurrentRegion));
            }
        }

        public bool IsNameFieldValid
        {
            get
            {
                return isNameFieldValid;
            }
            set
            {
                isNameFieldValid = value;
                RaisePropertyChanged(nameof(IsNameFieldValid));
            }
        }

        /// <summary>
        /// Gets the command to call when the user taps the cancel button.
        /// </summary>
        public AsyncCommand Cancel { get; private set; }

        protected override async Task FetchDataAsync()
        {
            var regions = await base.Api.GetAsync<List<RegionModel>>(
                $"{Constants.Endpoints.Regions}?{Constants.ApiOptions.IncludeDeleted}");

            var casted = regions.Select(r => new RegionViewModel(r, Save, Cancel));
            this.Regions.AddRange(casted);

            var selectedRegionId = Navigator.GetNavigationData<Guid>();
            if (selectedRegionId != null && selectedRegionId != Guid.Empty)
            {
                this.Selected = Regions.FirstOrDefault(s => s.Id == selectedRegionId);
            }
        }

        public RegionScreenViewModel()
        {
            this.Cancel = new AsyncCommand(
                (obj) =>
                {
                    this.Selected = null;
                    this.CurrentRegion = new RegionViewModel(this.Save, this.Cancel);
                    this.IsNameFieldValid = true;
                }, this.CanCancel);

            this.Save = new AsyncCommand(
                async (obj) =>
                {
                    var model = new RegionModel()
                    {
                        Id = this.currentRegion.Id,
                        Name = this.currentRegion.Name,
                        IsDeleted = this.currentRegion.IsDeleted
                    };

                    var existing = this.Regions.FirstOrDefault(u => u.Id == this.currentRegion.Id);
                    if (existing == null)
                    {
                        model = await base.Api.PostAsync<RegionModel>(Constants.Endpoints.Regions, model);
                        if(model == null)
                        {
                            //error saving, so show field error and return.
                            IsNameFieldValid = false;
                            return;
                        }
                        var newCopy = new RegionViewModel(model, Save, Cancel);
                        this.Regions.Add(newCopy);
                    }
                    else
                    {
                        model = await base.Api.PutAsync<RegionModel>(Constants.Endpoints.Regions, model);
                        if (model == null)
                        {
                            //error saving, so show field error and return.
                            IsNameFieldValid = false;
                            return;
                        }
                        existing.Id = model.Id;
                        existing.IsDeleted = model.IsDeleted;
                        existing.Name = model.Name;
                    }

                    this.CurrentRegion = new RegionViewModel(this.Save, this.Cancel);
                    this.Selected = null;
                    IsNameFieldValid = ValidateInput(this);
                }, this.ValidateInput);

            this.currentRegion = new RegionViewModel(this.Save, this.Cancel);
            this.Crumbs.Add(new BreadcrumbItemModel("Admin", this.GoToAdmin));
            this.Crumbs.Add(new BreadcrumbItemModel("Areas"));
            this.IsNameFieldValid = true;
            this.Regions = new ObservableCollection<RegionViewModel>();
        }

        private RegionViewModel currentRegion;
        private RegionViewModel selected;
        private bool isNameFieldValid;

        private bool ValidateNewInput(RegionViewModel toValidate)
        {
            if (string.IsNullOrEmpty(toValidate.Name))
            {
                // name is empty, so error
                return false;
            }

            //now validate that there is no duplicate name.
            foreach (var region in this.Regions)
            {
                if (region.Name.Equals(toValidate.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    //duplicate name, so error.
                    return false;
                }
            }

            //no errors, so valid name.
            return true;
        }

        private bool ValidateExistingInput(RegionViewModel current, RegionViewModel selected)
        {
            if (string.IsNullOrEmpty(current.Name))
            {
                // name is empty, so error
                return false;
            }

            //now validate that there is no duplicate name.
            foreach (var region in this.Regions)
            {
                //skip over self since that technically is not a dup.
                if (region.Id == selected.Id) continue;
                if (region.Name.Equals(current.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    //duplicate name, so error.
                    return false;
                }
            }

            //no errors, so valid name.
            return true;
        }

        private bool ValidateInput(object input)
        {
            //nothing selected, so must be a new entry
            if (this.selected == null)
            {
                //only valid if there is any text
                return IsNameFieldValid = ValidateNewInput(this.CurrentRegion);
            }

            return IsNameFieldValid = ValidateExistingInput(this.CurrentRegion, this.Selected);
        }

        private bool CanCancel(object input)
        {
            return !string.IsNullOrEmpty(this.currentRegion.Name);
        }
    }
}
