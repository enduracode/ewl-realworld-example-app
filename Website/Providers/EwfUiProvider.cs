using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using EwlRealWorld.Website.Pages;

namespace EwlRealWorld.Website.Providers {
	internal class EwfUiProvider: AppEwfUiProvider {
		public override List<ActionButtonSetup> GetGlobalNavActionControls() {
			if( AppTools.User == null ) {
				var signUpPage = User.GetInfo();
				return new[]
					{
						ActionButtonSetup.CreateWithUrl( "Home", Home.GetInfo() ),
						ActionButtonSetup.CreateWithUrl(
							"Sign in",
							EnterpriseWebLibrary.EnterpriseWebFramework.EwlRealWorld.Website.UserManagement.LogIn.GetInfo( Home.GetInfo().GetUrl() ) ),
						ActionButtonSetup.CreateWithUrl( signUpPage.ResourceName, signUpPage )
					}.ToList();
			}

			return new[]
				{
					ActionButtonSetup.CreateWithUrl( "Home", Home.GetInfo() ),
					ActionButtonSetup.CreateWithUrl(
						"New Article",
						Editor.GetInfo( null ),
						icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-pencil-square-o" ) ) ),
					ActionButtonSetup.CreateWithUrl( "Settings", User.GetInfo(), icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-cog" ) ) ),
					ActionButtonSetup.CreateWithUrl( UsersTableRetrieval.GetRowMatchingId( AppTools.User.UserId ).Username, Profile.GetInfo( AppTools.User.UserId ) )
				}.ToList();
		}
	}
}