using System;
using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
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
			var mod = getMod();
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
					var stack = FormItemList.CreateStack();

					stack.AddItems(
						mod.GetTitleTextControlFormItem( false, label: "Article title".ToComponents(), value: info.ArticleId.HasValue ? null : "" )
							.Append(
								mod.GetDescriptionTextControlFormItem( false, label: "What's this article about?".ToComponents(), value: info.ArticleId.HasValue ? null : "" ) )
							.Append(
								mod.GetBodyMarkdownTextControlFormItem(
									false,
									label: "Write your article (in markdown)".ToComponents(),
									controlSetup: TextControlSetup.Create( numberOfRows: 8 ),
									value: info.ArticleId.HasValue ? null : "" ) )
							.Append( getTagFormItem( tagIds ) )
							.Materialize() );

					ph.AddControlsReturnThis( stack.ToCollection().GetControls() );
					EwfUiStatics.SetContentFootActions( new ButtonSetup( "Publish Article" ).ToCollection() );
				} );
		}

		private ArticlesModification getMod() {
			if( info.ArticleId.HasValue )
				return info.Article.ToModification();

			var mod = ArticlesModification.CreateForInsert();
			mod.AuthorId = AppTools.User.UserId;
			return mod;
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
			var addUpdateRegions = new UpdateRegionSet();
			var tagName = new DataValue<string>();
			var removeUpdateRegions = new UpdateRegionSet();
			return new FlowIdContainer(
					FormState.ExecuteWithDataModificationsAndDefaultAction(
							PostBack.CreateIntermediate( addUpdateRegions.ToCollection(), id: "addTag", firstModificationMethod: () => addTag( tagIds, tagName ) )
								.ToCollection(),
							() => new TextControl(
								"",
								false,
								maxLength: TagsTable.TagNameColumn.Size,
								validationMethod: ( postBackValue, validator ) => tagName.Value = postBackValue ) )
						.ToFormItem()
						.ToComponentCollection(),
					updateRegionSets: addUpdateRegions.ToCollection() ).Append<FlowComponent>( new LineBreak() )
				.Append(
					new PhrasingIdContainer(
						getTagListComponents( tagIds, removeUpdateRegions ),
						updateRegionSets: addUpdateRegions.ToCollection().Append( removeUpdateRegions ) ) )
				.Materialize()
				.ToFormItem( label: "Enter tags".ToComponents() );
		}

		private void addTag( IEnumerable<int> tagIds, DataValue<string> tagName ) {
			var tagId = TagsTableRetrieval.GetAllRows().MatchingName( tagName.Value )?.TagId;
			if( !tagId.HasValue ) {
				tagId = MainSequence.GetNextValue();
				TagsModification.InsertRow( tagId.Value, tagName.Value );
			}

			if( !tagIds.Contains( tagId.Value ) )
				setTags( tagIds.Append( tagId.Value ).ToArray() );
		}

		private IReadOnlyCollection<PhrasingComponent> getTagListComponents( IEnumerable<int> tagIds, UpdateRegionSet removeUpdateRegions ) =>
			tagIds.Select(
					tagId => new GenericPhrasingContainer(
						new EwfButton(
								new CustomButtonStyle( children: new FontAwesomeIcon( "fa-times" ).ToCollection() ),
								behavior: new PostBackBehavior(
									postBack: PostBack.CreateIntermediate(
										removeUpdateRegions.ToCollection(),
										id: PostBack.GetCompositeId( "removeTag", tagId.ToString() ),
										firstModificationMethod: () => setTags( tagIds.Where( i => i != tagId ).ToArray() ) ) ) )
							.Concat( " {0}".FormatWith( TagsTableRetrieval.GetRowMatchingId( tagId ).TagName ).ToComponents() )
							.Materialize(),
						classes: ElementClasses.EditorTag ) )
				.Materialize();
	}
}