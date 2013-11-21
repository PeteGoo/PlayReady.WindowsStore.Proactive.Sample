//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//// PARTICULAR PURPOSE.
////
//// Copyright (c) Microsoft Corporation. All rights reserved

using System;
using Windows.Foundation;
using Microsoft.Media.PlayReadyClient;

namespace PlayReady_CS_RTM_Windows_Store_app
{
    public class RequestChain
    {
        protected IPlayReadyServiceRequest _serviceRequest = null;
        ReportResultDelegate _reportResult = null;

        IndivAndReportResult _indivAndReportResult  = null;
        LAAndReportResult _licenseAcquisition       = null;
        ServiceRequestConfigData _requestConfigData = null;
        public ServiceRequestConfigData RequestConfigData  
        {  
            set { this._requestConfigData=  value; }  
            get { return this._requestConfigData; } 
        }

        public RequestChain( IPlayReadyServiceRequest serviceRequest)
        {
            _serviceRequest = serviceRequest;
        }

        public void FinishAndReportResult(ReportResultDelegate callback)
        {
            _reportResult = callback;
            HandleServiceRequest();
        }
        
        void HandleServiceRequest()
        {
            if( _serviceRequest is PlayReadyIndividualizationServiceRequest )
            {
                HandleIndivServiceRequest( (PlayReadyIndividualizationServiceRequest)_serviceRequest);
            }
            else if ( _serviceRequest is PlayReadyLicenseAcquisitionServiceRequest )
            {
                HandleLicenseAcquisitionServiceRequest((PlayReadyLicenseAcquisitionServiceRequest)_serviceRequest);
            }
            else
            {
                TestLogger.LogError("ERROR: Unsupported serviceRequest " + _serviceRequest.GetType() );
            }
        }
        
        void HandleServiceRequest_Finished(bool bResult)
        {
            TestLogger.LogMessage("Enter RequestChain.HandleServiceRequest_Finished()" );
            
            _reportResult( bResult );
            
            TestLogger.LogMessage("Leave RequestChain.HandleServiceRequest_Finished()" );
        }

        void HandleIndivServiceRequest(PlayReadyIndividualizationServiceRequest serviceRequest)
        {
            TestLogger.LogMessage(" " );
            TestLogger.LogMessage("Enter RequestChain.HandleIndivServiceRequest()" );
            
            _indivAndReportResult = new IndivAndReportResult( new ReportResultDelegate(HandleServiceRequest_Finished));
            _indivAndReportResult.RequestConfigData = _requestConfigData;
            _indivAndReportResult.IndivReactively( serviceRequest );
            
            TestLogger.LogMessage("Leave RequestChain.HandleIndivServiceRequest()" );
        }

        void HandleLicenseAcquisitionServiceRequest(PlayReadyLicenseAcquisitionServiceRequest serviceRequest)
        {
            TestLogger.LogMessage(" " );
            TestLogger.LogMessage("Enter RequestChain.HandleLicenseAcquisitionServiceRequest()" );
            
            _licenseAcquisition = new LAAndReportResult( new ReportResultDelegate(HandleServiceRequest_Finished));
            _licenseAcquisition.RequestConfigData = _requestConfigData;
            _licenseAcquisition.AcquireLicenseReactively( serviceRequest);

            TestLogger.LogMessage("Leave RequestChain.HandleLicenseAcquisitionServiceRequest()" );
        }
    }
}
