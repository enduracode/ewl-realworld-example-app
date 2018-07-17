using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EwlRealWorld.Library;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.CommandConditions;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.Retrieval;
using EwlRealWorld.Library.DataAccess.TableConstants;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Humanizer;

// Parameter: int? articleId

// PageState: IEnumerable<int> tags

namespace EwlRealWorld.Website.Pages {
	partial class Editor: EwfPage {
		partial class Info {
			internal ArticlesRetrieval.Row Article { get; private set; }

			protected override void init() {
				if( ArticleId.HasValue )
					Article = ArticlesRetrieval.GetRowMatchingId( ArticleId.Value );
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

			var tagIds = getTags(
				info.ArticleId.HasValue ? ArticleTagsTableRetrieval.GetRowsLinkedToArticle( info.ArticleId.Value ).Select( i => i.TagId ) : Enumerable.Empty<int>() );

			FormState.ExecuteWithDataModificationsAndDefaultAction(
				PostBack.CreateFull(
						firstModificationMethod: () => {
							if( !info.ArticleId.HasValue ) {
								mod.ArticleId = MainSequence.GetNextValue();
								mod.Slug = getSuffixedSlug( mod.Title.ToUrlSlug() );
								mod.CreationDateAndTime = DateTime.UtcNow;
							}
							mod.Execute();

							if( info.ArticleId.HasValue )
								ArticleTagsModification.DeleteRows( new ArticleTagsTableEqualityConditions.ArticleId( info.ArticleId.Value ) );
							foreach( var i in tagIds )
								ArticleTagsModification.InsertRow( mod.ArticleId, i );
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
							value: info.ArticleId.HasValue ? null : "" ),
						getTagFormItem( tagIds ) );

					ph.AddControlsReturnThis( table );
					EwfUiStatics.SetContentFootActions( new ActionButtonSetup( "Publish Article", new PostBackButton() ) );
				} );
		}

		private string getSuffixedSlug( string slug ) {
			var otherArticles = ArticlesRetrieval.GetRowsOrderedByCreation();
			for( var suffix = 1;; suffix += 1 ) {
				var suffixedSlug = slug + ( suffix == 1 ? "" : "-{0}".FormatWith( suffix.ToString() ) );
				if( otherArticles.All( i => i.Slug != suffixedSlug ) )
					return suffixedSlug;
			}
		}

		private FormItem getTagFormItem( IEnumerable<int> tagIds ) {
			var rs = new UpdateRegionSet();
			var tagName = new DataValue<string>();
			var removeRs = new UpdateRegionSet();
			return FormItem.Create(
				"Enter tags",
				new PlaceHolder().AddControlsReturnThis(
					new NamingPlaceholder(
							FormState.ExecuteWithDataModificationsAndDefaultAction(
									PostBack.CreateIntermediate(
											rs.ToCollection(),
											id: "addTag",
											firstModificationMethod: () => {
												var tagId = TagsTableRetrieval.GetAllRows().MatchingName( tagName.Value )?.TagId;
												if( !tagId.HasValue ) {
													tagId = MainSequence.GetNextValue();
													TagsModification.InsertRow( tagId.Value, tagName.Value );
												}

												if( !tagIds.Contains( tagId.Value ) )
													setTags( tagIds.Append( tagId.Value ).ToArray() );
											} )
										.ToCollection(),
									() => new TextControl(
										"",
										false,
										maxLength: TagsTable.TagNameColumn.Size,
										validationMethod: ( postBackValue, validator ) => tagName.Value = postBackValue ) )
								.ToFormItem()
								.ToControl()
								.ToCollection(),
							updateRegionSets: rs.ToCollection() ).ToCollection()
						.Concat(
							new LineBreak().ToCollection<PhrasingComponent>()
								.Append(
									new PhrasingIdContainer(
										tagIds.Select(
												tagId => new GenericPhrasingContainer(
													new EwfButton(
															new CustomButtonStyle( children: new FontAwesomeIcon( "fa-times" ).ToCollection() ),
															behavior: new PostBackBehavior(
																postBack: PostBack.CreateIntermediate(
																	removeRs.ToCollection(),
																	id: PostBack.GetCompositeId( "removeTag", tagId.ToString() ),
																	firstModificationMethod: () => setTags( tagIds.Where( i => i != tagId ).ToArray() ) ) ) ).ToCollection()
														.Concat( " {0}".FormatWith( TagsTableRetrieval.GetRowMatchingId( tagId ).TagName ).ToComponents() )
														.Materialize(),
													classes: ElementClasses.EditorTag ) )
											.Materialize(),
										updateRegionSets: rs.ToCollection().Append( removeRs ) ) )
								.GetControls() ) ) );
		}
	}
}