using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Humanizer;

// Parameter: int? articleId

namespace EwlRealWorld.Website.Pages {
	partial class Editor: EwfPage {
		partial class Info {
			internal ArticleRevisionsTableRetrieval.Row Article { get; private set; }

			protected override void init() {
				if( ArticleId.HasValue )
					Article = ArticleRevisionsTableRetrieval.GetRowMatchingId( ArticleId.Value );
			}
		}

		protected override void loadData() {
			ArticleRevisionsModification mod;
			if( info.ArticleId.HasValue )
				mod = info.Article.ToModificationAsRevision();
			else {
				mod = ArticleRevisionsModification.CreateForInsertAsRevision();
				mod.Deleted = false;
			}

			FormState.ExecuteWithDataModificationsAndDefaultAction(
				PostBack.CreateFull(
						firstModificationMethod: () => {
							if( !info.ArticleId.HasValue )
								mod.Slug = getSuffixedSlug( mod.Title.ToUrlSlug() );
							mod.Execute();
						},
						actionGetter: () => new PostBackAction( Article.GetInfo( mod.ArticleRevisionId ) ) )
					.ToCollection(),
				() => {
					var table = FormItemBlock.CreateFormItemTable();

					table.AddFormItems(
						mod.GetTitleTextControlFormItem( false, label: "Article title".ToComponents(), value: info.ArticleId.HasValue ? null : "" ),
						mod.GetDescriptionTextControlFormItem( false, label: "What's this article about?".ToComponents(), value: info.ArticleId.HasValue ? null : "" ),
						mod.GetBodyMarkdownTextControlFormItem(
							false,
							label: "Write your article (in markdown)".ToComponents(),
							controlSetup: TextControlSetup.Create( numberOfRows: 8 ),
							value: info.ArticleId.HasValue ? null : "" ) );

					ph.AddControlsReturnThis( table );
					EwfUiStatics.SetContentFootActions( new ActionButtonSetup( "Publish Article", new PostBackButton() ) );
				} );
		}

		private string getSuffixedSlug( string slug ) {
			var otherArticles = ArticleRevisionsTableRetrieval.GetRows();
			for( var suffix = 1;; suffix += 1 ) {
				var suffixedSlug = slug + ( suffix == 1 ? "" : "-{0}".FormatWith( suffix.ToString() ) );
				if( otherArticles.All( i => i.Slug != suffixedSlug ) )
					return suffixedSlug;
			}
		}
	}
}