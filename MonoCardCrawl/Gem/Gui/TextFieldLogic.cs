using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Gui
{
    public static class TextFieldLogic
    {
        public static String HandleKeyPress(String InputBuffer, char Code)
        {
            if (Code == '\b')
            {
                if (InputBuffer.Length > 0)
                    InputBuffer = InputBuffer.Substring(0, InputBuffer.Length - 1);
            }
            else
            {
                InputBuffer += Code;
            }
            return InputBuffer;
        }


    }

	public class TextFieldKeyboardHandler : IKeyboardHandler
	{
		internal UIItem Item;

		public TextFieldKeyboardHandler(UIItem Root) { this.Item = Root; }

		public void KeyUp(System.Windows.Forms.KeyEventArgs e)
		{
		}

		public void KeyDown(System.Windows.Forms.KeyEventArgs e)
		{
		}

		public void KeyPress(System.Windows.Forms.KeyPressEventArgs e)
		{
			e.Handled = false;

			if (e.KeyChar == '~') return;

			if (Item == null) return;

			var label = Item.GetSetting("LABEL", "");
			label = TextFieldLogic.HandleKeyPress(label.ToString(), e.KeyChar);
			Item.Settings.Upsert("LABEL", label);
			e.Handled = true;
		}
	}
}
