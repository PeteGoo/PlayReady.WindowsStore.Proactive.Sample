//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//// PARTICULAR PURPOSE.
////
//// Copyright (c) Microsoft Corporation. All rights reserved

using System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Foundation;
using Windows.Media.Protection;
using Microsoft.Media.PlayReadyClient;

namespace PlayReady_CS_RTM_Windows_Store_app
{

    public class Playback
    {
        protected MediaElement _mediaElement = null;
        protected string _strMediaPath = null;
        protected MediaProtectionManager _protectionManager = new MediaProtectionManager();
        
        const string _prGUID = "{F4637010-03C3-42CD-B932-B48ADF3A6A54}";
        MediaProtectionServiceCompletion _serviceCompletionNotifier = null;

        RequestChain _requestChain = null;
        ServiceRequestConfigData _requestConfigData = null;
            
        public ServiceRequestConfigData RequestConfigData  
        {  
            set { this._requestConfigData=  value; }  
            get { return this._requestConfigData; } 
        }
        
        void HookEventHandlers()
        {
            _mediaElement.CurrentStateChanged += new RoutedEventHandler( CurrentStateChanged );
            _mediaElement.MediaEnded += new RoutedEventHandler( MediaEnded );
            _mediaElement.MediaFailed += new ExceptionRoutedEventHandler( MediaFailed );
            _mediaElement.MediaOpened += new RoutedEventHandler( MediaOpened );
        }
        
        void UnhookEventHandlers()
        {
            _mediaElement.CurrentStateChanged -= new RoutedEventHandler( CurrentStateChanged );
            _mediaElement.MediaEnded -= new RoutedEventHandler( MediaEnded );
            _mediaElement.MediaFailed -= new ExceptionRoutedEventHandler( MediaFailed );
            _mediaElement.MediaOpened -= new RoutedEventHandler( MediaOpened );
        }

        void SetupProtectionManager()
        {
            _protectionManager.ComponentLoadFailed += new ComponentLoadFailedEventHandler(ProtectionManager_ComponentLoadFailed);
            _protectionManager.ServiceRequested += new ServiceRequestedEventHandler(ProtectionManager_ServiceRequested);

            TestLogger.LogMessage("Creating protection system mappings...");
            //Setup PlayReady as the ProtectionSystem to use
            //The native ASF media source will use this information to instantiate PlayReady ITA (InputTrustAuthority)
            Windows.Foundation.Collections.PropertySet cpSystems = new Windows.Foundation.Collections.PropertySet();
            cpSystems.Add("{F4637010-03C3-42CD-B932-B48ADF3A6A54}", "Microsoft.Media.PlayReadyClient.PlayReadyWinRTTrustedInput"); //Playready TrustedInput Class Name
            _protectionManager.Properties.Add("Windows.Media.Protection.MediaProtectionSystemIdMapping", cpSystems);
            _protectionManager.Properties.Add("Windows.Media.Protection.MediaProtectionSystemId", "{F4637010-03C3-42CD-B932-B48ADF3A6A54}");

            _mediaElement.ProtectionManager = _protectionManager;
        }

        public void Play(MediaElement mediaElement, string strMediaPath)
        {
            TestLogger.LogMessage("Enter Playback.Play()" );
            
            _mediaElement = mediaElement;
            _strMediaPath = strMediaPath;

            SetupProtectionManager();
            HookEventHandlers();
            _mediaElement.Source = new Uri( _strMediaPath );
            
            TestLogger.LogMessage("Leave Playback.Play()" );
        }

        protected void CurrentStateChanged( object sender, RoutedEventArgs e )
        {
            TestLogger.LogMessage("CurrentState:" + ((MediaElement)sender).CurrentState);
        }

        virtual protected void MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            UnhookEventHandlers();
            TestLogger.LogMessage("MediaFailed Source: " + ((MediaElement)sender).Source );
            TestLogger.LogMessage("Playback Failed");
            TestLogger.LogMessage("MediaFailed: " + e.ErrorMessage );
        }

