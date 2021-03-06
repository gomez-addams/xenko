﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="Button"/>  that has <see cref="Button.SizeToContent"/> set to <c>false</c>.
    /// </summary>
    public class ImageButtonTest : UITestGameBase
    {
        private Button button;

        public ImageButtonTest()
        {
            CurrentVersion = 4;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            button = new Button
            {
                PressedImage = (SpriteFromTexture)new Sprite(Content.Load<Texture>("ImageButtonPressed")),
                NotPressedImage = (SpriteFromTexture)new Sprite(Content.Load<Texture>("ImageButtonNotPressed")),
                Content = null,
                SizeToContent = false
            };

            UIComponent.Page = new Engine.UIPage { RootElement = button };
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.TakeScreenshot();
            FrameGameSystem.Draw(DrawTest1).TakeScreenshot();
        }

        public void DrawTest1()
        {
            button.RaiseTouchDownEvent(new TouchEventArgs());
        }

        [Test]
        public void RunImageButtonTest()
        {
            RunGameTest(new ImageButtonTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new ImageButtonTest())
                game.Run();
        }
    }
}
