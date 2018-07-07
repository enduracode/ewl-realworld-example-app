using System;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Humanizer;

// Parameter: int? articleId

namespace EwlRealWorld.Website.Pages {
	partial class Editor: EwfPage {
		partial class Info {
			internal ArticlesTableRetrieval.Row Article { get; private set; }

			protected override void init() {
				if( ArticleId.HasValue )
					Article = ArticlesTableRetrieval.GetRowMatchingId( ArticleId.Value );
			}

			protected override bool userCanAccessResource => AppTools.User != null && ( !ArticleId.HasValue || Article.AuthorId == AppTools.User.UserId );
		}

		protected override void loadData() {
			ArticlesModification mod;
			if( info.ArticleId.HasValue )
				mod = info.Article.ToModification();
			else {
				mod = ArticlesModification.CreateForInsert();
				mod.AuthorId = AppTools.User.UserId;
			}

			FormState.ExecuteWithDataModificationsAndDefaultAction(
				PostBack.CreateFull(
						firstModificationMethod: () => {
							if( !info.ArticleId.HasValue ) {
								mod.ArticleId = MainSequence.GetNextValue();
								mod.Slug = getSuffixedSlug( mod.Title.ToUrlSlug() );
								mod.CreationDateAndTime = DateTime.UtcNow;
							}
							mod.Execute();
						},
						actionGetter: () => new PostBackAction( Article.GetInfo( mod.ArticleId ) ) )
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
			var otherArticles = ArticlesTableRetrieval.GetRows();
			for( var suffix = 1;; suffix += 1 ) {
				var suffixedSlug = slug + ( suffix == 1 ? "" : "-{0}".FormatWith( suffix.ToString() ) );
				if( otherArticles.All( i => i.Slug != suffixedSlug ) )
					return suffixedSlug;
			}
		}
	}
}