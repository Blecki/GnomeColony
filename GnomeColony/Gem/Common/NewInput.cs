﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace Gem
{
    public class NewInput
    {
        public class InputAction
        {
            public String Name;
            public Action Handler;
            public Keys Key;
            public KeyBindingType Type;
        }

        private GumInputMapper Mapper;
        private List<InputAction> InputActions = new List<InputAction>();
        private Gum.Root GuiRoot;
        private Action<Gum.InputEvents, Gum.InputEventArgs> MouseHandler;

        public NewInput(IntPtr Handle, Gum.Root GuiRoot, Action<Gum.InputEvents, Gum.InputEventArgs> MouseHandler)
        {
            Mapper = new GumInputMapper(Handle);
            this.MouseHandler = MouseHandler;
            this.GuiRoot = GuiRoot;
        }

        public void BindKeyAction(Keys Key, String Name, KeyBindingType Type)
        {
            InputActions.Add(new InputAction
            {
                Name = Name,
                Key = Key,
                Type = Type,
                Handler = null

            });
        }

        public void BindKeyAction(Keys Key, String Name, KeyBindingType Type, Action Handler)
        {
            InputActions.Add(new InputAction
            {
                Name = Name,
                Key = Key,
                Type = Type,
                Handler = Handler

            });
        }

        public void BindAction(String Name, Action Handler)
        {
            var binding = InputActions.FirstOrDefault(ia => ia.Name == Name);
            if (binding != null) binding.Handler += Handler;
        }

        public void ClearAction(String Name)
        {
            var binding = InputActions.FirstOrDefault(ia => ia.Name == Name);
            if (binding != null) binding.Handler = null;
        }

        public void FireActions()
        {
            var queue = Mapper.GetInputQueue();
            foreach (var @event in queue)
            {
                GuiRoot.HandleInput(@event.Message, @event.Args);
                if (!@event.Args.Handled)
                {
                    if (@event.Message == Gum.InputEvents.MouseClick ||
                        @event.Message == Gum.InputEvents.MouseMove)
                        MouseHandler(@event.Message, @event.Args);
                    else if (@event.Message == Gum.InputEvents.KeyUp)
                    {
                        foreach (var handler in InputActions.Where(ia => (int)ia.Key == @event.Args.KeyValue && ia.Type == KeyBindingType.Pressed))
                            if (handler.Handler != null) 
                                handler.Handler();
                    }
                }
            }

            // Check 'Held' actions
            var kbState = Keyboard.GetState();
            foreach (var handler in InputActions.Where(ia => ia.Type == KeyBindingType.Held))
                if (kbState.IsKeyDown(handler.Key) && handler.Handler != null) 
                    handler.Handler();
        }
    }
}