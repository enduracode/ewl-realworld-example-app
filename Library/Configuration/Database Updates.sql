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

create table Tags(
	TagId int
		not null
		constraint TagsPk primary key,
	TagName varchar( 25 )
		not null
		constraint TagsTagNameUnique unique
)
go

create table ArticleTags(
	ArticleId int
		not null
		constraint ArticleTagsArticleIdFk references Articles,
	TagId int
		not null
		constraint ArticleTagsTagIdFk references Tags,
	constraint ArticleTagsPk primary key( ArticleId, TagId )
)
go

create table Follows(
	FollowerId int
		not null
		constraint FollowsFollowerIdFk references Users,
	FolloweeId int
		not null
		constraint FollowsFolloweeIdFk references Users,
	constraint FollowsPk primary key( FollowerId, FolloweeId )
)
go

create table Favorites(
	UserId int
		not null
		constraint FavoritesUserIdFk references Users,
	ArticleId int
		not null
		constraint FavoritesArticleIdFk references Articles,
	constraint FavoritesPk primary key( UserId, ArticleId )
)
go