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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Piranha.Entities
{
	/// <summary>
	/// The post entity.
	/// </summary>
	[Serializable]
	public class Post : BaseEntity
	{
		#region Properties
		/// <summary>
		/// Gets/sets the unique post id.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets/sets whether this post instance is a draft or not.
		/// </summary>
		internal bool IsDraft { get; set; }

		/// <summary>
		/// Gets/sets the id of the post template.
		/// </summary>
		public Guid TemplateId { get; set; }

		/// <summary>
		/// Gets/sets the id of the permalink.
		/// </summary>
		public Guid PermalinkId { get; set; }

		/// <summary>
		/// Gets/sets whether to include this post in rss feeds.
		/// </summary>
		public bool AllowRss { get; set; }

		/// <summary>
		/// Gets/sets the post title.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Gets/sets the post excerpt.
		/// </summary>
		public string Excerpt { get; set; }

		/// <summary>
		/// Gets/sets the main body.
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// Gets/sets the meta keywords.
		/// </summary>
		public string Keywords { get; set; }

		/// <summary>
		/// Gets/sets the meta description.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets/sets the id's of content attached to this post.
		/// </summary>
		public IList<Guid> Attachments { get; set; }

		/// <summary>
		/// Gets/sets the optional template that should render the view.
		/// </summary>
		public string ViewTemplate { get; set; }

		/// <summary>
		/// Gets/sets the optional template that should render the archive view.
		/// </summary>
		public string ViewArchiveTemplate { get; set; }

		/// <summary>
		/// Gets/sets the date the post was initially created.
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// Gets/sets the date the post was last updated.
		/// </summary>
		public DateTime Updated { get; set; }

		/// <summary>
		/// Gets/sets the date the post was initially published.
		/// </summary>
		public DateTime? Published { get; set; }

		/// <summary>
		/// Gets/sets the date the post was last updated.
		/// </summary>
		public DateTime? LastPublished { get; set; }

		/// <summary>
		/// Gets/set the last modified date.
		/// </summary>
		public DateTime? LastModified { get; set; }

		/// <summary>
		/// Gets/sets the id of the user who initially created the post.
		/// </summary>
		public Guid CreatedById { get; set; }

		/// <summary>
		/// Gets/sets the id of the user who last updated the post.
		/// </summary>
		public Guid UpdatedById { get; set; }

		public string Site { get; set; }
		#endregion

		#region Navigation properties
		/// <summary>
		/// Gets/sets the post template.
		/// </summary>
		public PostTemplate Template { get; set; }

		/// <summary>
		/// Gets/sets the permalink used to access the post.
		/// </summary>
		public Permalink Permalink { get; set; }

		/// <summary>
		/// Gets/sets the properties attached to the post.
		/// </summary>
		public IList<Property> Properties { get; set; }

		/// <summary>
		/// Gets/sets the categories attached to the post.
		/// </summary>
		public IList<Category> Categories { get; set; }

		/// <summary>
		/// Gets/sets the currently available extensions.
		/// </summary>
		public IList<Extension> Extensions { get; set; }

		/// <summary>
		/// Gets/sets the currently available comments.
		/// </summary>
		public IList<Comment> Comments { get; set; }

		/// <summary>
		/// Gets/sets the user who initially created the template.
		/// </summary>
		public User CreatedBy { get; set; }

		/// <summary>
		/// Gets/sets the user who last updated the template.
		/// </summary>
		public User UpdatedBy { get; set; }
		#endregion

		#region Internal properties
		/// <summary>
		/// Gets/sets the internal json data for the attachments.
		/// </summary>
		internal string AttachmentsJson { get; set; }
		#endregion

		/// <summary>
		/// Default constructor. Creates a new post entity.
		/// </summary>
		public Post() {
			Attachments = new List<Guid>();
			Categories = new List<Category>();
			Extensions = new List<Extension>();
			Comments = new List<Comment>();
		}

		/// <summary>
		/// Gets the attachments for the current post.
		/// </summary>
		/// <returns>The attachments</returns>
		public IList<Media> GetAttachments() {
			using (var db = new DataContext()) {
				return db.Media.Where(m => Attachments.Contains(m.Id)).ToList();
			}
		}

		#region Events
		/// <summary>
		/// Called when the entity has been loaded.
		/// </summary>
		/// <param name="db">The db context</param>
		public override void OnLoad(DataContext db) {
			var js = new JavaScriptSerializer();
			Attachments = !String.IsNullOrEmpty(AttachmentsJson) ? js.Deserialize<List<Guid>>(AttachmentsJson) : Attachments;

			base.OnLoad(db);
		}

		/// <summary>
		/// Called when the entity is about to be saved.
		/// </summary>
		/// <param name="db">The db context</param>
		/// <param name="state">The current entity state</param>
		public override void OnSave(DataContext db, EntityState state) {
			var js = new JavaScriptSerializer();
			AttachmentsJson = js.Serialize(Attachments);

			base.OnSave(db, state);
		}
		#endregion
	}
}
