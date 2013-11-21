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
    public class DomainLeave : ServiceRequest
    {
        static Guid g_domainServiceID = new Guid("{DEB47F00-8A3B-416D-9B1E-5D55FD023044}");
        static Guid g_domainAccountID = new Guid("{3A87FB03-C53E-46F9-8CF8-9967CB6A1B14}");

        protected virtual void DomainLeaveServiceRequestCompleted(PlayReadyDomainLeaveServiceRequest sender, Exception hrCompletionStatus)
        {
            TestLogger.LogMessage("DomainLeaveServiceRequestCompleted");

            if (hrCompletionStatus != null)
            {
                TestLogger.LogError("DomainLeaveServiceRequestCompleted failed with " + hrCompletionStatus.HResult);
            }
        }

        void HandleIndivServiceRequest_Finished(bool bResult)
        {
            TestLogger.LogMessage("Enter DomainLeave.HandleIndivServiceRequest_Finished()");

            TestLogger.LogMessage("HandleIndivServiceRequest_Finished(): " + bResult.ToString());
            if (bResult)
            {
                DomainLeaveProactively();
            }

            TestLogger.LogMessage("Leave DomainLeave.HandleIndivServiceRequest_Finished()");
        }

        public void DomainLeaveProactively()
        {
            TestLogger.LogMessage("Enter DomainLeave.DomainLeaveProactively()");
            try
            {
                PlayReadyDomainLeaveServiceRequest domainLeaveRequest = new PlayReadyDomainLeaveServiceRequest();

                DomainLeaveReactively(domainLeaveRequest);
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
                    TestLogger.LogMessage("DomainLeaveProactively failed:" + ex.HResult);
                }
            }

            TestLogger.LogMessage("Leave DomainLeave.DomainLeaveProactively()");
        }

        void ConfigureServiceRequest()
        {
            PlayReadyDomainLeaveServiceRequest domainLeaveRequest = _serviceRequest as PlayReadyDomainLeaveServiceRequest;

            domainLeaveRequest.DomainServiceId = g_domainServiceID;
            domainLeaveRequest.DomainAccountId = g_domainAccountID;

            TestLogger.LogMessage(" ");
            TestLogger.LogMessage("Configure DomainJoin to these values:");

            domainLeaveRequest.Uri = new Uri(MainPage.g_LAURL);

            TestLogger.LogMessage("DomainServiceId: " + domainLeaveRequest.DomainServiceId);
            TestLogger.LogMessage("DomainAccountId: " + domainLeaveRequest.DomainAccountId);
            TestLogger.LogMessage("Uri: " + domainLeaveRequest.Uri);
            TestLogger.LogMessage(" ");
        }

        async public void DomainLeaveReactively(PlayReadyDomainLeaveServiceRequest domainLeaveRequest)
        {
            TestLogger.LogMessage("Enter DomainLeave.DomainLeaveReactively()");
            Exception exception = null;

            try
            {
                _serviceRequest = domainLeaveRequest;
                ConfigureServiceRequest();

                if (RequestConfigData.ManualEnabling)
                {
                    TestLogger.LogMessage("Manually posting the request...");

                    HttpHelper httpHelper = new HttpHelper(domainLeaveRequest);
                    await httpHelper.GenerateChallengeAndProcessResponse();
                }
                else
                {
                    TestLogger.LogMessage("Begin domain leave service request...");
                    await domainLeaveRequest.BeginServiceRequest();
                }
            }
            catch (Exception ex)
            {
                TestLogger.LogMessage("Saving exception..");
                exception = ex;
            }
            finally
            {
                DomainLeaveServiceRequestCompleted(domainLeaveRequest, exception);
            }

            TestLogger.LogMessage("Leave DomainLeave.DomainJoinReactively()");
        }
    }

    public class DomainLeaveAndReportResult : DomainLeave
    {
        ReportResultDelegate _reportResult = null;
        string _strExpectedError = null;

        public string ExpectedError
        {
            set { this._strExpectedError = value; }
            get { return this._strExpectedError; }
        }

        public DomainLeaveAndReportResult(ReportResultDelegate callback)
        {
            _reportResult = callback;
        }

        protected override void DomainLeaveServiceRequestCompleted(PlayReadyDomainLeaveServiceRequest sender, Exception hrCompletionStatus)
        {
            TestLogger.LogMessage("Enter DomainLeaveAndReportResult.DomainLeaveServiceRequestCompleted()");

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

            TestLogger.LogMessage("Leave DomainLeaveAndReportResult.DomainLeaveServiceRequestCompleted()");
        }

        protected override void EnablingActionCompleted(bool bResult)
        {
            TestLogger.LogMessage("Enter DomainJoinAndReportResult.EnablingActionCompleted()");

            _reportResult(bResult);

            TestLogger.LogMessage("Leave DomainJoinAndReportResult.EnablingActionCompleted()");
        }

    }

}