        virtual protected void MediaEnded( object sender, RoutedEventArgs e )
        {
            UnhookEventHandlers();
            TestLogger.LogMessage("MediaEnded: " + ((MediaElement)sender).Source );
            TestLogger.LogImportantMessage("Playback succeeded");
        }

        virtual protected void MediaOpened( object sender, RoutedEventArgs e )
        {
            TestLogger.LogMessage("MediaOpened: " + ((MediaElement)sender).Source );
        }

        void ProtectionManager_ComponentLoadFailed( MediaProtectionManager sender, ComponentLoadFailedEventArgs e )
        {
            TestLogger.LogMessage("Enter Playback.ProtectionManager_ComponentLoadFailed()" );
            TestLogger.LogMessage( e.Information.ToString() );
            
            //  List the failing components - RevocationAndRenewalInformation
            for ( int i = 0; i < e.Information.Items.Count; i++ )
            {
                TestLogger.LogMessage(e.Information.Items[i].Name + "\nReasons=0x" + e.Information.Items[i].Reasons + "\n"
                                                    + "Renewal Id=" + e.Information.Items[i].RenewalId );

            }
            e.Completion.Complete( false );
            TestLogger.LogMessage("Leave Playback.ProtectionManager_ComponentLoadFailed()" );
        }

        void ProtectionManager_ServiceRequested( MediaProtectionManager sender, ServiceRequestedEventArgs srEvent )
        {
            TestLogger.LogMessage("Enter Playback.ProtectionManager_ServiceRequested()" );
            
            _serviceCompletionNotifier = srEvent.Completion;
            IPlayReadyServiceRequest serviceRequest = ( IPlayReadyServiceRequest )srEvent.Request;
            TestLogger.LogMessage("Servie request type = " + serviceRequest.GetType());

            _requestChain = new RequestChain( serviceRequest );
            _requestChain.RequestConfigData = this.RequestConfigData;
            _requestChain.FinishAndReportResult( new ReportResultDelegate(HandleServiceRequest_Finished));
            
            TestLogger.LogMessage("Leave Playback.ProtectionManager_ServiceRequested()" );
        }

        void HandleServiceRequest_Finished(bool bResult)
        {
            TestLogger.LogMessage("Enter Playback.HandleServiceRequest_Finished()" );
            
            TestLogger.LogMessage("MediaProtectionServiceCompletion.Complete = " + bResult.ToString() );
            _serviceCompletionNotifier.Complete( bResult );
            
            TestLogger.LogMessage("Leave Playback.HandleServiceRequest_Finished()" );
        }
        
    }

    public class PlaybackAndReportResult : Playback
    {
        ReportResultDelegate _reportResult = null;
        string _strExpectedError = null;
        
        public PlaybackAndReportResult( ReportResultDelegate callback, string strExpectedError = null )
        {
            _reportResult       = callback;
            _strExpectedError   = strExpectedError;
        }

        override protected void MediaEnded( object sender, RoutedEventArgs e )
        {
            TestLogger.LogMessage("Enter PlaybackAndReportResult.MediaEnded()" );
            
            base.MediaEnded(sender, e);
            _reportResult( true );
            
            TestLogger.LogMessage("Leave PlaybackAndReportResult.MediaEnded()" );
        }
        
        override protected void MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            TestLogger.LogMessage("Enter PlaybackAndReportResult.MediaFailed()" );
            
            base.MediaFailed(sender, e);
            
            bool bHandled = false;
            if( _strExpectedError != null )
            {
                if ( e.ErrorMessage.ToLower().Contains( _strExpectedError.ToLower() ) )
                {
                    TestLogger.LogMessage( "'" + e.ErrorMessage + "' Contains " + _strExpectedError + "  as expected" );
                    bHandled = true;
                }
            }
            _reportResult( bHandled );
            
            TestLogger.LogMessage("Leave PlaybackAndReportResult.MediaFailed()" );
        }
        
    }


}

