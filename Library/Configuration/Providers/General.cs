using EnterpriseWebLibrary;

namespace EwlRealWorld.Library.Configuration.Providers {
	internal class General: SystemGeneralProvider {
		string SystemGeneralProvider.IntermediateLogInPassword => "password";
		string SystemGeneralProvider.EmailDefaultFromName => "EWL Team";
		string SystemGeneralProvider.EmailDefaultFromAddress => "contact@enterpriseweblibrary.org";
	}
}