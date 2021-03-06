﻿using Backend.DataAccess.Abstractions;
using Backend.DataAccess.Repositories.DataSources;
using Backend.Schemas;
using Mobile_Rounds.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DataAccess.Repositories
{
    /// <summary>
    /// Represents a way to interface with Regions in the database.
    /// </summary>
    public sealed class RegionRepository 
        : AbstractRepository<RegionModel, Region>
    {
        /// <summary>
        /// Creates a new instance for working with database based regions.
        /// </summary>
        /// <param name="database">The database to use.</param>
        public RegionRepository(DatabaseContext database)
            : base (new RegionDataSource(database))
        {
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<RegionModel>> GetAsync(bool includeDeleted)
        {
            return await DataSource
                //Get the records in order
                .GetOrdered(includeDeleted)
                //convert records to view models 
                .Select(r => new RegionModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsDeleted = r.IsMarkedAsDeleted,
                    Stations = r.Stations
                        .Where(s => includeDeleted || s.IsMarkedAsDeleted == false)
                        .OrderBy(s => s.Name)
                        .Select(s => new StationModel
                        {
                            Id = s.Id,
                            Name = s.Name,
                            IsDeleted = s.IsMarkedAsDeleted,
                            RegionId = s.RegionId,
                            ItemCount = s.Items.Count
                        }).ToList()
                })
                //load the data
                .ToListAsync();

        }

        /// <inheritdoc />
        public override async Task<RegionModel> InsertAsync(RegionModel toCreate)
        {
            var model = BuildModel(toCreate);
            var result = await DataSource.InsertAsync(model);
            return BuildViewModel(result);
        }

        /// <inheritdoc />
        public override async Task<RegionModel> UpdateAsync(RegionModel toUpdate)
        {
            var model = BuildModel(toUpdate);

            var result = await DataSource.UpdateAsync(model);
            return BuildViewModel(result);
        }

        protected override RegionModel BuildViewModel(Region model)
        {
            if (model == null) return null;
            return new RegionModel
            {
                Id = model.Id,
                Name = model.Name,
                IsDeleted = model.IsMarkedAsDeleted
            };
        }

        protected override Region BuildModel(RegionModel model)
        {
            if (model == null) return null;
            return new Region
            {
                Id = model.Id,
                Name = model.Name,
                IsMarkedAsDeleted = model.IsDeleted
            };
        }
    }
}
