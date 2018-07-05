using System;
using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.Encryption;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.TableRetrieval;

namespace EwlRealWorld.Website.Pages {
	partial class User: EwfPage {
		partial class Info {
			public override string ResourceName => AppTools.User != null ? "Your Settings" : "Sign up";
		}

		protected override void loadData() {
			if( AppTools.User == null )
				ph.AddControlsReturnThis(
					new EwfHyperlink(
							EnterpriseWebLibrary.EnterpriseWebFramework.EwlRealWorld.Website.UserManagement.LogIn.GetInfo( Home.GetInfo().GetUrl() ),
							new StandardHyperlinkStyle( "Have an account?" ) ).ToCollection()
						.GetControls() );

			UsersModification mod;
			if( AppTools.User != null )
				mod = UsersTableRetrieval.GetRowMatchingId( AppTools.User.UserId ).ToModification();
			else {
				mod = UsersModification.CreateForInsert();
				mod.ProfilePictureUrl = "";
				mod.ShortBio = "";
			}

			var password = new DataValue<string> { Value = "" };
			Tuple<IReadOnlyCollection<EtherealComponent>, Action<int>> logInHiddenFieldsAndMethod = null;
			FormState.ExecuteWithDataModificationsAndDefaultAction(
				PostBack.CreateFull(
						firstModificationMethod: () => {
							if( AppTools.User == null )
								mod.UserId = MainSequence.GetNextValue();
							if( password.Value.Any() ) {
								var passwordSalter = new Password( password.Value );
								mod.Salt = passwordSalter.Salt;
								mod.SaltedPassword = passwordSalter.ComputeSaltedHash();
							}
							mod.Execute();

							logInHiddenFieldsAndMethod?.Item2( mod.UserId );
						},
						actionGetter: () =>
							new PostBackAction( logInHiddenFieldsAndMethod != null ? (PageInfo)Home.GetInfo() : Profile.GetInfo( AppTools.User.UserId, false ) ) )
					.ToCollection(),
				() => {
					var table = FormItemBlock.CreateFormItemTable();

					if( AppTools.User != null )
						table.AddFormItems( mod.GetProfilePictureUrlUriFormItem( true, labelAndSubject: "URL of profile picture" ) );

					table.AddFormItems( mod.GetUsernameTextControlFormItem( false, label: "Username".ToComponents(), value: AppTools.User == null ? "" : null ) );

					if( AppTools.User != null )
						table.AddFormItems(
							mod.GetShortBioTextControlFormItem(
								true,
								label: "Short bio about you".ToComponents(),
								controlSetup: TextControlSetup.Create( numberOfRows: 8 ) ) );

					table.AddFormItems(
						mod.GetEmailAddressEmailAddressControlFormItem( false, label: "Email".ToComponents(), value: AppTools.User == null ? "" : null ) );

					if( AppTools.User == null )
						table.AddFormItems( password.GetPasswordModificationFormItems().ToArray() );
					else {
						var changePasswordChecked = new DataValue<bool>();
						table.AddFormItems(
							FormItem.Create(
								"",
								changePasswordChecked.ToBlockCheckbox(
									"Change password".ToComponents(),
									setup: new BlockCheckBoxSetup(
										nestedControlListGetter: () => FormState.ExecuteWithValidationPredicate(
											() => changePasswordChecked.Value,
											() => FormItemBlock.CreateFormItemList( numberOfColumns: 1, formItems: password.GetPasswordModificationFormItems() ).ToCollection() ) ),
									value: false ),
								validationGetter: control => control.Validation ) );
					}

					ph.AddControlsReturnThis( table );

					EwfUiStatics.SetContentFootActions( new ActionButtonSetup( AppTools.User != null ? "Update Settings" : "Sign up", new PostBackButton() ) );

					if( AppTools.User == null ) {
						logInHiddenFieldsAndMethod = FormsAuthStatics.GetLogInHiddenFieldsAndSpecifiedUserLogInMethod();
						logInHiddenFieldsAndMethod.Item1.AddEtherealControls( ph );
					}
				} );
		}
	}
}