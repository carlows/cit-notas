﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using control_notas_cit.Models.Repositorios;
using control_notas_cit.Models.Entidades;
using control_notas_cit.Models.ViewModels;
using IdentitySample.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace control_notas_cit.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext AppContext;
        private IRepositorioGenerico<Proyecto> repoProyectos = null;
        private IRepositorioGenerico<ApplicationUser> repoUsers = null;
        private string profesor_rol_id;

        public AdminController()
        {
            AppContext = new ApplicationDbContext();
            this.repoProyectos = new RepositorioGenerico<Proyecto>(AppContext);
            this.repoUsers = new RepositorioGenerico<ApplicationUser>(AppContext);

            this.profesor_rol_id = (AppContext.Roles.Where(x => x.Name == "Profesor")).Select(y => y.Id).Single();
        }

        //
        // GET: /Admin/
        public ActionResult Index()
        {
            return View(repoProyectos.SelectAll());
        }

        //
        // GET: /Admin/Crear/
        public ActionResult Crear()
        {
            List<ApplicationUser> users = AppContext.Users.Where(x => x.Roles.Select(y => y.RoleId).Contains(profesor_rol_id)).ToList();
            List<string> nombres = new List<string>();

            foreach( ApplicationUser u in users )
            {
                nombres.Add(string.Concat(u.Nombre + " " + u.Apellido));
            }
            
            return View(new ProjectViewModel
            {
                Profesores = nombres
            });
        }

        //
        // POST: /Admin/Crear/
        [HttpPost]
        public ActionResult Crear(ProjectViewModel model)
        {
            if( ModelState.IsValid )
            {
                List<ApplicationUser> profesores = (from u in AppContext.Users
                                                   where model.Profesores.Contains(string.Concat(u.Nombre, " ", u.Apellido))
                                                   select u).ToList();
                Proyecto p = new Proyecto()
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    Profesores = profesores
                };
                repoProyectos.Insert(p);
                repoProyectos.Save();

                return RedirectToAction("Index");
            }
            return View(model);
        }

        //
        // GET: /Admin/ListaProfesores/
        public ActionResult ListaProfesores()
        {
            List<ApplicationUser> model = AppContext.Users.Where(u => u.Roles.Select(r => r.RoleId).Contains(profesor_rol_id)).ToList();
            return View(model);
        }

        //
        // GET: /Admin/AgregarProfesor/
        public ActionResult AgregarProfesor()
        {
            return View(new ProfesorViewModel() 
            {
                Proyectos = new SelectList(repoProyectos.SelectAll().Select(p => p.Nombre).ToList())
            });
        }

        //
        // POST: /Admin/AgregarProfesor/
        [HttpPost]
        public async Task<ActionResult> AgregarProfesor(ProfesorViewModel model)
        {
            if( ModelState.IsValid )
            {
                ApplicationUser profesor;

                if (model.Proyecto != null)
                {
                    Proyecto p = AppContext.Proyectos.AsNoTracking().Where(z => z.Nombre == model.Proyecto).Single();

                    profesor = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        Nombre = model.Nombre,
                        Apellido = model.Apellido,
                        Cedula = model.Cedula,
                        PhoneNumber = model.Telefono,
                        Proyecto = p
                    };
                }
                else
                {
                    profesor = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        Nombre = model.Nombre,
                        Apellido = model.Apellido,
                        Cedula = model.Cedula,
                        PhoneNumber = model.Telefono
                    };
                }

                var profesorResult = await UserManager.CreateAsync(profesor, model.PasswordHash);

                if( profesorResult.Succeeded )
                {
                    var roleResult = await UserManager.AddToRoleAsync(profesor.Id, "Profesor");

                    if( !roleResult.Succeeded )
                    {
                        ModelState.AddModelError("", roleResult.Errors.First());
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", profesorResult.Errors.First());
                    return View();
                }

                return RedirectToAction("ListaProfesores");
            }

            model.Proyectos = new SelectList(repoProyectos.SelectAll().Select(p => p.Nombre).ToList());
            return View(model);
        }

        //
        // GET: /Admin/EditarProfesor/1
        public async Task<ActionResult> EditarProfesor(string id)
        {
            if (id == null)
            {
                return RedirectToAction("ListaProfesores");
            }
            var profesor = await UserManager.FindByIdAsync(id);
            if (profesor == null)
            {
                return RedirectToAction("ListaProfesores");
            }

            if (profesor.Proyecto != null)
            {
                return View(new ProfesorViewModel()
                {
                    Id = profesor.Id,
                    Nombre = profesor.Nombre,
                    Apellido = profesor.Apellido,
                    Email = profesor.Email,
                    Proyectos = new SelectList(repoProyectos.SelectAll().Select(p => p.Nombre).ToList(), profesor.Proyecto.Nombre)
                });
            }
            else
            {
                return View(new ProfesorViewModel()
                {
                    Id = profesor.Id,
                    Nombre = profesor.Nombre,
                    Apellido = profesor.Apellido,
                    Email = profesor.Email,
                    Proyectos = new SelectList(repoProyectos.SelectAll().Select(p => p.Nombre).ToList())
                });
            }
        }

        //
        // POST: /Roles/Delete/5
        [HttpPost]
        public async Task<ActionResult> BorrarProfesor(string id)
        {
            if (id == null)
            {
                return RedirectToAction("ListaProfesores");
            }

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return RedirectToAction("ListaProfesores");
            }
            var result = await UserManager.DeleteAsync(user);

            return RedirectToAction("ListaProfesores");
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
	}
}