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
using Piranha.Models.Manager.PageModels;

namespace Piranha.Areas.Manager.Controllers
{
	/// <summary>
	/// Manager controller for handling pages.
	/// </summary>
	public class PageController : ManagerController
	{
		/// <summary>
		/// Default controller. Gets the page list.
		/// </summary>
		[Access(Function = "ADMIN_PAGE")]
		public ActionResult Index(string id = "") {
			var internalId = Config.SiteTree;
			
			var activeSite = Session["activeSite"] as string;
			if (activeSite != null)
			{
				internalId = activeSite;
			}
			try {
				var param = Piranha.Models.SysParam.GetByName("SITEMAP_EXPANDED_LEVELS");
				ViewBag.Levels = param != null ? Convert.ToInt32(param.Value) : 0;

				if (!String.IsNullOrEmpty(id)) {
					var p = Page.GetSingle(new Guid(id), true);
					if (p != null)
						internalId = p.SiteTreeInternalId;

					ViewBag.Expanded = new Guid(id);
				} else ViewBag.Expanded = Guid.Empty;
			} catch {
				ViewBag.Levels = 0;
				ViewBag.Expanded = Guid.Empty;
			}
			var m = ListModel.Get(internalId);
			ViewBag.Title = @Piranha.Resources.Page.ListTitle;

			// Executes the page list loaded hook, if registered
			if (WebPages.Hooks.Manager.PageListModelLoaded != null)
				WebPages.Hooks.Manager.PageListModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), m);

