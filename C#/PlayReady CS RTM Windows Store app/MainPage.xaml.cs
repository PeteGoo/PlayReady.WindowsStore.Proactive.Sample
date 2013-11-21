//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//// PARTICULAR PURPOSE.
////
//// Copyright (c) Microsoft Corporation. All rights reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PlayReady_CS_RTM_Windows_Store_app;

//Simplifies adding PlayReady API calls
using Microsoft.Media.PlayReadyClient;

//Simplifies call to the MediaProtectionManager
using Windows.Media.Protection;
using Windows.Media;

//Interacting with the XAML elements needs to be done on a separate thread
using Windows.UI.Core;

namespace PlayReady_CS_RTM_Windows_Store_app
{

    /// <summary>
    /// PlayReady Sample Application for Windows 8 RTM in CSharp
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {

        //Global Variables
        static public string g_LAURL = "http://playready.directtaps.net/pr/svc/rightsmanager.asmx?PlayRight=1&FirstPlayExpiration=60";
        static public string g_pyvURL = "http://playready.directtaps.net/win/media/TallShip_with_Credits_folk_rock_soundtrack_encrypted.wmv";
        static public string g_ismURL = "http://playready.directtaps.net/smoothstreaming/SSWSS720H264PR/SuperSpeedway_720.ism/Manifest";

        public static CoreDispatcher _dispatcher;

        private MediaExtensionManager extensions = new Windows.Media.MediaExtensionManager();

        public MainPage()
        {
            this.InitializeComponent();
            //Create the dispatcher to be used when updating the XAML Text box
            _dispatcher = Window.Current.Dispatcher;

            GlobalData.g_mainPage = this;
            setupExtensionManager();
        }

        private void setupExtensionManager()
        {
            //Register ByteStreamHandler for PIFF
            //This can be removed if you don't play PIFF content
            //SmoothByteStreamHandler requires Microsoft Smooth Streaming SDK
            extensions.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "text/xml");
            extensions.RegisterByteStreamHandler("Microsoft.Media.AdaptiveStreaming.SmoothByteStreamHandler", ".ism", "application/vnd.ms-sstr+xml");

            //Register ByteStreamHander for pyv/pya content
            //This can be removed if you don't play pyv/pya content 
            extensions.RegisterByteStreamHandler("Microsoft.Media.PlayReadyClient.PlayReadyByteStreamHandler", ".pyv", "");
            extensions.RegisterByteStreamHandler("Microsoft.Media.PlayReadyClient.PlayReadyByteStreamHandler", ".pya", "");

            
        }

        private void log(string msg) { TestLogger.LogMessage(msg); }

        public async void LogMessage(String msg)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txtMessages.Text += msg + Environment.NewLine; });
        }

        public async void LogError(String msg)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { txtMessages.Text += msg + Environment.NewLine; });
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private Playback playMedia = null;

        private void btnPlayPyv_Click(object sender, RoutedEventArgs e)
        {
            playMedia = new Playback();
            playMedia.Play(myME, g_pyvURL);
        }

        private void btnPlayISM_Click(object sender, RoutedEventArgs e)
        {
            playMedia = new Playback();
            playMedia.Play(myME, g_ismURL);
        }

        private void btnDomainJoin_Click(object sender, RoutedEventArgs e)
        {
            DomainJoin domainJoin = new DomainJoin();
            domainJoin.DomainJoinProactively();
        }

        private void btnDomainLeave_Click(object sender, RoutedEventArgs e)
        {
            DomainLeave domainLeave = new DomainLeave();
            domainLeave.DomainLeaveProactively();
        }

        private void btnProactiveLA_Click(object sender, RoutedEventArgs e)
        {
            ServiceRequestConfigData requestConfigData = new ServiceRequestConfigData();

            //requestConfigData.KeyIdString = "7987aj+eE0uBlXi0N73AQw==";
            requestConfigData.KeyIdString = "GJsTKYmLVE+hXhYQKSqXAQ=="; // Use the same content id as the actual encoded tall ships video
            requestConfigData.EncryptionAlgorithm = PlayReadyEncryptionAlgorithm.Aes128Ctr;
            requestConfigData.Uri = new Uri("http://playready.directtaps.net/pr/svc/rightsmanager.asmx");
            requestConfigData.DomainServiceId = new Guid("{DEB47F00-8A3B-416D-9B1E-5D55FD023044}");

            LicenseAcquisition licenseAcquisition = new LicenseAcquisition();
            licenseAcquisition.RequestConfigData = requestConfigData;
            licenseAcquisition.AcquireLicenseProactively();
        }

        private async void btnPlayLocal_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".wmv");
            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }
            
            IRandomAccessStream ras = await file.OpenAsync(FileAccessMode.Read);
            //myME.SetSource(ras, file.ContentType);
            
            playMedia = new Playback();
            playMedia.Play(myME, ras, file.ContentType);
            
            
        }
    }

    public class GlobalData
    {
        public static MainPage g_mainPage = null;
    }

    public class TestLogger
    {

        public static void LogMessage(String message)
        {
            if (GlobalData.g_mainPage != null)
            {
                GlobalData.g_mainPage.LogMessage(message);
            }
        }

        public static void LogImportantMessage(String message)
        {
            LogMessage("*****************************************" + message + "**********************************");
        }

        public static void LogError(String message)
        {
            if (GlobalData.g_mainPage != null)
            {
                GlobalData.g_mainPage.LogError(message);
            }
        }
    }
}
