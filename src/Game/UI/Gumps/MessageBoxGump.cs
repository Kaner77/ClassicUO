﻿#region license

// Copyright (C) 2020 ClassicUO Development Community on Github
// 
// This project is an alternative client for the game Ultima Online.
// The goal of this is to develop a lightweight client considering
// new technologies.
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using SDL2;

namespace ClassicUO.Game.UI.Gumps
{
    internal class MessageBoxGump : Gump
    {
        private readonly Action<bool> _action;

        public MessageBoxGump(int w, int h, string message, Action<bool> action, bool hasBackground = false) : base
            (0, 0)
        {
            CanMove = true;
            CanCloseWithRightClick = false;
            CanCloseWithEsc = false;
            AcceptMouseInput = true;
            AcceptKeyboardInput = true;

            IsModal = true;
            LayerOrder = UILayer.Over;
            WantUpdateSize = false;

            Width = w;
            Height = h;
            _action = action;

            Add
            (
                new ResizePic(0x0A28)
                {
                    Width = w, Height = h
                }
            );

            if (hasBackground)
            {
                ResizePic background = new ResizePic(3000)
                {
                    X = X + 30,
                    Y = Y + 40,
                    Width = Width - 60,
                    Height = Height - 100
                };

                Add(background);
            }

            Add
            (
                new Label(message, false, 0x0386, Width - 90, 1)
                {
                    X = 40,
                    Y = 45
                }
            );

            X = (Client.Game.Window.ClientBounds.Width - Width) >> 1;
            Y = (Client.Game.Window.ClientBounds.Height - Height) >> 1;

            // OK
            Button b;

            Add
            (
                b = new Button(0, 0x0481, 0x0483, 0x0482)
                {
                    Y = Height - 45,
                    ButtonAction = ButtonAction.Activate
                }
            );

            b.X = (Width - b.Width) >> 1;

            WantUpdateSize = false;

            UIManager.KeyboardFocusControl = this;
            UIManager.KeyboardFocusControl.SetKeyboardFocus();
        }

        protected override void OnKeyUp(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            base.OnKeyUp(key, mod);

            if (key == SDL.SDL_Keycode.SDLK_RETURN && mod == 0)
            {
                OnButtonClick(0);
            }
        }

        public override void OnButtonClick(int buttonID)
        {
            switch (buttonID)
            {
                case 0:
                    _action?.Invoke(true);
                    Dispose();

                    break;
            }
        }
    }


    internal class EntryDialog : Gump
    {
        private readonly Action<string> _action;
        private readonly StbTextBox _textBox;

        public EntryDialog(int w, int h, string message, Action<string> action) : base(0, 0)
        {
            CanMove = false;
            CanCloseWithRightClick = false;
            CanCloseWithEsc = false;
            AcceptMouseInput = false;

            IsModal = true;
            LayerOrder = UILayer.Over;
            WantUpdateSize = false;

            Width = w;
            Height = h;
            _action = action;

            Add
            (
                new ResizePic(0x0A28)
                {
                    Width = w,
                    Height = h
                }
            );

            Label l;

            Add
            (
                l = new Label(message, false, 0x0386, Width - 90, 1)
                {
                    X = 40,
                    Y = 45
                }
            );

            Add
            (
                new ResizePic(0x0BB8)
                {
                    X = 40,
                    Y = 45 + l.Height + 5,
                    Width = w - 90,
                    Height = 25
                }
            );

            int ww = w - 94;

            _textBox = new StbTextBox(0xFF, -1, ww, true, FontStyle.BlackBorder | FontStyle.Fixed)
            {
                X = 42,
                Y = 45 + l.Height + 7,
                Width = ww,
                Height = 25
            };

            Add(_textBox);

            X = (Client.Game.Window.ClientBounds.Width - Width) >> 1;
            Y = (Client.Game.Window.ClientBounds.Height - Height) >> 1;


            // OK
            Button b;

            Add
            (
                b = new Button(0, 0x0481, 0x0482, 0x0483)
                {
                    Y = Height - 45,
                    ButtonAction = ButtonAction.Activate
                }
            );

            b.X = (Width - b.Width) >> 1;
        }

        public override void OnButtonClick(int buttonID)
        {
            switch (buttonID)
            {
                case 0:
                    _action?.Invoke(_textBox.Text);
                    Dispose();

                    break;
            }
        }
    }
}