﻿// /* ============================================================================
// Copyright 2014 Hewlett Packard
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ============================================================================ */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenStack.Common.ServiceLocation;
using OpenStack.Compute;
using OpenStack.Identity;

namespace OpenStack.Test.Compute
{
    [TestClass]
    public class ComputeServiceClientTests
    {
        internal TestComputeServicePocoClient ServicePocoClient;
        
        internal string authId = "12345";
        internal string endpoint = "http://testcomputeendpoint.com/v2/1234567890";
        internal ServiceLocator ServiceLocator;

        [TestInitialize]
        public void TestSetup()
        {
            this.ServicePocoClient = new TestComputeServicePocoClient();
            this.ServiceLocator = new ServiceLocator();
            
            var manager = this.ServiceLocator.Locate<IServiceLocationOverrideManager>();
            manager.RegisterServiceInstance(typeof(IComputeServicePocoClientFactory), new TestComputeServicePocoClientFactory(this.ServicePocoClient));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.ServicePocoClient = new TestComputeServicePocoClient();
            this.ServiceLocator = new ServiceLocator();
        }

        IOpenStackCredential GetValidCreds()
        {
            var catalog = new OpenStackServiceCatalog();
            catalog.Add(new OpenStackServiceDefinition("Nova", "Compute Service",
                new List<OpenStackServiceEndpoint>()
                {
                    new OpenStackServiceEndpoint(endpoint, string.Empty, "some version", "some version info", "1,2,3")
                }));

            var creds = new OpenStackCredential(new Uri(this.endpoint), "SomeUser", "Password", "SomeTenant");
            creds.SetAccessTokenId(this.authId);
            creds.SetServiceCatalog(catalog);
            return creds;
        }

        [TestMethod]
        public async Task CanListFlavors()
        {
            var flv1 = new ComputeFlavor("1", "m1.tiny", "512", "2", "10", new Uri("http://someuri.com/v2/flavors/1"),
                new Uri("http://someuri.com/flavors/1"));
            var flv2 = new ComputeFlavor("2", "m1.small", "1024", "4", "100", new Uri("http://someuri.com/v2/flavors/2"),
                new Uri("http://someuri.com/flavors/2"));
            var flavors = new List<ComputeFlavor>() {flv1, flv2};

            this.ServicePocoClient.GetFlavorsDelegate = () => Task.Factory.StartNew(() => (IEnumerable<ComputeFlavor>)flavors);

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var resp = await client.ListFlavors();
            Assert.IsNotNull(resp);

            var respFlavors = resp.ToList();
            Assert.AreEqual(2, respFlavors.Count());
            Assert.AreEqual(flv1, respFlavors[0]);
            Assert.AreEqual(flv2, respFlavors[1]);
        }

        [TestMethod]
        public async Task CanListImages()
        {
            var img1 = new ComputeImage("12345", "image1", new Uri("http://someuri.com/v2/images/12345"), new Uri("http://someuri.com/images/12345"),"active",DateTime.Now,DateTime.Now,10, 512, 100);
            var img2 = new ComputeImage("23456", "image2", new Uri("http://someuri.com/v2/images/23456"), new Uri("http://someuri.com/images/23456"), "active", DateTime.Now, DateTime.Now, 10, 512, 100);
            var images = new List<ComputeImage>() { img1, img2 };

            this.ServicePocoClient.GetImagesDelegate = () => Task.Factory.StartNew(() => (IEnumerable<ComputeImage>)images);

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var resp = await client.ListImages();
            Assert.IsNotNull(resp);

            var respImage = resp.ToList();
            Assert.AreEqual(2, respImage.Count());
            Assert.AreEqual(img1, respImage[0]);
            Assert.AreEqual(img2, respImage[1]);
        }

        [TestMethod]
        public async Task CanGetImage()
        {
            var img1 = new ComputeImage("12345", "image1", new Uri("http://someuri.com/v2/images/12345"), new Uri("http://someuri.com/images/12345"), "active", DateTime.Now, DateTime.Now, 10, 512, 100);

            this.ServicePocoClient.GetImageDelegate = (id) =>
            {
                Assert.AreEqual("12345", id);
                return Task.Factory.StartNew(() => img1);
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var image = await client.GetImage("12345");

            Assert.IsNotNull(image);
            Assert.AreEqual(img1, image);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetImageWithNullImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetImage(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetImageWithEmptyImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetImage(string.Empty);
        }

        [TestMethod]
        public async Task CanDeleteImage()
        {
            this.ServicePocoClient.DeleteImageDelegate = async (imageId) =>
            {
                await Task.Run(() => Assert.AreEqual(imageId, "12345"));
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteImage("12345");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteImageWithNullImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteImage(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteImageWithEmptyImageIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.DeleteImage(string.Empty);
        }

        [TestMethod]
        public async Task CanGetFlavor()
        {
            var expectedFlavor = new ComputeFlavor("1", "m1.tiny", "512", "2", "10", new Uri("http://someuri.com/v2/flavors/1"),
                new Uri("http://someuri.com/flavors/1"));

            this.ServicePocoClient.GetFlavorDelegate = (id) =>
            {
                Assert.AreEqual("1", id);
                return Task.Factory.StartNew(() => expectedFlavor);
            };

            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            var flavor = await client.GetFlavor("1");
            
            Assert.IsNotNull(flavor);
            Assert.AreEqual(expectedFlavor, flavor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetFlavorWithNullFlavorIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetFlavor(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetFlavorWithEmptyFlavorIdThrows()
        {
            var client = new ComputeServiceClient(GetValidCreds(), "Nova", CancellationToken.None, this.ServiceLocator);
            await client.GetFlavor(string.Empty);
        }
    }
}
