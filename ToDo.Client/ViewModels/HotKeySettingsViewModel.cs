using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ToDo.Client.ViewModels
{
    public class HotKeySettingsViewModel : ViewModel
    {
        public Action showDashboardCb;
        public Action closeWindowCb;

        public bool Changed { get; private set; }

        public bool Enabled { get; set; }

        private const string Alt = "Alt";
        private const string Shift = "Shift";
        private const string None = "{None}";

        private string chr;
        public string Character
        {
            get { return chr; }
            set
            {
                chr = value;
                if (!string.IsNullOrEmpty(chr))
                    chr = chr[0].ToString().ToUpper();

                RaisePropertyChanged();
            }
        }

        public List<string> Modifiers { get; set; }

        public string SelectedModifier { get; set; }
        public string Key { get; set; } //TODO: Remove

        public HotKeySettingsViewModel(Action showDashboardCb, Action closeWindowCb)
        {
            this.showDashboardCb = showDashboardCb;
            this.closeWindowCb = closeWindowCb;

            Modifiers = new List<string>()
            {
                None,
                Alt,
                Shift
            };

            Changed = false;
            
            Setup();
        }

        private void Setup()
        {
            var Settings = Properties.Settings.Default;

            Enabled = Settings.HotkeyEnabled && Settings.Hotkey != null;

            if (Enabled)
            {
                Hotkey hk = Hotkey.Deserialize(Settings.Hotkey);

                SelectedModifier = ToString(hk.Modifier);
                RaisePropertyChanged("SelectedModifier");

                Character = hk.Key.ToString();
                RaisePropertyChanged("Character");
            }
        }

        private string ToString(ModifierKeys modifier)
        {
            if (modifier == ModifierKeys.None)
                return None;
            else if (modifier == ModifierKeys.Alt)
                return Alt;
            else if (modifier == ModifierKeys.Shift)
                return Shift;
            else
                throw new Exception("Modifier not supported");
        }

        public ICommand CancelCommand
        {
            get { return new CommandHelper(closeWindowCb); }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new CommandHelper(Save);
            }
        }

        private void Save()
        {
            try
            {
                if (!Enabled)
                    Hotkey.ClearHotkey();
                else
                {
                    ModifierKeys mod = ToModifier(SelectedModifier);
                    Key key = ToKey(Character);

                    Hotkey hk = new Hotkey(mod, key);
                    Hotkey.RegisterHotkey(hk, showDashboardCb);
                }

                Changed = true;
                closeWindowCb.Invoke();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);

                Changed = false;
            }
        }

        private Key ToKey(string key)
        {
            return (Key)Enum.Parse(typeof(Key), key);
        }

        private ModifierKeys ToModifier(string modifier)
        {
            if (modifier == null)
                return ModifierKeys.None;

            switch (modifier)
            {
                case Alt:
                    return ModifierKeys.Alt;
                case Shift:
                    return ModifierKeys.Shift;
                case None:
                default:
                    return ModifierKeys.None;

            }
        }
    }
}
