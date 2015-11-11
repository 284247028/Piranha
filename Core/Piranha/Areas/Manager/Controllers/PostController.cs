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
using System.Web.Mvc;

using Piranha.Models;
using Piranha.Models.Manager.PostModels;

namespace Piranha.Areas.Manager.Controllers
{
	/// <summary>
	/// Manager controller for handling posts.
	/// </summary>
	public class PostController : ManagerController
	{
		/// <summary>
		/// Default constructor. Gets the post list.
		/// </summary>
		[Access(Function = "ADMIN_POST")]
		public ActionResult Index() {
			var activeSite = Session["activeSite"] as string;
			if (activeSite != null)
			{
				return Site(activeSite);
			}
			var m = Models.PostListModel.Get();
			ViewBag.Title = @Piranha.Resources.Post.ListTitle;

			// Executes the post list loaded hook, if registered
			if (WebPages.Hooks.Manager.PostListModelLoaded != null)
				WebPages.Hooks.Manager.PostListModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), m);

			return View(@"~/Areas/Manager/Views/Post/Index.cshtml", m);
		}


		/// <summary>
		/// Gets the site tree with the given id.
		/// </summary>
		/// <param name="id">The internal id of the site tree</param>
		[Access(Function = "ADMIN_PAGE")]
		public ActionResult Site(string id)
		{
			Session["activeSite"] = id.ToUpper();
			var m = Models.PostListModel.Get(id);
			ViewBag.Title = @Piranha.Resources.Post.ListTitle;

			// Executes the post list loaded hook, if registered
			if (WebPages.Hooks.Manager.PostListModelLoaded != null)
				WebPages.Hooks.Manager.PostListModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), m);

			return View(@"~/Areas/Manager/Views/Post/Index.cshtml", m);
		}
		/// <summary>
		/// Gets the post list for the specified post template.
		/// </summary>
		/// <param name="id">The post template id</param>
		[Access(Function = "ADMIN_POST")]
		public ActionResult Template(string id) {
			var m = Models.PostListModel.GetByTemplateId(new Guid(id));
			ViewBag.Title = @Piranha.Resources.Post.ListTitle;

			// Executes the post list loaded hook, if registered
			if (WebPages.Hooks.Manager.PostListModelLoaded != null)
				WebPages.Hooks.Manager.PostListModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), m);

			return View(@"~/Areas/Manager/Views/Post/Index.cshtml", m);
		}

		/// <summary>
		/// Creates a new post.
		/// </summary>
		/// <param name="im">The insert model</param>
		[HttpPost()]
		[Access(Function = "ADMIN_POST")]
		public ActionResult Insert(InsertModel im) {
			EditModel pm = EditModel.CreateByTemplate(im.TemplateId);

			ViewBag.Title = Piranha.Resources.Post.EditTitleNew + pm.Template.Name.ToLower();

			// Executes the post edit loaded hook, if registered
			if (WebPages.Hooks.Manager.PostEditModelLoaded != null)
				WebPages.Hooks.Manager.PostEditModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), pm);

			return View(@"~/Areas/Manager/Views/Post/Edit.cshtml", pm);
		}

		/// <summary>
		/// Edits the post with the given id.
		/// </summary>
		/// <param name="id">The post id</param>
		[Access(Function = "ADMIN_POST")]
		public ActionResult Edit(string id) {
			EditModel m = EditModel.GetById(new Guid(id));

			if (!String.IsNullOrEmpty(Request["action"])) {
				if (Request["action"].ToLower() == "attachments") {
					m.Action = EditModel.ActionType.ATTACHMENTS;
				}
			}
			ViewBag.Title = Piranha.Resources.Post.EditTitleExisting;

			// Executes the post edit loaded hook, if registered
			if (WebPages.Hooks.Manager.PostEditModelLoaded != null)
				WebPages.Hooks.Manager.PostEditModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), m);

			return View(@"~/Areas/Manager/Views/Post/Edit.cshtml", m);
		}

		/// <summary>
		/// Saves the model.
		/// </summary>
		/// <param name="m">The model</param>
		[HttpPost(), ValidateInput(false)]
		[Access(Function = "ADMIN_POST")]
		public ActionResult Edit(bool draft, EditModel m) {
			if (ModelState.IsValid) {
				try {
					m.Post.Site = ((String)Session["activeSite"]).ToUpper();
					// Executes the post edit before save hook, if registered
					if (WebPages.Hooks.Manager.PostEditModelBeforeSave != null)
						WebPages.Hooks.Manager.PostEditModelBeforeSave(this, WebPages.Manager.GetActiveMenuItem(), m, !draft);

					if (m.SaveAll(draft)) {
						// Executes the post edit after save hook, if registered
						if (WebPages.Hooks.Manager.PostEditModelAfterSave != null)
							WebPages.Hooks.Manager.PostEditModelAfterSave(this, WebPages.Manager.GetActiveMenuItem(), m, !draft);

						ModelState.Clear();
						if (!draft) {
							if (m.Post.Published == m.Post.LastPublished)
								SuccessMessage(Piranha.Resources.Post.MessagePublished, true);
							else SuccessMessage(Piranha.Resources.Post.MessageUpdated, true);
						} else SuccessMessage(Piranha.Resources.Post.MessageSaved, true);

						return RedirectToAction("edit", new { id = m.Post.Id, returl = ViewBag.ReturnUrl });
					} else ErrorMessage(Piranha.Resources.Post.MessageNotSaved);
				} catch (DuplicatePermalinkException) {
					// Manually set the duplicate error.
					ModelState.AddModelError("Permalink", @Piranha.Resources.Global.PermalinkDuplicate);
					// If this is the default permalink, remove the model state so it will be shown.
					if (Permalink.Generate(m.Post.Title) == m.Permalink.Name)
						ModelState.Remove("Permalink.Name");
				} catch (Exception e) {
					ErrorMessage(e.ToString());
				}
			}
			m.Refresh();

			// Executes the post edit loaded hook, if registered
			if (WebPages.Hooks.Manager.PostEditModelLoaded != null)
				WebPages.Hooks.Manager.PostEditModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), m);

			if (m.Post.IsNew)
				ViewBag.Title = Piranha.Resources.Post.EditTitleNew + m.Template.Name.ToLower();
			else ViewBag.Title = Piranha.Resources.Post.EditTitleExisting;

			return View(@"~/Areas/Manager/Views/Post/Edit.cshtml", m);
		}

		/// <summary>
		/// Deletes the post.
		/// </summary>
		/// <param name="id">The post id</param>
		[Access(Function = "ADMIN_POST")]
		public ActionResult Delete(string id) {
			EditModel pm = EditModel.GetById(new Guid(id));

			if (pm.DeleteAll())
				SuccessMessage(Piranha.Resources.Post.MessageDeleted, true);
			else ErrorMessage(Piranha.Resources.Post.MessageNotDeleted, true);

			if (!String.IsNullOrEmpty(ViewBag.ReturnUrl))
				return Redirect(ViewBag.ReturnUrl);
			return RedirectToAction("index");
		}

		/// <summary>
		/// Reverts to latest published verison.
		/// </summary>
		/// <param name="id">The post id.</param>
		[Access(Function = "ADMIN_POST")]
		public ActionResult Revert(string id) {
			EditModel.Revert(new Guid(id));

			SuccessMessage(Piranha.Resources.Post.MessageReverted);

			return Edit(id);
		}

		/// <summary>
		/// Unpublishes the specified page.
		/// </summary>
		/// <param name="id">The post id</param>
		[Access(Function = "ADMIN_POST")]
		public ActionResult Unpublish(string id) {
			EditModel.Unpublish(new Guid(id));

			SuccessMessage(Piranha.Resources.Post.MessageUnpublished);

			return Edit(id);
		}
	}
}
