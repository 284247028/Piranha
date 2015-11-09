/*
 * Copyright (c) 2011-2015 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 * 
 * http://github.com/piranhacms/piranha
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

using Piranha.Data;

namespace Piranha.Models
{
	/// <summary>
	/// Active record for a post.
	/// 
	/// Changes made to records of this type are logged.
	/// </summary>
	[PrimaryKey(Column = "post_id,post_draft")]
	[Join(TableName = "posttemplate", ForeignKey = "post_template_id", PrimaryKey = "posttemplate_id")]
	[Join(TableName = "permalink", ForeignKey = "post_permalink_id", PrimaryKey = "permalink_id")]
	[Serializable]
	public class Post : DraftRecord<Post>, IPost, ICacheRecord<Post>
	{
		#region Fields
		/// <summary>
		/// Gets/sets the id.
		/// </summary>
		[Column(Name = "post_id")]
		public override Guid Id { get; set; }

		/// <summary>
		/// Gets/sets whether this is a draft.
		/// </summary>
		[Column(Name = "post_draft")]
		public override bool IsDraft { get; set; }

		/// <summary>
		/// Gets/sets the template id.
		/// </summary>
		[Column(Name = "post_template_id")]
		public Guid TemplateId { get; set; }

		/// <summary>
		/// Gets/sets the permalink id.
		/// </summary>
		[Column(Name = "post_permalink_id")]
		public Guid PermalinkId { get; set; }

		/// <summary>
		/// Gets/sets if this post should be included in rss feeds.
		/// </summary>
		[Column(Name = "post_rss")]
		[Display(ResourceType = typeof(Piranha.Resources.Post), Name = "AllowRss")]
		public bool AllowRss { get; set; }

		/// <summary>
		/// Gets/sets the title.
		/// </summary>
		[Column(Name = "post_title")]
		[Display(ResourceType = typeof(Piranha.Resources.Post), Name = "Title")]
		[Required(ErrorMessageResourceType = typeof(Piranha.Resources.Post), ErrorMessageResourceName = "TitleRequired")]
		[StringLength(128, ErrorMessageResourceType = typeof(Piranha.Resources.Post), ErrorMessageResourceName = "TitleLength")]
		public string Title { get; set; }

		/// <summary>
		/// Gets/sets the permalink.
		/// </summary>
		[Column(Name = "permalink_name", ReadOnly = true, Table = "permalink")]
		[Display(ResourceType = typeof(Piranha.Resources.Post), Name = "Permalink")]
		public string Permalink { get; set; }

		/// <summary>
		/// Gets/sets the meta keywords.
		/// </summary>
		[Column(Name = "post_keywords")]
		[Display(ResourceType = typeof(Piranha.Resources.Post), Name = "Keywords")]
		[StringLength(255, ErrorMessageResourceType = typeof(Piranha.Resources.Post), ErrorMessageResourceName = "KeywordsLength")]
		public string Keywords { get; set; }

		/// <summary>
		/// Gets/sets the meta description.
		/// </summary>
		[Column(Name = "post_description")]
		[Display(ResourceType = typeof(Piranha.Resources.Post), Name = "Description")]
		[StringLength(255, ErrorMessageResourceType = typeof(Piranha.Resources.Post), ErrorMessageResourceName = "DescriptionLength")]
		public string Description { get; set; }

		/// <summary>
		/// Gets/sets the excerpt.
		/// </summary>
		[Column(Name = "post_excerpt")]
		[Display(ResourceType = typeof(Piranha.Resources.Post), Name = "Excerpt")]
		[StringLength(255, ErrorMessageResourceType = typeof(Piranha.Resources.Post), ErrorMessageResourceName = "ExcerptLength")]
		public string Excerpt { get; set; }

		/// <summary>
		/// Gets/sets the body.
		/// </summary>
		[Column(Name = "post_body")]
		public HtmlString Body { get; set; }

		/// <summary>
		/// Gets/sets the template name.
		/// </summary>
		[Column(Name = "posttemplate_name", Table = "posttemplate")]
		public string TemplateName { get; private set; }

		/// <summary>
		/// Gets/sets the attachments.
		/// </summary>
		[Column(Name = "post_attachments", Json = true)]
		public List<Guid> Attachments { get; set; }

		/// <summary>
		/// Gets/sets the post controller.
		/// </summary>
		[Column(Name = "post_controller")]
		[Display(ResourceType = typeof(Piranha.Resources.Post), Name = "Template")]
		[StringLength(128, ErrorMessageResourceType = typeof(Piranha.Resources.Page), ErrorMessageResourceName = "TemplateLength")]
		public string PostController { get; set; }

		/// <summary>
		/// Gets/sets the template controller.
		/// </summary>
		[Column(Name = "posttemplate_controller", ReadOnly = true, Table = "posttemplate")]
		public string TemplateController { get; private set; }

		/// <summary>
		/// Gets/sets the post controller.
		/// </summary>
		[Column(Name = "post_view")]
		[Display(ResourceType = typeof(Piranha.Resources.Post), Name = "View")]
		[StringLength(128, ErrorMessageResourceType = typeof(Piranha.Resources.Post), ErrorMessageResourceName = "ViewLength")]
		public string PostView { get; set; }

		/// <summary>
		/// Gets/sets the template controller.
		/// </summary>
		[Column(Name = "posttemplate_view", ReadOnly = true, Table = "posttemplate")]
		public string TemplateView { get; private set; }
		/// <summary>
		/// Gets/sets the created date.
		/// </summary>
		[Column(Name = "post_created")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public override DateTime Created { get; set; }

		/// <summary>
		/// Gets/sets the updated date.
		/// </summary>
		[Column(Name = "post_updated")]
		public override DateTime Updated { get; set; }

		/// <summary>
		/// Gets/sets the published date.
		/// </summary>
		[Column(Name = "post_published")]
		public override DateTime Published { get; set; }

		/// <summary>
		/// Gets/sets the published date.
		/// </summary>
		[Column(Name = "post_last_published")]
		public override DateTime LastPublished { get; set; }

		/// <summary>
		/// Gets/sets the last modified date.
		/// </summary>
		[Column(Name = "post_last_modified")]
		public override DateTime LastModified { get; set; }

		/// <summary>
		/// Gets/sets the user id that created the record.
		/// </summary>
		[Column(Name = "post_created_by")]
		public override Guid CreatedBy { get; set; }

		/// <summary>
		/// Gets/sets the user id that created the record.
		/// </summary>
		[Column(Name = "post_updated_by")]
		public override Guid UpdatedBy { get; set; }

		[Column(Name = "site")]
		public string Site { get; set; }
		#endregion

		#region Properties
		/// <summary>
		/// Gets the controller for the post.
		/// </summary>
		public string Controller {
			get { return !String.IsNullOrEmpty(PostController) ? PostController : TemplateController; }
		}

		/// <summary>
		/// Gets the view for the post.
		/// </summary>
		public string View {
			get { return !String.IsNullOrEmpty(PostView) ? PostView : TemplateView; }
		}
		#endregion

		#region Cache
		/// <summary>
		/// Maps permalink to post id.
		/// </summary>
		private static Dictionary<string, Guid> PermalinkCache = new Dictionary<string, Guid>();

		/// <summary>
		/// Maps permalink id to post id.
		/// </summary>
		private static Dictionary<Guid, Guid> PermalinkIdCache = new Dictionary<Guid, Guid>();
		#endregion

		/// <summary>
		/// Default constructor. Creates a new post.
		/// </summary>
		public Post()
			: base() {
			ExtensionType = Extend.ExtensionType.Post;
			IsDraft = true;
			Attachments = new List<Guid>();
			LogChanges = true;
		}

		#region Static accessors
		/// <summary>
		/// Gets a single post.
		/// </summary>
		/// <param name="id">The post id</param>
		/// <returns>The post</returns>
		public static Post GetSingle(Guid id) {
			if (!Application.Current.CacheProvider.Contains(id.ToString())) {
				var p = Post.GetSingle("post_id = @0 AND post_draft = 0", id);

				if (p != null)
					AddToCache(p);
				else return null;
			}
			return (Post)Application.Current.CacheProvider[id.ToString()];
		}

		/// <summary>
		/// Gets a single post from the database.
		/// </summary>
		/// <param name="id">The post id</param>
		/// <param name="draft">Whether to get the draft or not</param>
		/// <returns>The post</returns>
		public static Post GetSingle(Guid id, bool draft) {
			if (!draft)
				return GetSingle(id);
			return GetSingle("post_id = @0 AND post_draft = @1", id, draft);
		}

		/// <summary>
		/// Gets the post specified by the given permalink.
		/// </summary>
		/// <param name="permalink">The permalink</param>
		/// <returns>The post</returns>
		public static Post GetByPermalink(string permalink, bool draft = false) {
			if (!draft) {
				if (!PermalinkCache.ContainsKey(permalink.ToLower())) {
					var p = Post.GetSingle("permalink_name = @0 AND post_draft = @1", permalink, draft);

					if (p != null)
						AddToCache(p);
				}
				if (!Application.Current.CacheProvider.Contains(PermalinkCache[permalink.ToLower()].ToString()))
					Application.Current.CacheProvider[PermalinkCache[permalink.ToLower()].ToString()] = Post.GetSingle("permalink_name = @0 AND post_draft = @1", permalink, draft);
				return (Post)Application.Current.CacheProvider[PermalinkCache[permalink.ToLower()].ToString()];
			}
			return Post.GetSingle("permalink_name = @0 AND post_draft = @1", permalink, draft);
		}

		/// <summary>
		/// Gets the post specified by the given permalink id.
		/// </summary>
		/// <param name="permalinkid">The permalink id</param>
		/// <param name="draft">Whether to get the draft or not</param>
		/// <returns>The post</returns>
		public static Post GetByPermalinkId(Guid permalinkid, bool draft = false) {
			if (!draft) {
				if (!PermalinkIdCache.ContainsKey(permalinkid)) {
					var p = Post.GetSingle("post_permalink_id = @0 AND post_draft = @1", permalinkid, draft);

					if (p != null)
						AddToCache(p);
				}
				if (!Application.Current.CacheProvider.Contains(PermalinkIdCache[permalinkid].ToString()))
					Application.Current.CacheProvider[PermalinkIdCache[permalinkid].ToString()] = Post.GetSingle("post_permalink_id = @0 AND post_draft = @1", permalinkid, draft);
				return (Post)Application.Current.CacheProvider[PermalinkIdCache[permalinkid].ToString()];
			}
			return Post.GetSingle("post_permalink_id = @0 AND post_draft = @1", permalinkid, draft);
		}

		/// <summary>
		/// Gets the posts for the given category id.
		/// </summary>
		/// <param name="id">The category id</param>
		/// <returns>A list of posts</returns>
		public static List<Post> GetByCategoryId(Guid id) {
			return GetFieldsByCategoryId("*", id);
		}

		/// <summary>
		/// Gets the posts for the given category id.
		/// </summary>
		/// <param name="fields">The fields</param>
		/// <param name="id">The category id</param>
		/// <returns>A list of posts</returns>
		public static List<Post> GetFieldsByCategoryId(string fields, Guid id) {
			return Post.GetFields(fields, "post_draft = 0 AND post_id IN (" +
				"SELECT relation_data_id FROM relation WHERE relation_type = @0 AND relation_related_id = @1)",
				Relation.RelationType.POSTCATEGORY, id, new Params() { OrderBy = "post_published DESC" });
		}

		/// <summary>
		/// Gets the posts for the given category name.
		/// </summary>
		/// <param name="name">The category name</param>
		/// <returns>A list of posts</returns>
		public static List<Post> GetByCategoryName(string name) {
			return Post.Get("post_draft = 0 AND post_id IN (" +
				"SELECT relation_data_id FROM relation WHERE relation_type = @0 AND relation_related_id = (" +
				"SELECT category_id FROM category WHERE category_name = @1))", Relation.RelationType.POSTCATEGORY, name);
		}

		public static List<Post> GetByTemplateId(Guid id) {
			return Post.Get("post_draft = 0 AND post_template_id = @0", id);
		}
		#endregion

		/// <summary>
		/// Adds the given post to the cache.
		/// </summary>
		/// <param name="p">The post</param>
		private static void AddToCache(Post p) {
			Application.Current.CacheProvider[p.Id.ToString()] = p;
			PermalinkCache[p.Permalink] = p.Id;
			PermalinkIdCache[p.PermalinkId] = p.Id;

		}

		#region Handlers
		/// <summary>
		/// Create an empty attachment list if it is null in the database.
		/// </summary>
		/// <param name="lst">The attachments</param>
		/// <returns>The attachments, or a default list</returns>
		protected List<Guid> OnAttachmentsLoad(List<Guid> lst) {
			if (lst != null)
				return lst;
			return new List<Guid>();
		}
		#endregion

		/// <summary>
		/// Invalidates the current record from the cache.
		/// </summary>
		/// <param name="record">The record</param>
		public void InvalidateRecord(Post record) {
			Application.Current.CacheProvider.Remove(record.Id.ToString());

			// If we click save & publish right away the permalink is not created yet.
			if (record.Permalink != null && PermalinkCache.ContainsKey(record.Permalink))
				PermalinkCache.Remove(record.Permalink);
			if (record.Permalink != null && PermalinkIdCache.ContainsKey(record.PermalinkId))
				PermalinkIdCache.Remove(record.PermalinkId);
		}
	}
}
