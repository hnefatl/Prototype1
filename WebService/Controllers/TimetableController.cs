using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Data.DataContext;
using WebService.Models;

namespace WebService.Controllers
{
    public class TimetableController
        : Controller
    {
        //
        // GET: /Timetable/

        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult Timetable()
        {
            using (DataRepository Repo = new DataRepository())
            {
                return PartialView("Timetable", (DateTime.Now));
            }
        }
    }
}
