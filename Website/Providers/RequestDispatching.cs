using EnterpriseWebLibrary.EnterpriseWebFramework;
using Tewl.Tools;

namespace EwlRealWorld.Website.Providers {
	partial class RequestDispatching {
		protected override IEnumerable<BaseUrlPattern> GetBaseUrlPatterns() => Pages.Home.UrlPatterns.BaseUrlPattern().ToCollection();
		public override UrlHandler GetFrameworkUrlParent() => new Pages.Home();
	}
}