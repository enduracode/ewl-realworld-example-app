using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EwlRealWorld.Library.DataAccess.Retrieval;
using EwlRealWorld.Library.DataAccess.TableRetrieval;

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

		protected override void loadData() {
			EwfUiStatics.SetPageActions(
				AppTools.User != null && info.UserId == AppTools.User.UserId
					? ActionButtonSetup.CreateWithUrl( "Edit Profile Settings", Pages.User.GetInfo(), icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-cog" ) ) )
					: AppStatics.GetFollowAction( info.UserId ) );

			ph.AddControlsReturnThis( getArticleSection() );
		}

		private Control getArticleSection() =>
			new LegacySection(
				new LineList(
						new[] { getAuthorTabComponents(), getFavoriteTabComponents() }.Select( i => (LineListItem)i.ToComponentListItem() ),
						verticalAlignment: FlexboxVerticalAlignment.Center ).ToCollection()
					.GetControls()
					.Append( getResultTable() ) );

		private IReadOnlyCollection<PhrasingComponent> getAuthorTabComponents() {
			const string label = "My Articles";
			return !info.ShowFavorites
				       ? label.ToComponents()
				       : new EwfButton(
						       new StandardButtonStyle( label ),
						       behavior: new PostBackBehavior(
							       postBack: PostBack.CreateFull( id: "author", firstModificationMethod: () => parametersModification.ShowFavorites = false ) ) )
					       .ToCollection();
		}

		private IReadOnlyCollection<PhrasingComponent> getFavoriteTabComponents() {
			const string label = "Favorited Articles";
			return info.ShowFavorites
				       ? label.ToComponents()
				       : new EwfButton(
						       new StandardButtonStyle( label ),
						       behavior: new PostBackBehavior(
							       postBack: PostBack.CreateFull( id: "favorite", firstModificationMethod: () => parametersModification.ShowFavorites = true ) ) )
					       .ToCollection();
		}

		private Control getResultTable() {
			var results = info.ShowFavorites ? ArticlesRetrieval.GetRowsLinkedToUser( info.UserId ) : ArticlesRetrieval.GetRowsLinkedToAuthor( info.UserId );
			var usersById = UsersTableRetrieval.GetRows().ToIdDictionary();
			var tagsByArticleId = ArticleTagsTableRetrieval.GetRows().ToArticleIdLookup();
			var favoritesByArticleId = FavoritesTableRetrieval.GetRows().ToArticleIdLookup();

			var table = EwfTable.Create( defaultItemLimit: DataRowLimit.Fifty );
			table.AddData( results, i => new EwfTableItem( AppStatics.GetArticleDisplay( i, usersById, tagsByArticleId, favoritesByArticleId ).ToCell() ) );
			return table;
		}
	}
}