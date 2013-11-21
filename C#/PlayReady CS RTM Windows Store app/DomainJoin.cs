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
    public class DomainJoin : ServiceRequest
    {
        static Guid g_domainServiceID = new Guid("{DEB47F00-8A3B-416D-9B1E-5D55FD023044}");
        static Guid g_domainAccountID = new Guid("{3A87FB03-C53E-46F9-8CF8-9967CB6A1B14}");

        protected virtual void DomainJoinServiceRequestCompleted(PlayReadyDomainJoinServiceRequest sender, Exception hrCompletionStatus)
        {
            TestLogger.LogMessage("DomainJoinServiceRequestCompleted");

            if (hrCompletionStatus != null)
            {
                TestLogger.LogError("DomainJoinServiceRequestCompleted failed with " + hrCompletionStatus.HResult);
            }
        }

        void HandleIndivServiceRequest_Finished(bool bResult)
        {
            TestLogger.LogMessage("Enter DomainJoin.HandleIndivServiceRequest_Finished()");

            TestLogger.LogMessage("HandleIndivServiceRequest_Finished(): " + bResult.ToString());
            if (bResult)
            {
                DomainJoinProactively();
            }

            TestLogger.LogMessage("Leave DomainJoin.HandleIndivServiceRequest_Finished()");
        }

        public void DomainJoinProactively()
        {
            TestLogger.LogMessage("Enter DomainJoin.DomainJoinProactively()");
            try
            {
                PlayReadyDomainJoinServiceRequest domainJoinRequest = new PlayReadyDomainJoinServiceRequest();

                DomainJoinReactively(domainJoinRequest);
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

            TestLogger.LogMessage("Leave DomainJoin.DomainJoinProactively");
        }

        void ConfigureServiceRequest()
        {
            PlayReadyDomainJoinServiceRequest domainJoinRequest = _serviceRequest as PlayReadyDomainJoinServiceRequest;

            domainJoinRequest.DomainServiceId = g_domainServiceID;
            domainJoinRequest.DomainAccountId = g_domainAccountID;

            TestLogger.LogMessage(" ");
            TestLogger.LogMessage("Configure DomainJoin to these values:");

            domainJoinRequest.Uri = new Uri(MainPage.g_LAURL);

            TestLogger.LogMessage("DomainServiceId: " + domainJoinRequest.DomainServiceId);
            TestLogger.LogMessage("DomainAccountId: " + domainJoinRequest.DomainAccountId);
            TestLogger.LogMessage("Uri: " + domainJoinRequest.Uri);
            TestLogger.LogMessage(" ");
        }

        async public void DomainJoinReactively(PlayReadyDomainJoinServiceRequest domainJoinRequest)
        {
            TestLogger.LogMessage("Enter DomainJoin.DomainJoinReactively()");
            Exception exception = null;

            try
            {
                _serviceRequest = domainJoinRequest;
                ConfigureServiceRequest();

                if (RequestConfigData.ManualEnabling)
                {
                    TestLogger.LogMessage("Manually posting the request...");

                    HttpHelper httpHelper = new HttpHelper(domainJoinRequest);
                    await httpHelper.GenerateChallengeAndProcessResponse();
                }
                else
                {
                    TestLogger.LogMessage("Begin domain join service request...");
                    await domainJoinRequest.BeginServiceRequest();
                }
            }
            catch (Exception ex)
            {
                TestLogger.LogMessage("Saving exception..");
                exception = ex;
            }
            finally
            {
                DomainJoinServiceRequestCompleted(domainJoinRequest, exception);
            }

            TestLogger.LogMessage("Leave DomainJoin.DomainJoinReactively()");
        }
    }

    public class DomainJoinAndReportResult : DomainJoin
    {
        ReportResultDelegate _reportResult = null;
        string _strExpectedError = null;

        public string ExpectedError
        {
            set { this._strExpectedError = value; }
            get { return this._strExpectedError; }
        }

        public DomainJoinAndReportResult(ReportResultDelegate callback)
        {
            _reportResult = callback;
        }

        protected override void DomainJoinServiceRequestCompleted(PlayReadyDomainJoinServiceRequest sender, Exception hrCompletionStatus)
        {
            TestLogger.LogMessage("Enter DomainJoinAndReportResult.DomainJoinServiceRequestCompleted()");

            if (hrCompletionStatus == null)
            {
                TestLogger.LogMessage("************************************    Domain Join succeeded       ****************************************");
                _reportResult(true);
            }
            else
            {
                if (!PerformEnablingActionIfRequested(hrCompletionStatus))
                {
                    TestLogger.LogError("DomainJoinServiceRequestCompleted ERROR: " + hrCompletionStatus.ToString());
                    _reportResult(false);
                }
            }

            TestLogger.LogMessage("Leave DomainJoinAndReportResult.DomainJoinServiceRequestCompleted()");
        }

        protected override void EnablingActionCompleted(bool bResult)
        {
            TestLogger.LogMessage("Enter DomainJoinAndReportResult.EnablingActionCompleted()");

            _reportResult(bResult);

            TestLogger.LogMessage("Leave DomainJoinAndReportResult.EnablingActionCompleted()");
        }

    }

}
