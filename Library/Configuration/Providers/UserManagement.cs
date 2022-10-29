using System;
using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.UserManagement;
using EnterpriseWebLibrary.UserManagement.IdentityProviders;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.CommandConditions;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using NodaTime;
using Tewl.Tools;

namespace EwlRealWorld.Library.Configuration.Providers {
	internal class UserManagement: SystemUserManagementProvider {
		protected override IEnumerable<IdentityProvider> GetIdentityProviders() =>
			new LocalIdentityProvider(
				"EWL Team",
				"contact EWL Team.",
				emailAddress => {
					var user = UsersTableRetrieval.GetRows( new UsersTableEqualityConditions.EmailAddress( emailAddress ) ).SingleOrDefault();
					if( user == null )
						return null;
					return ( getUserObject( user ), user.Salt, user.SaltedPassword );
				},
				userId => {
					var user = UsersTableRetrieval.GetRowMatchingId( userId );
					return ( user.LoginCodeSalt, user.HashedLoginCode,
						       user.LoginCodeExpirationDateAndTime.ToNewUnderlyingValue( v => LocalDateTime.FromDateTime( v ).InUtc().ToInstant() ),
						       user.LoginCodeRemainingAttemptCount, user.LoginCodeDestinationUrl );
				},
				( userId, salt, saltedPassword ) => {
					var mod = UsersModification.CreateForUpdate( new UsersTableEqualityConditions.UserId( userId ) );
					mod.Salt = salt;
					mod.SaltedPassword = saltedPassword;
					mod.Execute();
				},
				( userId, salt, hashedCode, expirationTime, remainingAttemptCount, destinationUrl ) => {
					var mod = UsersModification.CreateForUpdate( new UsersTableEqualityConditions.UserId( userId ) );
					mod.LoginCodeSalt = salt;
					mod.HashedLoginCode = hashedCode;
					mod.LoginCodeExpirationDateAndTime = expirationTime?.InUtc().ToDateTimeUnspecified();
					mod.LoginCodeRemainingAttemptCount = remainingAttemptCount;
					mod.LoginCodeDestinationUrl = destinationUrl;
					mod.Execute();
				} ).ToCollection();

		protected override IEnumerable<User> GetUsers() => UsersTableRetrieval.GetRows().OrderBy( i => i.Username ).Select( getUserObject );

		protected override User GetUser( int userId ) => getUserObject( UsersTableRetrieval.GetRowMatchingId( userId, returnNullIfNoMatch: true ) );

		protected override User GetUser( string emailAddress ) =>
			getUserObject( UsersTableRetrieval.GetRows( new UsersTableEqualityConditions.EmailAddress( emailAddress ) ).SingleOrDefault() );

		private User getUserObject( UsersTableRetrieval.Row user ) =>
			user == null ? null : new User( user.UserId, user.EmailAddress, getRole(), null, friendlyName: user.Username );

		protected override int InsertOrUpdateUser( int? userId, string emailAddress, int roleId, Instant? lastRequestTime ) {
			if( userId.HasValue ) {
				var mod = UsersModification.CreateForUpdate( new UsersTableEqualityConditions.UserId( userId.Value ) );
				mod.EmailAddress = emailAddress;
				mod.Execute();
			}
			else {
				userId = MainSequence.GetNextValue();
				UsersModification.InsertRow( userId.Value, "", emailAddress, "", emailAddress, 0, null, null, null, null, null, "" );
			}
			return userId.Value;
		}

		protected override void DeleteUser( int userId ) => throw new NotImplementedException();

		protected override IEnumerable<Role> GetRoles() => getRole().ToCollection();

		private Role getRole() => new Role( 1, "Standard User", false, false );
	}
}