			return View(@"~/Areas/Manager/Views/Page/Index.cshtml", m);
		}

		/// <summary>
		/// Gets the site tree with the given id.
		/// </summary>
		/// <param name="id">The internal id of the site tree</param>
		[Access(Function = "ADMIN_PAGE")]
		public ActionResult Site(string id) {
			try {
				var param = Piranha.Models.SysParam.GetByName("SITEMAP_EXPANDED_LEVELS");
				ViewBag.Levels = param != null ? Convert.ToInt32(param.Value) : 0;
				ViewBag.Expanded = Guid.Empty;
			} catch {
				ViewBag.Levels = 0;
				ViewBag.Expanded = Guid.Empty;
			}
			var m = ListModel.Get(id);
			ViewBag.Title = @Piranha.Resources.Page.ListTitle;

			// Executes the page list loaded hook, if registered
			if (WebPages.Hooks.Manager.PageListModelLoaded != null)
				WebPages.Hooks.Manager.PageListModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), m);

			return View(@"~/Areas/Manager/Views/Page/Index.cshtml", m);
		}

		/// <summary>
		/// Gets the SEO list for the site tree with the given id.
		/// </summary>
		/// <param name="id">The internal id of the site.</param>
		public ActionResult Seo(string id) {
			try {
				var param = Piranha.Models.SysParam.GetByName("SITEMAP_EXPANDED_LEVELS");
				ViewBag.Levels = param != null ? Convert.ToInt32(param.Value) : 0;
				ViewBag.Expanded = Guid.Empty;
			} catch {
				ViewBag.Levels = 0;
				ViewBag.Expanded = Guid.Empty;
			}
			var m = ListModel.GetSEO(id);
			ViewBag.Title = @Piranha.Resources.Page.ListTitle;

			// Executes the page list loaded hook, if registered
			if (WebPages.Hooks.Manager.PageListModelLoaded != null)
				WebPages.Hooks.Manager.PageListModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), m);

			return View(@"~/Areas/Manager/Views/Page/Index.cshtml", m);
		}

		/// <summary>
		/// Opens the edit view for the selected page.
		/// </summary>
		/// <param name="id">The page id</param>
		[Access(Function = "ADMIN_PAGE")]
		public ActionResult Edit(string id) {
			EditModel pm = EditModel.GetById(new Guid(id));

			if (!String.IsNullOrEmpty(Request["action"])) {
				if (Request["action"].ToLower() == "seo") {
					ViewBag.ReturnUrl = Url.Action("seo", new { @id = pm.Page.SiteTreeInternalId.ToLower() });
					pm.Action = EditModel.ActionType.SEO;
				} else if (Request["action"].ToLower() == "attachments") {
					pm.Action = EditModel.ActionType.ATTACHMENTS;
				}
			}

			if (!pm.IsSite)
				ViewBag.Title = Piranha.Resources.Page.EditTitleExisting;
			else ViewBag.Title = Piranha.Resources.Global.Edit + " " + pm.SiteTree.Name.ToLower();

			// Executes the page edit loaded hook, if registered
			if (WebPages.Hooks.Manager.PageEditModelLoaded != null)
				WebPages.Hooks.Manager.PageEditModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), pm);

			if (pm.Page.OriginalId != Guid.Empty)
				return View(@"~/Areas/Manager/Views/Page/EditCopy.cshtml", pm);
			return View(@"~/Areas/Manager/Views/Page/Edit.cshtml", pm);
		}

		/// <summary>
		/// Saves the currently edited page.
		/// </summary>
		/// <param name="pm">The page model</param>
		[HttpPost(), ValidateInput(false)]
		[Access(Function = "ADMIN_PAGE")]
		public ActionResult Edit(bool draft, EditModel pm) {
			if (ModelState.IsValid) {
				try {
					// Executes the page edit before save hook, if registered
					if (WebPages.Hooks.Manager.PageEditModelBeforeSave != null)
						WebPages.Hooks.Manager.PageEditModelBeforeSave(this, WebPages.Manager.GetActiveMenuItem(), pm, !draft);

					if (pm.SaveAll(draft)) {
						// Executes the page edit after save hook, if registered
						if (WebPages.Hooks.Manager.PageEditModelAfterSave != null)
							WebPages.Hooks.Manager.PageEditModelAfterSave(this, WebPages.Manager.GetActiveMenuItem(), pm, !draft);

						ModelState.Clear();
						if (!draft) {
							if (pm.Page.Published == pm.Page.LastPublished)
								SuccessMessage(Piranha.Resources.Page.MessagePublished);
							else SuccessMessage(Piranha.Resources.Page.MessageUpdated);
						} else SuccessMessage(Piranha.Resources.Page.MessageSaved);
					} else ErrorMessage(Piranha.Resources.Page.MessageNotSaved);
				} catch (DuplicatePermalinkException) {
					// Manually set the duplicate error.
					ModelState.AddModelError("Permalink", @Piranha.Resources.Global.PermalinkDuplicate);
					// If this is the default permalink, remove the model state so it will be shown.
					if (Permalink.Generate(pm.Page.Title) == pm.Permalink.Name)
						ModelState.Remove("Permalink.Name");
				} catch (Exception e) {
					ErrorMessage(e.ToString());
				}
			}
			pm.Refresh();

			// Executes the page list loaded hook, if registered
			if (WebPages.Hooks.Manager.PageEditModelLoaded != null)
				WebPages.Hooks.Manager.PageEditModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), pm);

			if (!pm.IsSite) {
				if (pm.Page.IsNew)
					ViewBag.Title = Piranha.Resources.Page.EditTitleNew + pm.Template.Name.ToLower();
				else ViewBag.Title = Piranha.Resources.Page.EditTitleExisting;
			} else ViewBag.Title = Piranha.Resources.Global.Edit + " " + pm.SiteTree.Name.ToLower();

			if (pm.Page.OriginalId != Guid.Empty)
				return View(@"~/Areas/Manager/Views/Page/EditCopy.cshtml", pm);
			return View(@"~/Areas/Manager/Views/Page/Edit.cshtml", pm);
		}

		/// <summary>
		/// Detaches the page with the given id from it's original.
		/// </summary>
		/// <param name="id">The page id</param>
		[Access(Function = "ADMIN_PAGE")]
		public ActionResult Detach(string id) {
			ViewBag.Title = Piranha.Resources.Page.EditTitleExisting;
			if (EditModel.Detach(new Guid(id))) {
				SuccessMessage(@Piranha.Resources.Page.MessageDetached);
				return View(@"~/Areas/Manager/Views/Page/Edit.cshtml", EditModel.GetById(new Guid(id)));
			} else {
				ErrorMessage(@Piranha.Resources.Page.MessageNotDetached);
				return View(@"~/Areas/Manager/Views/Page/EditCopy.cshtml", EditModel.GetById(new Guid(id)));
			}
		}

		/// <summary>
		/// Creates a new page.
		/// </summary>
		/// <param name="im">The insert model</param>
		[HttpPost()]
		[Access(Function = "ADMIN_PAGE")]
		public ActionResult Insert(InsertModel im) {
			EditModel pm = null;

			if (im.OriginalId != Guid.Empty)
				pm = EditModel.CreateByOriginalAndPosition(im.OriginalId, im.ParentId, im.Seqno, im.SiteTreeId, im.SiteTree);
			else pm = EditModel.CreateByTemplateAndPosition(im.TemplateId, im.ParentId, im.Seqno, im.SiteTreeId, im.SiteTree);

			ViewBag.Title = Piranha.Resources.Page.EditTitleNew + pm.Template.Name.ToLower();

			// Executes the page list loaded hook, if registered
			if (WebPages.Hooks.Manager.PageEditModelLoaded != null)
				WebPages.Hooks.Manager.PageEditModelLoaded(this, WebPages.Manager.GetActiveMenuItem(), pm);

			if (im.OriginalId != Guid.Empty)
				return View(@"~/Areas/Manager/Views/Page/EditCopy.cshtml", pm);
			return View(@"~/Areas/Manager/Views/Page/Edit.cshtml", pm);
		}

		/// <summary>
		/// Deletes the page specified by the given id.
		/// </summary>
		/// <param name="id">The page id</param>
		[Access(Function = "ADMIN_PAGE")]
		public ActionResult Delete(string id) {
			EditModel pm = EditModel.GetById(new Guid(id), true);

			try {
				if (pm.DeleteAll())
					SuccessMessage(Piranha.Resources.Page.MessageDeleted, true);
				else ErrorMessage(Piranha.Resources.Page.MessageNotDeleted, true);
			} catch (Exception e) {
				ErrorMessage(e.ToString(), true);
			}
			// Get the site page for the deleted page so we position ourselves correctly
			var p = Page.GetSingle("page_parent_id=@0", pm.Page.SiteTreeId);
			if (p != null)
				return RedirectToAction("index", new { id = p.Id });
			return RedirectToAction("index");
		}

		/// <summary>
		/// Reverts to latest published verison.
		/// </summary>
		/// <param name="id">The page id.</param>
		[Access(Function = "ADMIN_PAGE")]
		public ActionResult Revert(string id) {
			EditModel.Revert(new Guid(id));

			SuccessMessage(Piranha.Resources.Page.MessageReverted);

			return Edit(id);
		}

		/// <summary>
		/// Unpublishes the specified page.
		/// </summary>
		/// <param name="id">The page id</param>
		[Access(Function = "ADMIN_PAGE")]
		public ActionResult Unpublish(string id) {
			EditModel.Unpublish(new Guid(id));

			SuccessMessage(Piranha.Resources.Page.MessageUnpublished);

			return Edit(id);
		}

		/// <summary>
		/// Renders the sibling select list from the given input parameters.
		/// </summary>
		public ActionResult Siblings(string page_id, string page_parentid, string page_seqno, string parentid, string site_tree) {
			return View(EditModel.BuildSiblingPages(new Guid(page_id), new Guid(page_parentid), Convert.ToInt32(page_seqno), new Guid(parentid), site_tree));
		}

		/// <summary>
		/// Gets the grouplist for the given group and page.
		/// </summary>
		/// <param name="page_id">The page id.</param>
		/// <param name="group_id">The group id.</param>
		public ActionResult GroupList(string page_id, string group_id) {
			var page = Piranha.Models.Page.GetSingle(new Guid(page_id), true);
			var groups = SysGroup.GetParents(new Guid(group_id));
			groups.Reverse();

			return View("Partial/GroupList", new GroupListModel() {
				Groups = groups, Page = page
			});
		}

		/// <summary>
		/// Gets the page with the given id and returns it as a json object.
		/// </summary>
		/// <param name="id">The page id</param>
		/// <returns>The page</returns>
		public JsonResult Get(string id) {
			return Json(Page.GetSingle(new Guid(id)));
		}
	}
}
