using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Media.PlayReadyClient;

namespace PlayReady_CS_RTM_Windows_Store_app
{

    public class LicenseManagement 
    {

        static public void DumpLicenseValues(PlayReadyLicense license)
        {
            TestLogger.LogMessage(" " );
            TestLogger.LogMessage("License values:" );
            
            TestLogger.LogMessage("FullyEvaluated  :" + license.FullyEvaluated.ToString() );
            TestLogger.LogMessage("UsableForPlay   :" + license.UsableForPlay.ToString() );

            if( license.ExpirationDate == null )
            {
                TestLogger.LogMessage("Expiration date  : Not specified" );
            }
            else
            {
                TestLogger.LogMessage("Expiration date  :" + license.ExpirationDate.ToString() );
            }
            TestLogger.LogMessage("Expiration period after first play  :" + license.ExpireAfterFirstPlay );
            
            TestLogger.LogMessage("DomainAccountId :" + license.DomainAccountID.ToString() );
            TestLogger.LogMessage("ChainDepth      :" + license.ChainDepth );
            for( uint i = 0; i < license.ChainDepth; i++ )
            {
                Guid keyId = license.GetKIDAtChainDepth(i);
                TestLogger.LogMessage(String.Format("KeyId at chain depth ( {0} ) : {1}", i, keyId.ToString() ));
            }
            TestLogger.LogMessage(" " );
            
        }
        
        static public  PlayReadyLicense FindSingleLicense( Guid keyId, string keyIdString, bool bFullyEvaluated )
        {
            TestLogger.LogMessage("Enter LicenseManagement.FindSingleLicense()" );
            
            PlayReadyContentHeader contentHeader = new PlayReadyContentHeader(
                                                                                keyId,
                                                                                keyIdString,
                                                                                PlayReadyEncryptionAlgorithm.Aes128Ctr,
                                                                                null,
                                                                                null,
                                                                                String.Empty, 
                                                                                Guid.Empty);
            
            TestLogger.LogMessage("Creating PlayReadyLicenseIterable..." );
            PlayReadyLicenseIterable licenseIterable = new PlayReadyLicenseIterable( contentHeader, bFullyEvaluated );
            foreach( PlayReadyLicense lic in licenseIterable )
            {
                DumpLicenseValues( lic );
            }
            
            PlayReadyLicense license = null;
            IEnumerable<IPlayReadyLicense> licenseEnumerable = licenseIterable;
            
            int licenseCount = Enumerable.Count<IPlayReadyLicense>( licenseEnumerable );
            TestLogger.LogMessage("License count  :" + licenseCount );
            if( licenseCount > 0 )
            {
                license = Enumerable.ElementAt<IPlayReadyLicense>( licenseEnumerable, 0 ) as PlayReadyLicense;
            }
            
            TestLogger.LogMessage("Leave LicenseManagement.FindSingleLicense()" );
            
            return license;
        }
        
        static public  IPlayReadyLicense[] FindMultipleLicenses( Guid keyId, string keyIdString, bool bFullyEvaluated )
        {
            TestLogger.LogMessage("Enter LicenseManagement.FindMultipleLicenses()" );
            
            PlayReadyContentHeader contentHeader = new PlayReadyContentHeader(
                                                                                keyId,
                                                                                keyIdString,
                                                                                PlayReadyEncryptionAlgorithm.Aes128Ctr,
                                                                                null,
                                                                                null,
                                                                                String.Empty, 
                                                                                Guid.Empty);
            
            TestLogger.LogMessage("Creating PlayReadyLicenseIterable..." );
            PlayReadyLicenseIterable licenseIterable = new PlayReadyLicenseIterable( contentHeader, bFullyEvaluated );
            foreach( PlayReadyLicense lic in licenseIterable )
            {
                DumpLicenseValues( lic );
            }
            
            IPlayReadyLicense[] licenses = null;
            IEnumerable<IPlayReadyLicense> licenseEnumerable = licenseIterable;
            
            int licenseCount = Enumerable.Count<IPlayReadyLicense>( licenseEnumerable );
            TestLogger.LogMessage("License count  :" + licenseCount );
            if( licenseCount > 0 )
            {
                licenses = Enumerable.ToArray<IPlayReadyLicense>( licenseEnumerable );
            }
            
            TestLogger.LogMessage("Leave LicenseManagement.FindMultipleLicenses()" );
            
            return licenses;
        }

        static public async  Task DeleteLicenses( Guid keyId, string keyIdString, PlayReadyEncryptionAlgorithm algorithm )
        {
            TestLogger.LogMessage("Enter LicenseManagement.DeleteLicenses()" );
            TestLogger.LogMessage("PlayReadyEncryptionType = " + algorithm.ToString() );
            
            PlayReadyContentHeader contentHeader = new PlayReadyContentHeader(
                                                                                keyId,
                                                                                keyIdString,
                                                                                algorithm,
                                                                                null,
                                                                                null,
                                                                                String.Empty, 
                                                                                Guid.Empty);
            
            TestLogger.LogMessage("Deleting licenses..." );
            await PlayReadyLicenseManagement.DeleteLicenses( contentHeader );
            
            TestLogger.LogMessage("Leave LicenseManagement.DeleteLicenses()" );
            
        }
        

    }


}
