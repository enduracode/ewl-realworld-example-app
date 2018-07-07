using System;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EwlRealWorld.Library.DataAccess.CommandConditions;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
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

			ph.AddControlsReturnThis(
				AppStatics.GetAuthorDisplay( info.Article ).Append( new HtmlBlockContainer( Markdown.ToHtml( info.Article.BodyMarkdown ) ) ).GetControls() );
		}
	}
}