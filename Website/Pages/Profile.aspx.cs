using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EwlRealWorld.Library.DataAccess.Retrieval;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Tewl.Tools;

// Parameter: int userId
// OptionalParameter: bool showFavorites

namespace EwlRealWorld.Website.Pages {
	partial class Profile: EwfPage {
		partial class Info {
			internal UsersTableRetrieval.Row User { get; private set; }

			protected override void init() {
				User = UsersTableRetrieval.GetRowMatchingId( UserId );
			}

			public override string ResourceName => User.Username;
		}

		protected override PageContent getContent() =>
			new UiPageContent(
				pageActions: AppTools.User != null && info.UserId == AppTools.User.UserId
					             ? new HyperlinkSetup( Pages.User.GetInfo(), "Edit Profile Settings", icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-cog" ) ) )
						             .ToCollection()
					             : AppStatics.GetFollowAction( info.UserId ).ToCollection() ).Add( getArticleSection() );

		private FlowComponent getArticleSection() =>
			new Section(
				new LineList(
						new[] { getAuthorTabComponents(), getFavoriteTabComponents() }.Select( i => (LineListItem)i.ToComponentListItem() ),
						verticalAlignment: FlexboxVerticalAlignment.Center ).Append( getResultTable() )
					.Materialize() );

		private IReadOnlyCollection<PhrasingComponent> getAuthorTabComponents() {
			const string label = "My Articles";
			return !info.ShowFavorites
				       ? label.ToComponents()
				       : new EwfButton(
					       new StandardButtonStyle( label ),
					       behavior: new PostBackBehavior(
						       postBack: PostBack.CreateFull( id: "author", modificationMethod: () => parametersModification.ShowFavorites = false ) ) ).ToCollection();
		}

		private IReadOnlyCollection<PhrasingComponent> getFavoriteTabComponents() {
			const string label = "Favorited Articles";
			return info.ShowFavorites
				       ? label.ToComponents()
				       : new EwfButton(
					       new StandardButtonStyle( label ),
					       behavior: new PostBackBehavior(
						       postBack: PostBack.CreateFull( id: "favorite", modificationMethod: () => parametersModification.ShowFavorites = true ) ) ).ToCollection();
		}

		private FlowComponent getResultTable() {
			var results = info.ShowFavorites ? ArticlesRetrieval.GetRowsLinkedToUser( info.UserId ) : ArticlesRetrieval.GetRowsLinkedToAuthor( info.UserId );
			var usersById = UsersTableRetrieval.GetRows().ToIdDictionary();
			var tagsByArticleId = ArticleTagsTableRetrieval.GetRows().ToArticleIdLookup();
			var favoritesByArticleId = FavoritesTableRetrieval.GetRows().ToArticleIdLookup();

			var table = EwfTable.Create( defaultItemLimit: DataRowLimit.Fifty );
			table.AddData( results, i => EwfTableItem.Create( AppStatics.GetArticleDisplay( i, usersById, tagsByArticleId, favoritesByArticleId ).ToCell() ) );
			return table;
		}
	}
}