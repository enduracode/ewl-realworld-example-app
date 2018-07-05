create table Users(
	UserId int
		not null
		constraint UsersPk primary key,
	ProfilePictureUrl varchar( 100 )
		not null,
	Username varchar( 100 )
		not null
		constraint UsersUsernameUnique unique,
	ShortBio varchar( 500 )
		not null,
	EmailAddress varchar( 100 )
		not null,
	Salt int
		not null,
	SaltedPassword varbinary( 20 )
		null
)
go