using System;
using System.Collections.Generic;

namespace LonestarShowdown.Custom
{
    internal class Player
    {
        public byte[] ImageData { get; set; }
        public bool IsFreeAgent { get; set; }

        public double Age
        {
            get { return (int) (Math.Floor(DateTime.Today.Subtract(DateOfBirth).TotalDays/365.25)); }
        }

        public static string SelectedPosition { get; set; }
        public static List<string> AvailablePositions { get; set; }
        public int? LineUpId { get; set; }
        public string Address { get; set; }
        public string LineUpPosition { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int PlayerId { get; set; }

        public string Name
        {
            get { return FirstName + " " + LastName; }
        }

        public string Email { get; set; }
        public string Position { get; set; }
        public int JerseyNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesStarted { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public int Saves { get; set; }
        public int GoalsAllowed { get; set; }
        public int StatId { get; set; }
    }
}