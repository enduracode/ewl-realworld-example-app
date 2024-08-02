using EnterpriseWebLibrary.UserManagement;
using EnterpriseWebLibrary.UserManagement.IdentityProviders;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.CommandConditions;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using NodaTime;

namespace EwlRealWorld.Library.Configuration.Providers;

internal class UserManagement: SystemUserManagementProvider {
	protected override IEnumerable<IdentityProvider> GetIdentityProviders() =>
		new LocalIdentityProvider(
			"EWL Team",
			"contact EWL Team.",
			emailAddress => {
				var user = UsersTableRetrieval.GetRows( new UsersTableEqualityConditions.EmailAddress( emailAddress ) ).SingleOrDefault();
				if( user is null )
					return null;
				return ( getUserObject( user )!, user.Salt, user.SaltedPassword );
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

	protected override IEnumerable<SystemUser> GetUsers() => UsersTableRetrieval.GetRows().OrderBy( i => i.Username ).Select( i => getUserObject( i )! );

	protected override SystemUser? GetUser( int userId ) => UsersTableRetrieval.TryGetRowMatchingId( userId, out var user ) ? getUserObject( user ) : null;

	protected override SystemUser? GetUser( string emailAddress ) =>
		getUserObject( UsersTableRetrieval.GetRows( new UsersTableEqualityConditions.EmailAddress( emailAddress ) ).SingleOrDefault() );

	private SystemUser? getUserObject( UsersTableRetrieval.Row? user ) =>
		user is null ? null : new SystemUser( user.UserId, user.EmailAddress, getRole(), friendlyName: user.Username );

	protected override int InsertOrUpdateUser( int? userId, string emailAddress, int roleId ) {
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

	private Role getRole() => new( 1, "Standard User", false, false );

	protected override IEnumerable<UserRequest> GetUserRequests() => [ ];

	protected override void InsertUserRequest( int userId, Instant requestTime ) {}

	protected override void ClearUserRequests() {}
}