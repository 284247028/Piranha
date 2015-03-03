/*
 * Copyright (c) 2015 H�kan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 * 
 * http://github.com/piranhacms/piranha
 * 
 */

namespace Piranha.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;

	internal sealed class Configuration : DbMigrationsConfiguration<Piranha.Db>
	{
		public Configuration() {
			AutomaticMigrationsEnabled = false;
		}

		protected override void Seed(Piranha.Db db) {
			bool initalCreate = db.Params.Count() == 0;

			// Params
			var param = db.Params.Where(p => p.Name == "SITE_LAST_MODIFIED").SingleOrDefault();
			if (param == null) {
				db.Params.Add(new Data.Param() {
					Name = "SITE_LAST_MODIFIED",
					IsSystem = true,
					Description = "Global last modification date.",
					Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
				});
			}
		}
	}
}
