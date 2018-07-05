using System;
using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.CommandConditions;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using NodaTime;

namespace EwlRealWorld.Library.Configuration.Providers {
	internal class UserManagement: FormsAuthCapableUserManagementProvider {
		void SystemUserManagementProvider.DeleteUser( int userId ) {
			throw new NotImplementedException();
		}

		IEnumerable<Role> SystemUserManagementProvider.GetRoles() => getRole().ToCollection();

		IEnumerable<FormsAuthCapableUser> FormsAuthCapableUserManagementProvider.GetUsers() =>
			UsersTableRetrieval.GetRows().OrderBy( i => i.Username ).Select( getUserObject );

		FormsAuthCapableUser FormsAuthCapableUserManagementProvider.GetUser( int userId ) =>
			getUserObject( UsersTableRetrieval.GetRowMatchingId( userId, returnNullIfNoMatch: true ) );

		FormsAuthCapableUser FormsAuthCapableUserManagementProvider.GetUser( string email ) =>
			getUserObject( UsersTableRetrieval.GetRows( new UsersTableEqualityConditions.EmailAddress( email ) ).SingleOrDefault() );

		private FormsAuthCapableUser getUserObject( UsersTableRetrieval.Row user ) {
			return user == null
				       ? null
				       : new FormsAuthCapableUser(
					       user.UserId,
					       user.EmailAddress,
					       getRole(),
					       null,
					       user.Salt,
					       user.SaltedPassword,
					       false,
					       friendlyName: user.Username );
		}

		private Role getRole() => new Role( 1, "Standard User", false, false );

		void FormsAuthCapableUserManagementProvider.InsertOrUpdateUser(
			int? userId, string email, int roleId, Instant? lastRequestTime, int salt, byte[] saltedPassword, bool mustChangePassword ) {
			if( userId.HasValue ) {
				var userMod = UsersModification.CreateForUpdate( new UsersTableEqualityConditions.UserId( userId.Value ) );
				userMod.EmailAddress = email;
				userMod.Salt = salt;
				userMod.SaltedPassword = saltedPassword;
				userMod.Execute();
			}
			else
				UsersModification.InsertRow( MainSequence.GetNextValue(), "", email, "", email, salt, saltedPassword );
		}

		void FormsAuthCapableUserManagementProvider.GetPasswordResetParams( string email, string password, out string subject, out string bodyHtml ) {
			subject = "";
			bodyHtml = "";
		}

		string FormsAuthCapableUserManagementProvider.AdministratingCompanyName => "EWL Team";
		string FormsAuthCapableUserManagementProvider.LogInHelpInstructions => "contact EWL Team.";
	}
}