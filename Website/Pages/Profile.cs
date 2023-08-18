using EwlRealWorld.Library.DataAccess.Retrieval;
using EwlRealWorld.Library.DataAccess.TableRetrieval;

// EwlPage
// Parameter: int userId
// OptionalParameter: bool showFavorites

namespace EwlRealWorld.Website.Pages;

partial class Profile {
	private UsersTableRetrieval.Row user;

	protected override void init() {
		user = UsersTableRetrieval.GetRowMatchingId( UserId );
	}

	protected override string getResourceName() => user.Username;

	protected override UrlHandler getUrlParent() => new Home();

	protected override PageContent getContent() =>
		new UiPageContent(
			pageActions: AppTools.User != null && UserId == AppTools.User.UserId
				             ? new HyperlinkSetup( User.GetInfo(), "Edit Profile Settings", icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-cog" ) ) )
					             .ToCollection()
				             : AppStatics.GetFollowAction( UserId ).ToCollection() ).Add( getArticleSection() );

	private FlowComponent getArticleSection() =>
		new Section(
			new LineList(
					new[] { getAuthorTabComponents(), getFavoriteTabComponents() }.Select( i => (LineListItem)i.ToComponentListItem() ),
					verticalAlignment: FlexboxVerticalAlignment.Center ).Append( getResultTable() )
				.Materialize() );

	private IReadOnlyCollection<PhrasingComponent> getAuthorTabComponents() {
		const string label = "My Articles";
		return !ShowFavorites
			       ? label.ToComponents()
			       : new EwfButton(
				       new StandardButtonStyle( label ),
				       behavior: new PostBackBehavior(
					       postBack: PostBack.CreateFull( id: "author", modificationMethod: () => parametersModification.ShowFavorites = false ) ) ).ToCollection();
	}

	private IReadOnlyCollection<PhrasingComponent> getFavoriteTabComponents() {
		const string label = "Favorited Articles";
		return ShowFavorites
			       ? label.ToComponents()
			       : new EwfButton(
				       new StandardButtonStyle( label ),
				       behavior: new PostBackBehavior(
					       postBack: PostBack.CreateFull( id: "favorite", modificationMethod: () => parametersModification.ShowFavorites = true ) ) ).ToCollection();
	}

	private FlowComponent getResultTable() {
		var results = ShowFavorites ? ArticlesRetrieval.GetRowsLinkedToUser( UserId ) : ArticlesRetrieval.GetRowsLinkedToAuthor( UserId );
		var usersById = UsersTableRetrieval.GetRows().ToIdDictionary();
		var tagsByArticleId = ArticleTagsTableRetrieval.GetRows().ToArticleIdLookup();
		var favoritesByArticleId = FavoritesTableRetrieval.GetRows().ToArticleIdLookup();

		var table = EwfTable.Create( defaultItemLimit: DataRowLimit.Fifty );
		table.AddData( results, i => EwfTableItem.Create( AppStatics.GetArticleDisplay( i, usersById, tagsByArticleId, favoritesByArticleId ).ToCell() ) );
		return table;
	}
}