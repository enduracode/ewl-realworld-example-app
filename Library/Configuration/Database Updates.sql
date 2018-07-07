﻿create table Users(
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

create table Articles(
	ArticleId int
		not null
		constraint ArticlesPk primary key,
	AuthorId int
		not null
		constraint ArticlesAuthorIdFk references Users,
	Slug varchar( 100 )
		not null
		constraint ArticlesSlugUnique unique,
	Title varchar( 100 )
		not null,
	[Description] varchar( 100 )
		not null,
	BodyMarkdown varchar( max )
		not null,
	CreationDateAndTime datetime2
		not null
)
go