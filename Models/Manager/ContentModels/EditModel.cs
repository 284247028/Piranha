﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

using Piranha.Data;

namespace Piranha.Models.Manager.ContentModels
{
	/// <summary>
	/// Edit model for the content record.
	/// </summary>
	public class EditModel
	{
		#region Properties
		/// <summary>
		/// Gets/sets the content record.
		/// </summary>
		public Piranha.Models.Content Content { get ; set ; }

		/// <summary>
		/// Gets/sets the optional file.
		/// </summary>
		public HttpPostedFileBase UploadedFile { get ; set ; }
		#endregion

		/// <summary>
		/// Default constructor. Creates a new model.
		/// </summary>
		public EditModel() {
			Content = new Piranha.Models.Content() ;
		}

		/// <summary>
		/// Gets the edit model for the content with the given id.
		/// </summary>
		/// <param name="id">The content id</param>
		/// <returns>The model</returns>
		public static EditModel GetById(Guid id) {
			EditModel em = new EditModel() ;
			em.Content = Piranha.Models.Content.GetSingle(id) ;

			return em ;
		}

		/// <summary>
		/// Saves the edit model.
		/// </summary>
		public bool SaveAll() {
			var context = HttpContext.Current ;

			// Check if this is an image
			try {
				Image img = Image.FromStream(UploadedFile.InputStream) ;
				Content.IsImage = true ;
				Content.Width = img.Width ;
				Content.Height = img.Height ;
			} catch {
				Content.IsImage = false ;
			}

			Content.Filename = UploadedFile.FileName ;
			Content.Type = UploadedFile.ContentType ;
			Content.Size = UploadedFile.ContentLength ;

			if (Content.Save()) {
				string path = context.Server.MapPath("~/App_Data/content") ;
				if (File.Exists(Content.PhysicalPath)) {
					File.Delete(Content.PhysicalPath) ;
					Content.DeleteCache() ;
				}
				UploadedFile.SaveAs(Content.PhysicalPath) ;
				//UploadedFile.SaveAs(path + "/" + Content.Id) ;
				return true ;
			}
			return false ;
		}

		/// <summary>
		/// Deletes the specified content and its related file.
		/// </summary>
		public bool DeleteAll() {
			using (IDbTransaction tx = Database.OpenTransaction()) {
				try {
					File.Delete(HttpContext.Current.Server.MapPath("~/App_Data/Content/" + Content.Id)) ;
					Content.Delete(tx) ;
					tx.Commit() ;
					return true ;
				} catch { tx.Rollback() ; }
			}
			return false ;
		}
	}
}
