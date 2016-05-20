using NHotkey.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ToDo.Client
{
    public class Hotkey
    {
        private const char Delim = ',';
        
        public ModifierKeys Modifier { get; private set; }
        public Key Key { get; private set; }

        public Hotkey(ModifierKeys mod, Key key)
        {
            this.Modifier = mod;
            this.Key = key;
        }

        public override string ToString()
        {
            string str = Convert.ToString((int)Modifier);
            str += Delim;
            str += Convert.ToString((int)Key);

            return str;
        }

        public string DisplayName
        {
            get
            {
                return string.Join(" + ", Modifier.ToString(), Key.ToString());
            }
        }

        
        public static Hotkey Deserialize(string str)
        {
            string[] split = str.Split(Delim);

            //None is a value
            ModifierKeys mod = (ModifierKeys)Convert.ToInt32(split[0]);
            Key key = (Key)Convert.ToInt32(split[1]);

            return new Hotkey(mod, key);
        }

        #region Hotkey "Manager"

        private const string ShowAppKey = "ShowApp";

        private static Properties.Settings Settings = Properties.Settings.Default;
        private static HotkeyManager Manager = HotkeyManager.Current;

        public static void ClearHotkey()
        {
            RegisterHotkey(null, null);
        }

        /// <summary>
        /// Registers a HotKey with CTRL + {mod} + {key}
        /// </summary>
        /// <param name="hotkey"></param>
        /// <param name="callback"></param>
        public static void RegisterHotkey(Hotkey hotkey, Action callback)
        {
            if (hotkey == null || callback == null)
            {
                Manager.Remove(ShowAppKey);
                Settings.Hotkey = null;
                Settings.HotkeyEnabled = false;
            }
            else
            {
                Settings.Hotkey = hotkey.ToString();
                Settings.HotkeyEnabled = true;

                Manager.AddOrReplace(ShowAppKey, hotkey.Key, 
                    ModifierKeys.Control | hotkey.Modifier, 
                    (sender,e) => callback.Invoke());
            }

            Settings.Save();
        }

        /// <summary>
        /// Registers the hotkey.
        /// </summary>
        /// <param name="callback">If null, hotkey will be removed</param>
        public static void SetDefaultShowWindowHotkey(Action callback)
        {
            if (Settings.HotkeyEnabled)
            {
                if (!string.IsNullOrEmpty(Settings.Hotkey))
                {
                    var hk = Hotkey.Deserialize(Settings.Hotkey);
                    RegisterHotkey(hk, callback);
                }
            }
        }

        public static Hotkey GetDefaultShowWindowHotkey()
        {
            if (Settings.HotkeyEnabled == true //Need?
                && !string.IsNullOrEmpty(Settings.Hotkey))
            {
                try
                {
                    var hk = Hotkey.Deserialize(Settings.Hotkey);
                    return hk;
                }
                catch { }
            }

            return null;
        }

        #endregion
    }

}
