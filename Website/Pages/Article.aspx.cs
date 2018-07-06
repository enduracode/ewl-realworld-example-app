using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Markdig;

// Parameter: int articleId

namespace EwlRealWorld.Website.Pages {
	partial class Article: EwfPage {
		partial class Info {
			internal ArticleRevisionsTableRetrieval.Row Article { get; private set; }

			protected override void init() {
				Article = ArticleRevisionsTableRetrieval.GetRowMatchingId( ArticleId );
			}

			public override string ResourceName => Article.Title;
		}

		protected override void loadData() {
			ph.AddControlsReturnThis( new HtmlBlockContainer( Markdown.ToHtml( info.Article.BodyMarkdown ) ).ToCollection().GetControls() );
		}
	}
}