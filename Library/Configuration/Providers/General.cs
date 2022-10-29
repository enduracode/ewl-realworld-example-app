using EnterpriseWebLibrary;

namespace EwlRealWorld.Library.Configuration.Providers {
	internal class General: SystemGeneralProvider {
		protected override string IntermediateLogInPassword => "password";
		protected override string EmailDefaultFromName => "EWL Team";
		protected override string EmailDefaultFromAddress => "contact@enterpriseweblibrary.org";
	}
}