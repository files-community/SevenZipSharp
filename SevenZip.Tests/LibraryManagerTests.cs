namespace SevenZip.Tests
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using NUnit.Framework;

    [TestFixture]
    [NonParallelizable]
    public class LibraryManagerTests : TestBase
    {
        [Test]
        public void SetNonExistant7zDllLocationTest()
        {
            Assert.Throws<SevenZipLibraryException>(() => SevenZipLibraryManager.SetLibraryPath("null"));
        }

        [Test]
        public void CurrentLibraryFeaturesTest()
        {
            //var features = SevenZipBase.CurrentLibraryFeatures;

            // Exercising more code paths...
            //features = SevenZipLibraryManager.CurrentLibraryFeatures;

            //Assert.IsTrue(features.HasFlag(LibraryFeature.ExtractAll));
            //Assert.IsTrue(features.HasFlag(LibraryFeature.CompressAll));
            //Assert.IsTrue(features.HasFlag(LibraryFeature.Modify));
        }

        [Test]
        public void CreateInstanceReleasesFactoryReference()
        {
            var libraryPath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                Environment.Is64BitProcess ? "7z64.dll" : "7z.dll");
            var module = NativeMethods.LoadLibrary(libraryPath);
            Assert.That(module, Is.Not.EqualTo(IntPtr.Zero));

            IInArchive archive = null;
            var unknown = IntPtr.Zero;

            try
            {
                archive = ComInterop.CreateInstance<IInArchive>(
                    module,
                    Formats.InFormatGuids[InArchiveFormat.SevenZip]);

                Assert.That(ComWrappers.TryGetComInstance(archive, out unknown), Is.True);

                ComInterop.Release(archive);
                archive = null;

                var referenceCount = Marshal.AddRef(unknown);
                try
                {
                    Assert.That(referenceCount, Is.EqualTo(2));
                }
                finally
                {
                    Marshal.Release(unknown);
                }
            }
            finally
            {
                if (archive != null)
                {
                    ComInterop.Release(archive);
                }

                if (unknown != IntPtr.Zero)
                {
                    Marshal.Release(unknown);
                }

                NativeMethods.FreeLibrary(module);
            }
        }

        [Test]
        public void PropVariantClearResetsOwnedBstr()
        {
            var value = new PropVariant
            {
                VarType = VarEnum.VT_BSTR,
                Value = Marshal.StringToBSTR("value")
            };

            value.Clear();

            Assert.That(value.Value, Is.EqualTo(IntPtr.Zero));
            Assert.That(value.Object, Is.Null);
        }

        [Test]
        public void FreeLibraryRemovesRegistrationWithoutCreatedArchive()
        {
            var originalPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "7z.dll");
            var alternatePath = Path.Combine(OutputDirectory, "7z-copy.dll");
            File.Copy(originalPath, alternatePath);

            var user = new object();
            SevenZipLibraryManager.SetLibraryPath(originalPath);

            try
            {
                SevenZipLibraryManager.LoadLibrary(user, InArchiveFormat.SevenZip);
                SevenZipLibraryManager.FreeLibrary(user, InArchiveFormat.SevenZip);

                Assert.DoesNotThrow(() => SevenZipLibraryManager.SetLibraryPath(alternatePath));
            }
            finally
            {
                SevenZipLibraryManager.FreeLibrary(user, InArchiveFormat.SevenZip);
                SevenZipLibraryManager.SetLibraryPath(originalPath);
            }
        }

        [Test]
        public void InvalidLibraryDoesNotLeaveStaleModuleHandle()
        {
            var originalPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "7z.dll");
            var invalidLibraryPath = Path.Combine(Environment.SystemDirectory, "kernel32.dll");
            var user = new object();

            SevenZipLibraryManager.SetLibraryPath(invalidLibraryPath);

            try
            {
                Assert.Throws<SevenZipLibraryException>(() =>
                    SevenZipLibraryManager.LoadLibrary(user, InArchiveFormat.SevenZip));
                Assert.DoesNotThrow(() => SevenZipLibraryManager.SetLibraryPath(originalPath));
            }
            finally
            {
                SevenZipLibraryManager.FreeLibrary(user, InArchiveFormat.SevenZip);
                SevenZipLibraryManager.SetLibraryPath(originalPath);
            }
        }

        [Test]
        public void SetLibraryPathInvalidatesModifyCapabilityCache()
        {
            var originalPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "7z.dll");
            var oldLibraryPath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "SevenZipSharp.dll");

            try
            {
                SevenZipLibraryManager.SetLibraryPath(oldLibraryPath);
                Assert.That(SevenZipLibraryManager.ModifyCapable, Is.False);

                SevenZipLibraryManager.SetLibraryPath(originalPath);
                Assert.That(SevenZipLibraryManager.ModifyCapable, Is.True);
            }
            finally
            {
                SevenZipLibraryManager.SetLibraryPath(originalPath);
            }
        }
    }
}
