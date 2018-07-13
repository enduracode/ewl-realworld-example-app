using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EwlRealWorld.Library;

namespace EwlRealWorld.Website.Pages {
	partial class Home: EwfPage {
		protected override void loadData() {
			if( AppTools.User == null )
				ph.AddControlsReturnThis(
					new GenericFlowContainer(
							new GenericFlowContainer( EwfApp.Instance.AppDisplayName.ToComponents() ).ToCollection<FlowComponent>()
								.Append( new Paragraph( "A place to share your knowledge.".ToComponents() ) )
								.Materialize(),
							classes: ElementClasses.Banner ).ToCollection()
						.GetControls() );
		}
	}
}