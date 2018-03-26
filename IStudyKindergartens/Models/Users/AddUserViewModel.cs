﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IStudyKindergartens.Models.Users
{
    public class AddUserViewModel
    {
        [Display(Name = "PictureName")]
        public string PictureName { get; set; }

        [StringLength(maximumLength: 100)]
        [Required(ErrorMessage = "Вкажіть своє прізвище!")]
        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [StringLength(maximumLength: 100)]
        [Required(ErrorMessage = "Вкажіть своє ім'я!")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [StringLength(maximumLength: 100)]
        [Required(ErrorMessage = "Вкажіть своє ім'я по батькові!")]
        [Display(Name = "FathersName")]
        public string FathersName { get; set; }

        [Required(ErrorMessage = "Вкажіть свій номер телефону!")]
        [Display(Name = "PhoneNumber")]
        [Remote("CheckPhoneNumber", "Account", ErrorMessage = "Не коректно вказаний номер телефону!")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Вкажіть свій електронний адрес!")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Не коректно вказаний емейл!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Вкажіть свою дату народження!")]
        [Display(Name = "DateOfBirth")]
        [Remote("CheckDateOfBirth", "Account", ErrorMessage = "Не коректна дата народження!")]
        public string DateOfBirth { get; set; }

        [Required(ErrorMessage = "Вкажіть пароль!")]
        [StringLength(100, ErrorMessage = "Пароль повинен бути більше 6 символів", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Повторіть пароль!")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Паролі не співпадають!")]
        public string ConfirmPassword { get; set; }
    }
}