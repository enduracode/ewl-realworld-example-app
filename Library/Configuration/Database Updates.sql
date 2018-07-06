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

create table UserTransactions(
	UserTransactionId int
		not null
		constraint UserTransactionsPk primary key,
	TransactionDateAndTime datetime2
		not null,
	UserId int
		null
		constraint UserTransactionsUserIdFk references Users
)
go

create table Revisions(
	RevisionId int
		not null
		constraint RevisionsPk primary key,
	LatestRevisionId int
		not null
		constraint RevisionsLatestRevisionIdFk references Revisions,
	UserTransactionId int
		not null
		constraint RevisionsUserTransactionIdFk references UserTransactions,
	constraint RevisionsLatestRevisionIdAndUserTransactionIdUnique unique( LatestRevisionId, UserTransactionId )
)
go

create table ArticleRevisions(
	ArticleRevisionId int
		not null
		constraint ArticleRevisionsPk primary key
		constraint ArticleRevisionsArticleRevisionIdFk references Revisions,
	AuthorId int
		not null
		constraint ArticleRevisionsAuthorIdFk references Users,
	Slug varchar( 100 )
		not null
		constraint ArticleRevisionsSlugUnique unique,
	Title varchar( 100 )
		not null,
	[Description] varchar( 100 )
		not null,
	BodyMarkdown varchar( max )
		not null,
	Deleted bit
		not null
)
go