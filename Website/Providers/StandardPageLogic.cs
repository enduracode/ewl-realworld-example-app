using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using Tewl.Tools;

namespace EwlRealWorld.Website.Providers {
	internal class StandardPageLogic: AppStandardPageLogicProvider {
		protected override string AppDisplayName => "Conduit";
		protected override List<ResourceInfo> GetStyleSheets() => new StaticFiles.StylesCss().ToCollection<ResourceInfo>().ToList();
	}
}