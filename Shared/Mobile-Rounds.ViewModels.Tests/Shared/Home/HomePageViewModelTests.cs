﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mobile_Rounds.ViewModels.Platform;
using Mobile_Rounds.ViewModels.Shared.Commands;
using Mobile_Rounds.ViewModels.Shared.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobile_Rounds.ViewModels.Tests.Shared.Home
{
    class TestSettings : ISettings
    {
        private object val;
        public TestSettings(object value)
        {
            val = value;
        }
        public TReturn GetValue<TReturn>(string key)
        {
            return (TReturn)val;
        }

        public void SaveValue(string key, object value)
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class HomePageViewModelTests
    {
        [TestInitialize]
        public void Start()
        {
            //register dummy api request type.
            ServiceResolver.Register<IApiRequest>(() => null);
            ServiceResolver.Register<ISettings>(() => new TestSettings(true));
        }

        [TestMethod]
        public void ConstructorSetsCorrectDefaults()
        {
            var vm = new HomePageViewModel();
            Assert.IsNotNull(vm.StartRound);
            Assert.IsNotNull(vm.Sync);
            Assert.IsFalse(vm.IsLoading);
        }

        [TestMethod]
        public void ConstructorSetsCorrectCommandTypes()
        {
            ServiceResolver.Register<IApiRequest>(() => null);
            var vm = new HomePageViewModel();

            Assert.IsInstanceOfType(vm.StartRound, typeof(StartRoundCommand));
            Assert.IsInstanceOfType(vm.Sync, typeof(AsyncCommand));
        }
    }
}
