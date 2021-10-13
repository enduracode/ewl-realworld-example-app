using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using EwlRealWorld.Website.Pages;

namespace EwlRealWorld.Website.Providers {
	internal class EwfUi: AppEwfUiProvider {
		public override IReadOnlyCollection<ActionComponentSetup> GetGlobalNavActions() {
			if( AppTools.User == null ) {
				var signUpPage = User.GetInfo();
				return new[]
					{
						new HyperlinkSetup( Home.GetInfo(), "Home" ),
						new HyperlinkSetup( EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement.Pages.LogIn.GetInfo( Home.GetInfo().GetUrl() ), "Sign in" ),
						new HyperlinkSetup( signUpPage, signUpPage.ResourceName )
					}.ToList();
			}

			return new[]
				{
					new HyperlinkSetup( Home.GetInfo(), "Home" ),
					new HyperlinkSetup( Editor.GetInfo( null ), "New Article", icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-pencil-square-o" ) ) ),
					new HyperlinkSetup( User.GetInfo(), "Settings", icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-cog" ) ) ),
					new HyperlinkSetup( Profile.GetInfo( AppTools.User.UserId ), UsersTableRetrieval.GetRowMatchingId( AppTools.User.UserId ).Username )
				}.ToList();
		}
	}
}