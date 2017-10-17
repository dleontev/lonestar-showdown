using Caliburn.Micro;
using LonestarShowdown.Views;

namespace LonestarShowdown.Shell
{
    public class AppViewModel : Conductor<object>
    {
        protected override void OnActivate()
        {
            ActivateItem(new SignInViewModel());
            base.OnActivate();
        }
    }
}