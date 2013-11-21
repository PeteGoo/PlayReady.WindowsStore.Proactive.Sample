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

    public class LicenseAcquisition : ServiceRequest
    {
        protected virtual void LAServiceRequestCompleted( PlayReadyLicenseAcquisitionServiceRequest  sender, Exception hrCompletionStatus )
        {
        }

        void HandleIndivServiceRequest_Finished(bool bResult)
        {
            TestLogger.LogMessage("Enter LicenseAcquisition.HandleIndivServiceRequest_Finished()");

            TestLogger.LogMessage("HandleIndivServiceRequest_Finished(): " + bResult.ToString());
            if (bResult)
            {
                AcquireLicenseProactively();
            }

            TestLogger.LogMessage("Leave LicenseAcquisition.HandleIndivServiceRequest_Finished()");
        }

        public void  AcquireLicenseProactively()
        {
            try
            {
                PlayReadyContentHeader contentHeader = new PlayReadyContentHeader(
                                                                                    RequestConfigData.KeyId,
                                                                                    RequestConfigData.KeyIdString,
                                                                                    RequestConfigData.EncryptionAlgorithm,
                                                                                    RequestConfigData.Uri,
                                                                                    RequestConfigData.Uri,
                                                                                    String.Empty,
                                                                                    RequestConfigData.DomainServiceId);

                TestLogger.LogMessage("Creating license acquisition service request...");
                PlayReadyLicenseAcquisitionServiceRequest licenseRequest = new PlayReadyLicenseAcquisitionServiceRequest();
                licenseRequest.ContentHeader = contentHeader;
                AcquireLicenseReactively(licenseRequest);
            }
            catch (Exception ex)
            {
                if (ex.HResult == ServiceRequest.MSPR_E_NEEDS_INDIVIDUALIZATION)
                {
                    PlayReadyIndividualizationServiceRequest indivServiceRequest = new PlayReadyIndividualizationServiceRequest();

                    RequestChain requestChain = new RequestChain(indivServiceRequest);
                    requestChain.FinishAndReportResult(new ReportResultDelegate(HandleIndivServiceRequest_Finished));
                }
                else
                {
                    TestLogger.LogMessage("DomainJoinProactively failed:" + ex.HResult);
                }
            }
            
        }

        static public void DumpContentHeaderValues(PlayReadyContentHeader contentHeader)
        {
            TestLogger.LogMessage(" " );
            TestLogger.LogMessage("Content header values:" );
            if( contentHeader == null )
            {
                return;
            }
            TestLogger.LogMessage("CustomAttributes :" + contentHeader.CustomAttributes );
            TestLogger.LogMessage("DecryptorSetup   :" + contentHeader.DecryptorSetup.ToString() );
            TestLogger.LogMessage("DomainServiceId  :" + contentHeader.DomainServiceId.ToString() );
            TestLogger.LogMessage("EncryptionType   :" + contentHeader.EncryptionType.ToString() );
            TestLogger.LogMessage("KeyId            :" + contentHeader.KeyId.ToString() );
            TestLogger.LogMessage("KeyIdString      :" + contentHeader.KeyIdString );
            TestLogger.LogMessage("LicenseAcquisitionUrl :" + contentHeader.LicenseAcquisitionUrl.ToString() );
        }

        void ConfigureServiceRequest()
        {
            PlayReadyLicenseAcquisitionServiceRequest licenseRequest = _serviceRequest as PlayReadyLicenseAcquisitionServiceRequest;
            
            DumpContentHeaderValues( licenseRequest.ContentHeader );
            
            TestLogger.LogMessage(" " );
            TestLogger.LogMessage("Configure license request to these values:" );

            licenseRequest.Uri = new Uri(MainPage.g_LAURL);

            TestLogger.LogMessage("ChallengeCustomData:" + "Custom Data" );
            licenseRequest.ChallengeCustomData = "Custom Data";

            
            TestLogger.LogMessage(" " );
        }
        
        async public void  AcquireLicenseReactively(PlayReadyLicenseAcquisitionServiceRequest licenseRequest)
        {
            TestLogger.LogMessage("Enter LicenseAcquisition.AcquireLicenseReactively()" );
            Exception exception = null;
            
            try
            {   
                _serviceRequest = licenseRequest;
                ConfigureServiceRequest();

                TestLogger.LogMessage("ChallengeCustomData = " + licenseRequest.ChallengeCustomData);
                if( RequestConfigData.ManualEnabling )
                {
                    TestLogger.LogMessage("Manually posting the request..." );
                    
                    HttpHelper httpHelper = new HttpHelper( licenseRequest );
                    await httpHelper.GenerateChallengeAndProcessResponse();
                }
                else
                {
                    TestLogger.LogMessage("Begin license acquisition service request..." );
                    await licenseRequest.BeginServiceRequest();
                }
            }
            catch( Exception ex )
            {
                TestLogger.LogMessage("Saving exception.." );
                exception = ex;
            }
            finally
            {
                TestLogger.LogMessage("Post-LicenseAcquisition Values:");
                TestLogger.LogMessage("DomainServiceId          = " + licenseRequest.DomainServiceId.ToString());
                if( exception == null )
                {
                    TestLogger.LogMessage("ResponseCustomData       = " + licenseRequest.ResponseCustomData);
                }
                DumpContentHeaderValues( licenseRequest.ContentHeader );
                
                LAServiceRequestCompleted( licenseRequest, exception );
            }
            
            TestLogger.LogMessage("Leave LicenseAcquisition.AcquireLicenseReactively()" );
        }
        
    }

    public class LAAndReportResult : LicenseAcquisition
    {
        ReportResultDelegate _reportResult = null;
        string _strExpectedError = null;
        
        public string ExpectedError  
        {  
            set { this._strExpectedError =  value; }  
            get { return this._strExpectedError; } 
        }
        
        public LAAndReportResult( ReportResultDelegate callback)
        {
            _reportResult = callback;
        }
        
        protected override void LAServiceRequestCompleted( PlayReadyLicenseAcquisitionServiceRequest  sender, Exception hrCompletionStatus )
        {
            TestLogger.LogMessage("Enter LAAndReportResult.LAServiceRequestCompleted()" );

            if( hrCompletionStatus == null )
            {
                TestLogger.LogMessage("************************************    License acquisition succeeded       ****************************************");
               _reportResult( true );
            }
            else
            {
                if( !PerformEnablingActionIfRequested(hrCompletionStatus) && !HandleExpectedError(hrCompletionStatus) )
                {
                    TestLogger.LogError( "LAServiceRequestCompleted ERROR: " + hrCompletionStatus.ToString() );
                   _reportResult( false );
                }
            }
                
            TestLogger.LogMessage("Leave LAAndReportResult.LAServiceRequestCompleted()" );
        }
        
        protected override void EnablingActionCompleted(bool bResult)
        {
            TestLogger.LogMessage("Enter LAAndReportResult.EnablingActionCompleted()" );

            _reportResult( bResult );
            
            TestLogger.LogMessage("Leave LAAndReportResult.EnablingActionCompleted()" );
        }

        protected override bool HandleExpectedError(Exception ex)
        {
            TestLogger.LogMessage("Enter LAAndReportResult.HandleExpectedError()" );
            
            if( string.IsNullOrEmpty( _strExpectedError ) )
            {
                TestLogger.LogMessage("Setting error code to " + RequestConfigData.ExpectedLAErrorCode );
                _strExpectedError = RequestConfigData.ExpectedLAErrorCode;
            }
            
            bool bHandled = false;
            if( _strExpectedError != null )
            {
                if ( ex.Message.ToLower().Contains( _strExpectedError.ToLower() ) )
                {
                    TestLogger.LogMessage( "'" + ex.Message + "' Contains " + _strExpectedError + "  as expected" );
                    bHandled = true;
                    _reportResult( true );
                }
            }
            
            TestLogger.LogMessage("Leave LAAndReportResult.HandleExpectedError()" );
            return bHandled;
        }
        
    }

}
