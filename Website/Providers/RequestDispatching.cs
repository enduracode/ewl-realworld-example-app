using EnterpriseWebLibrary.EnterpriseWebFramework;

namespace EwlRealWorld.Website.Providers {
	partial class RequestDispatching {
		public override UrlHandler GetFrameworkUrlParent() => new Pages.Home();
	}
}