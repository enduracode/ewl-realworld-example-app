using System;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Tewl.Tools;

// EwlPage

namespace EwlRealWorld.Website.Pages {
	partial class User {
		public override string ResourceName => AppTools.User != null ? "Your Settings" : "Sign up";

		protected override UrlHandler getUrlParent() => new Home();

		protected override PageContent getContent() {
			var mod = getMod();
			Action<int> passwordUpdater = null;
			AuthenticationStatics.SpecifiedUserLoginModificationMethod specifiedUserLoginMethod = null;
			return FormState.ExecuteWithDataModificationsAndDefaultAction(
				PostBack.CreateFull(
						modificationMethod: () => {
							if( AppTools.User == null )
								mod.UserId = MainSequence.GetNextValue();
							mod.Execute();
							passwordUpdater?.Invoke( mod.UserId );

							specifiedUserLoginMethod?.Invoke( mod.UserId );
						},
						actionGetter: () => new PostBackAction( specifiedUserLoginMethod != null ? (PageBase)Home.GetInfo() : Profile.GetInfo( AppTools.User.UserId ) ) )
					.ToCollection(),
				() => {
					var content = new UiPageContent( contentFootActions: new ButtonSetup( AppTools.User != null ? "Update Settings" : "Sign up" ).ToCollection() );

					if( AppTools.User == null )
						content.Add(
							new EwfHyperlink(
								EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement.Pages.LogIn.GetInfo( Home.GetInfo().GetUrl() ),
								new StandardHyperlinkStyle( "Have an account?" ) ) );

					content.Add( getFormItemStack( mod, out passwordUpdater ) );

					if( AppTools.User == null ) {
						var logInHiddenFieldsAndMethods = AuthenticationStatics.GetLogInHiddenFieldsAndMethods();
						content.Add( logInHiddenFieldsAndMethods.hiddenFields );
						specifiedUserLoginMethod = logInHiddenFieldsAndMethods.modificationMethods.specifiedUserLoginMethod;
					}

					return content;
				} );
		}

		private UsersModification getMod() {
			if( AppTools.User != null )
				return UsersTableRetrieval.GetRowMatchingId( AppTools.User.UserId ).ToModification();

			var mod = UsersModification.CreateForInsert();
			mod.ProfilePictureUrl = "";
			mod.ShortBio = "";
			mod.Salt = 0;
			mod.LoginCodeDestinationUrl = "";
			return mod;
		}

		private FlowComponent getFormItemStack( UsersModification mod, out Action<int> passwordUpdater ) {
			var stack = FormItemList.CreateStack();
			Action<int> passwordUpdaterLocal = null;

			if( AppTools.User != null )
				stack.AddItem( mod.GetProfilePictureUrlUrlControlFormItem( true, label: "URL of profile picture".ToComponents() ) );
			stack.AddItem( mod.GetUsernameTextControlFormItem( false, label: "Username".ToComponents(), value: AppTools.User == null ? "" : null ) );
			if( AppTools.User != null )
				stack.AddItem(
					mod.GetShortBioTextControlFormItem( true, label: "Short bio about you".ToComponents(), controlSetup: TextControlSetup.Create( numberOfRows: 8 ) ) );
			stack.AddItem( mod.GetEmailAddressEmailAddressControlFormItem( false, label: "Email".ToComponents(), value: AppTools.User == null ? "" : null ) );

			if( AppTools.User == null )
				stack.AddItems( AuthenticationStatics.GetPasswordModificationFormItems( out passwordUpdaterLocal ) );
			else {
				var changePasswordChecked = new DataValue<bool>();
				stack.AddItem(
					changePasswordChecked.ToFlowCheckbox(
							"Change password".ToComponents(),
							setup: FlowCheckboxSetup.Create(
								nestedContentGetter: () => FormState.ExecuteWithValidationPredicate(
									() => changePasswordChecked.Value,
									() => FormItemList.CreateGrid( 1, items: AuthenticationStatics.GetPasswordModificationFormItems( out passwordUpdaterLocal ) )
										.ToCollection() ) ),
							value: false )
						.ToFormItem() );
			}

			passwordUpdater = passwordUpdaterLocal;
			return stack;
		}
	}
}