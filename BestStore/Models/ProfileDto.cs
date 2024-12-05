﻿using System.ComponentModel.DataAnnotations;

namespace BestStore.Models
{
    public class ProfileDto
    {
        [Required(ErrorMessage = "Please enter your first name"), MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your first name"), MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "The format of the phone number is not valid"), MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required, MaxLength(200)]
        public string Address { get; set; } = string.Empty;
    }
}