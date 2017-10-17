using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using LonestarShowdown.Custom;
using LonestarShowdown.Database;

namespace LonestarShowdown.Views
{
    internal class TeamSelectionViewModel : Screen
    {
        private string[] _firstElevenNames;
        private int?[] _firstElevenNumbers;
        private Dictionary<string, int> _myPositionList;
        private List<Player> _mySquad;
        private Team _myTeam;
        private Player _selectedPlayer;

        public TeamSelectionViewModel(List<Player> mySquad, Team myTeam)
        {
            _myTeam = myTeam;
            _mySquad = mySquad;

            // Loads the list of all the available positions.
            using (var db = new LonestarDbContext())
            {
                MyPositionList = (from a in db.Squads select a).ToDictionary(a => a.SquadPosition, a => a.LineUpID);
            }

            Player.AvailablePositions = new List<string>();

            UpdateAvailablePositionList();

            LoadFirstEleven();
        }

        public Player SelectedPlayer
        {
            get { return _selectedPlayer; }
            set
            {
                _selectedPlayer = value;
                NotifyOfPropertyChange(() => SelectedPlayer);
            }
        }

        public string[] FirstElevenNames
        {
            get { return _firstElevenNames; }
            set
            {
                _firstElevenNames = value;
                NotifyOfPropertyChange(() => FirstElevenNames);
            }
        }

        public int?[] FirstElevenNumbers
        {
            get { return _firstElevenNumbers; }
            set
            {
                _firstElevenNumbers = value;
                NotifyOfPropertyChange(() => FirstElevenNumbers);
            }
        }

        public Team MyTeam
        {
            get { return _myTeam; }
            set
            {
                if (_myTeam == value)
                    return;

                _myTeam = value;
                NotifyOfPropertyChange(() => MyTeam);
            }
        }

        public List<Player> MySquad
        {
            get { return _mySquad; }
            set
            {
                if (_mySquad == value)
                    return;

                _mySquad = value;
                NotifyOfPropertyChange(() => MySquad);
            }
        }

        public Dictionary<string, int> MyPositionList
        {
            get { return _myPositionList; }
            set
            {
                if (_myPositionList == value)
                    return;

                _myPositionList = value;
                NotifyOfPropertyChange(() => MyPositionList);
            }
        }

        /// <summary>
        ///     Saves the changes to the db.
        /// </summary>
        public void SaveChangesAction()
        {
            using (var db = new LonestarDbContext())
            {
                foreach (var item in MySquad)
                {
                    db.Personnels.Find(item.PlayerId).LineUpID = item.LineUpId;
                }
                db.SaveChanges();
            }
            MyConfig.Configuration.HasTeamChanged = true;
            BackAction();
        }

        /// <summary>
        ///     Returns back to the MyTeamViewScreen.
        /// </summary>
        public void CancelChangesAction()
        {
            MyConfig.Configuration.HasTeamChanged = false;
            BackAction();
        }

        /// <summary>
        ///     Extracts the starting eleven.
        /// </summary>
        private void LoadFirstEleven()
        {
            // Extracts the first eleven's jersey numbers and last names.
            FirstElevenNames = new string[11];
            FirstElevenNumbers = new int?[11];
            foreach (var item in MySquad.Where(item => item.LineUpId != null && item.LineUpId <= 11))
            {
                if (item.LineUpId != null)
                {
                    FirstElevenNames[(int) item.LineUpId - 1] = item.LastName;
                    FirstElevenNumbers[(int) item.LineUpId - 1] = item.JerseyNumber;
                }
            }
        }

        /// <summary>
        ///     Clears the position.
        /// </summary>
        public void ResetPositionAction(Player selectedPlayer)
        {
            SelectedPlayer = selectedPlayer;
            ChangePositionAction(true);
        }

        /// <summary>
        ///     Changes positions.
        /// </summary>
        public void ChangePositionAction(bool isReset = false)
        {
            var player = MySquad.FirstOrDefault(p => p.PlayerId == SelectedPlayer.PlayerId);
            if (player != null)
            {
                player.LineUpPosition = isReset ? null : Player.SelectedPosition;
                player.LineUpId = isReset ? (int?) null : MyPositionList[Player.SelectedPosition];
            }

            MySquad = MySquad.OrderBy(x => x.LineUpPosition == null).ThenBy(x => x.LineUpPosition).ToList();

            UpdateAvailablePositionList();

            LoadFirstEleven();

            NotifyOfPropertyChange(null);
        }

        /// <summary>
        ///     Gets all the free positions.
        /// </summary>
        private void UpdateAvailablePositionList()
        {
            Player.AvailablePositions.Clear();

            foreach (
                var item in
                    MyPositionList.Keys.Where(item => MySquad.FirstOrDefault(x => x.LineUpPosition == item) == null))
            {
                Player.AvailablePositions.Add(item);
            }
        }

        /// <summary>
        ///     Returns back to the MyTeam screen.
        /// </summary>
        public void BackAction()
        {
            TryClose();
        }
    }
}