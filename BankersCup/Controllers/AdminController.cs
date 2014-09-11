﻿using BankersCup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BankersCup.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View(MvcApplication.CurrentGame.RegisteredTeams);
        }

        public ActionResult Register()
        {
            Team teamModel = new Team();
            teamModel.Players = new List<Player>() { new Player() };
            return View(teamModel);
        }

        [HttpPost]
        public ActionResult Register(Team newTeam)
        {
            newTeam.RegistrationCode = new Random().Next(0, 1000).ToString();
            MvcApplication.CurrentGame.RegisteredTeams.Add(newTeam);
            return RedirectToAction("Index");
        }
    }
}