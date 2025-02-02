﻿using EnterpriseWebLibrary.UserManagement;
using EwlRealWorld.Library;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.CommandConditions;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.Retrieval;
using EwlRealWorld.Library.DataAccess.TableConstants;
using EwlRealWorld.Library.DataAccess.TableRetrieval;

namespace EwlRealWorld.Website.Pages;

// EwlPage
// Parameter: int? articleId
partial class Editor {
	private ArticlesRetrieval.Row? article;

	protected override void init() {
		if( ArticleId.HasValue )
			article = ArticlesRetrieval.GetRowMatchingId( ArticleId.Value );
	}

	protected override bool userCanAccess => AppTools.User != null && ( !ArticleId.HasValue || article!.AuthorId == AppTools.User.UserId );

	protected override UrlHandler getUrlParent() => new Home();

	protected override PageContent getContent() {
		var mod = getMod();
		var tagIds = ComponentStateItem.Create(
			"tags",
			ArticleId.HasValue
				? ArticleTagsTableRetrieval.GetRowsLinkedToArticle( ArticleId.Value ).Select( i => i.TagId ).Materialize()
				: Enumerable.Empty<int>().Materialize(),
			v => v.All( id => TagsTableRetrieval.TryGetRowMatchingId( id, out _ ) ),
			true );
		return FormState.ExecuteWithActions(
			PostBack.CreateFull(
				modificationMethod: () => {
					if( !ArticleId.HasValue ) {
						mod.ArticleId = MainSequence.GetNextValue();
						mod.Slug = getSuffixedSlug( mod.Title.ToUrlSlug() );
						mod.CreationDateAndTime = DateTime.UtcNow;
					}
					mod.Execute();

					if( ArticleId.HasValue )
						ArticleTagsModification.DeleteRows( new ArticleTagsTableEqualityConditions.ArticleId( ArticleId.Value ) );
					foreach( var i in tagIds.Value )
						ArticleTagsModification.InsertRow( mod.ArticleId, i );
				},
				actionGetter: () => new PostBackAction( Article.GetInfo( mod.ArticleId ) ) ),
			() => {
				var stack = FormItemList.CreateStack( generalSetup: new FormItemListSetup( etherealContent: tagIds.ToCollection() ) );

				stack.AddItems(
					mod.GetTitleTextControlFormItem( false, label: "Article title".ToComponents(), value: ArticleId.HasValue ? null : "" )
						.Append( mod.GetDescriptionTextControlFormItem( false, label: "What's this article about?".ToComponents(), value: ArticleId.HasValue ? null : "" ) )
						.Append(
							mod.GetBodyMarkdownTextControlFormItem(
								false,
								label: "Write your article (in markdown)".ToComponents(),
								controlSetup: TextControlSetup.Create( numberOfRows: 8 ),
								value: ArticleId.HasValue ? null : "" ) )
						.Append( getTagFormItem( tagIds ) )
						.Materialize() );

				return new UiPageContent( contentFootActions: new ButtonSetup( "Publish Article" ).ToCollection() ).Add( stack );
			} );
	}

	private ArticlesModification getMod() {
		if( ArticleId.HasValue )
			return article!.ToModification();

		var mod = ArticlesModification.CreateForInsert();
		mod.AuthorId = SystemUser.Current!.UserId;
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

	private FormItem getTagFormItem( AbstractDataValue<IReadOnlyCollection<int>> tagIds ) {
		var addUpdateRegions = new UpdateRegionSet();
		var tagName = new DataValue<string>( false );
		var removeUpdateRegions = new UpdateRegionSet();
		return new FlowIdContainer(
				FormState.ExecuteWithActions(
						PostBack.CreateIntermediate( addUpdateRegions, id: "addTag", modificationMethod: () => addTag( tagIds, tagName ) ),
						() => new TextControl(
							"",
							false,
							maxLength: TagsTable.TagNameColumn.Size,
							validationMethod: ( postBackValue, _ ) => tagName.Value = postBackValue ) )
					.ToFormItem()
					.ToComponentCollection(),
				updateRegionSets: addUpdateRegions ).Append<FlowComponent>( new LineBreak() )
			.Append( new PhrasingIdContainer( getTagListComponents( tagIds, removeUpdateRegions ), updateRegionSets: addUpdateRegions.Add( removeUpdateRegions ) ) )
			.Materialize()
			.ToFormItem( label: "Enter tags".ToComponents() );
	}

	private void addTag( AbstractDataValue<IReadOnlyCollection<int>> tagIds, DataValue<string> tagName ) {
		var tagId = TagsTableRetrieval.GetAllRows().MatchingName( tagName.Value )?.TagId;
		if( !tagId.HasValue ) {
			tagId = MainSequence.GetNextValue();
			TagsModification.InsertRow( tagId.Value, tagName.Value );
		}

		if( !tagIds.Value.Contains( tagId.Value ) )
			tagIds.Value = tagIds.Value.Append( tagId.Value ).Materialize();
	}

	private IReadOnlyCollection<PhrasingComponent>
		getTagListComponents( AbstractDataValue<IReadOnlyCollection<int>> tagIds, UpdateRegionSet removeUpdateRegions ) =>
		tagIds.Value.Select(
				tagId => new GenericPhrasingContainer(
					new EwfButton(
							new CustomButtonStyle( children: new FontAwesomeIcon( "fa-times" ).ToCollection() ),
							behavior: new PostBackBehavior(
								postBack: PostBack.CreateIntermediate(
									removeUpdateRegions,
									id: PostBack.GetCompositeId( "removeTag", tagId.ToString() ),
									modificationMethod: () => tagIds.Value = tagIds.Value.Where( i => i != tagId ).Materialize() ) ) )
						.Concat( " {0}".FormatWith( TagsTableRetrieval.GetRowMatchingId( tagId ).TagName ).ToComponents() )
						.Materialize(),
					classes: ElementClasses.EditorTag ) )
			.Materialize();
}