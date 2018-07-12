using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EwlRealWorld.Library;
using EwlRealWorld.Library.DataAccess.CommandConditions;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Humanizer;
using Markdig;

// Parameter: int articleId

namespace EwlRealWorld.Website.Pages {
	partial class Article: EwfPage {
		partial class Info {
			internal ArticlesTableRetrieval.Row Article { get; private set; }

			protected override void init() {
				Article = ArticlesTableRetrieval.GetRowMatchingId( ArticleId );
			}

			public override string ResourceName => Article.Title;
		}

		protected override void loadData() {
			if( AppTools.User != null && info.Article.AuthorId == AppTools.User.UserId )
				EwfUiStatics.SetPageActions(
					ActionButtonSetup.CreateWithUrl(
						"Edit Article",
						Editor.GetInfo( info.ArticleId ),
						icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-pencil" ) ) ),
					new ActionButtonSetup(
						"Delete Article",
						new PostBackButton(
							postBack: PostBack.CreateFull(
								id: "delete",
								firstModificationMethod: () => {
									ArticleTagsModification.DeleteRows( new ArticleTagsTableEqualityConditions.ArticleId( info.ArticleId ) );
									ArticlesModification.DeleteRows( new ArticlesTableEqualityConditions.ArticleId( info.ArticleId ) );
								},
								actionGetter: () => new PostBackAction( Home.GetInfo() ) ) ),
						icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-trash" ) ) ) );
			else
				EwfUiStatics.SetPageActions( getFollowAction(), getFavoriteAction() );

			ph.AddControlsReturnThis(
				AppStatics.GetAuthorDisplay( info.Article )
					.Append( new HtmlBlockContainer( Markdown.ToHtml( info.Article.BodyMarkdown ) ) )
					.Append(
						new LineList(
							ArticleTagsTableRetrieval.GetRowsLinkedToArticle( info.ArticleId )
								.Select( i => (LineListItem)TagsTableRetrieval.GetRowMatchingId( i.TagId ).TagName.ToComponents().ToComponentListItem() ),
							generalSetup: new ComponentListSetup( classes: ElementClasses.Tag ) ) )
					.GetControls() );
		}

		private ActionButtonSetup getFollowAction() {
			ActionControl actionControl;
			if( AppTools.User == null )
				actionControl = new EwfLink( Pages.User.GetInfo() );
			else if( FollowsTableRetrieval.GetRowMatchingPk( AppTools.User.UserId, info.Article.AuthorId, returnNullIfNoMatch: true ) == null )
				actionControl = new PostBackButton(
					postBack: PostBack.CreateFull(
						id: "follow",
						firstModificationMethod: () => FollowsModification.InsertRow( AppTools.User.UserId, info.Article.AuthorId ) ) );
			else
				actionControl = new PostBackButton(
					postBack: PostBack.CreateFull(
						id: "unfollow",
						firstModificationMethod: () => FollowsModification.DeleteRows(
							new FollowsTableEqualityConditions.FollowerId( AppTools.User.UserId ),
							new FollowsTableEqualityConditions.FolloweeId( info.Article.AuthorId ) ) ) );

			return new ActionButtonSetup(
				"{0} {1} ({2})".FormatWith(
					AppTools.User == null || FollowsTableRetrieval.GetRowMatchingPk( AppTools.User.UserId, info.Article.AuthorId, returnNullIfNoMatch: true ) == null
						? "Follow"
						: "Unfollow",
					UsersTableRetrieval.GetRowMatchingId( info.Article.AuthorId ).Username,
					FollowsTableRetrieval.GetRows( new FollowsTableEqualityConditions.FolloweeId( info.Article.AuthorId ) ).Count() ),
				actionControl,
				icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-plus" ) ) );
		}

		private ActionButtonSetup getFavoriteAction() {
			ActionControl actionControl;
			if( AppTools.User == null )
				actionControl = new EwfLink( Pages.User.GetInfo() );
			else if( FavoritesTableRetrieval.GetRowMatchingPk( AppTools.User.UserId, info.ArticleId, returnNullIfNoMatch: true ) == null )
				actionControl = new PostBackButton(
					postBack: PostBack.CreateFull(
						id: "favorite",
						firstModificationMethod: () => FavoritesModification.InsertRow( AppTools.User.UserId, info.ArticleId ) ) );
			else
				actionControl = new PostBackButton(
					postBack: PostBack.CreateFull(
						id: "unfavorite",
						firstModificationMethod: () => FavoritesModification.DeleteRows(
							new FavoritesTableEqualityConditions.UserId( AppTools.User.UserId ),
							new FavoritesTableEqualityConditions.ArticleId( info.ArticleId ) ) ) );

			return new ActionButtonSetup(
				"{0} Article ({1})".FormatWith(
					AppTools.User == null || FavoritesTableRetrieval.GetRowMatchingPk( AppTools.User.UserId, info.ArticleId, returnNullIfNoMatch: true ) == null
						? "Favorite"
						: "Unfavorite",
					FavoritesTableRetrieval.GetRows( new FavoritesTableEqualityConditions.ArticleId( info.ArticleId ) ).Count() ),
				actionControl,
				icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-heart" ) ) );
		}
	}
}