﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using control_notas_cit.Models.Entidades;
using System.ComponentModel.DataAnnotations;

namespace control_notas_cit.Models.ViewModels
{
    public class CoordinadorIndexViewModel
    {
        public Celula Celula { get; set; }
        public Semana Semana { get; set; }
    }

    public class AlumnoViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage="Este campo es obligatorio")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "Este campo es obligatorio")]
        public string Apellido { get; set; }
        [Required(ErrorMessage = "Este campo es obligatorio")]
        public string Cedula { get; set; }
        [Required(ErrorMessage = "Este campo es obligatorio")]
        public string Telefono { get; set; }
        [Required(ErrorMessage = "Este campo es obligatorio")]
        [EmailAddress]
        public string Email { get; set; }
    }
}