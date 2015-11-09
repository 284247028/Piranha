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
using System.Web;

using Piranha.Entities;

namespace Piranha.Areas.Manager.Models
{
	/// <summary>
	/// View model for the post list view.
	/// </summary>
	public class PostListModel
	{
		#region Inner classes
		/// <summary>
		/// Internal post status.
		/// </summary>
		public enum PostStatus
		{
			PUBLISHED, UNPUBLISHED, DRAFT
		}

		/// <summary>
		/// Internal post view model.
		/// </summary>
		public class PostModel
		{
			/// <summary>
			/// Gets/sets the id of the post.
			/// </summary>
			public Guid Id { get; set; }

			/// <summary>
			/// Gets/sets the title of the post.
			/// </summary>
			public string Title { get; set; }

			/// <summary>
			/// Gets/sets the name of the post type.
			/// </summary>
			public string TemplateName { get; set; }

			/// <summary>
			/// Gets/sets the current status of the post.
			/// </summary>
			public PostStatus Status { get; set; }

			/// <summary>
			/// Gets/sets the number of new comments.
			/// </summary>
			public int NewComments { get; set; }

			/// <summary>
			/// Gets/sets the date the post was created.
			/// </summary>
			public DateTime Created { get; set; }

			/// <summary>
			/// Gets/sets the date the post was last updated.
			/// </summary>
			public DateTime Updated { get; set; }
		}

		/// <summary>
		/// Internal post template view model.
		/// </summary>
		public class TemplateModel
		{
			/// <summary>
			/// Gets/sets the Id.
			/// </summary>
			public Guid Id { get; set; }

			/// <summary>
			/// Gets/sets the name.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Gets/sets the description.
			/// </summary>
			public string Description { get; set; }

			/// <summary>
			/// Gets/sets the html preview of the template.
			/// </summary>
			public string Preview { get; set; }
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets/sets the currently available posts.
		/// </summary>
		public List<PostModel> Posts { get; set; }

		/// <summary>
		/// Gets/sets the currently available post templates.
		/// </summary>
		public List<TemplateModel> Templates { get; set; }

		/// <summary>
		/// Gets/sets the currently active post template.
		/// </summary>
		public Guid ActiveTemplate { get; set; }
		#endregion

		/// <summary>
		/// Default constructor.
		/// </summary>
		public PostListModel() {
			Posts = new List<PostModel>();
			Templates = new List<TemplateModel>();
		}

		/// <summary>
		/// Gets the post list model.
		/// </summary>
		/// <returns>The model</returns>
		public static PostListModel Get() {
			return Get(Guid.Empty);
		}


		/// <summary>
		/// Gets the post list model for the given post type id.
		/// </summary>
		/// <param name="templateid">The post type id</param>
		/// <returns>The model</returns>
		public static PostListModel GetByTemplateId(Guid templateid) {
			return Get(templateid);
		}

		#region Private methods
		/// <summary>
		/// Gets the model for the given parameters.
		/// </summary>
		/// <param name="templateid">The post type id</param>
		/// <returns>The model</returns>
		private static PostListModel Get(Guid templateid) {
			var m = new PostListModel();

			using (var db = new DataContext()) {
				// Get the posts
				var query = db.PostDrafts.Include(p => p.Template);
				if (templateid != Guid.Empty) {
					query = query.Where(p => p.TemplateId == templateid);
					m.ActiveTemplate = templateid;
				}
				m.Posts = query.
					OrderBy(p => p.Title).ToList().
					Select(p => new PostModel() {
						Id = p.Id,
						Title = p.Title,
						TemplateName = p.Template.Name,
						Status = (!p.LastPublished.HasValue ? PostStatus.UNPUBLISHED : (p.Updated > p.LastPublished ? PostStatus.DRAFT : PostStatus.PUBLISHED)),
						NewComments = db.Comments.Where(c => c.ParentId == p.Id && c.InternalStatus == 0).Count(),
						Created = p.Created,
						Updated = p.Updated
					}).ToList();

				// Get the templates
				m.Templates = db.PostTemplates.
					OrderBy(t => t.Name).
					Select(t => new TemplateModel() {
						Id = t.Id,
						Name = t.Name,
						Description = t.Description,
						Preview = t.Preview
					}).ToList();
			}
			return m;
		}
		#endregion
	}
}