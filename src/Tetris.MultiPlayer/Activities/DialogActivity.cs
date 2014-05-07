using Jv.Games.Xna.Async;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tetris.MultiPlayer.Components;

namespace Tetris.MultiPlayer.Activities
{
    class DialogActivity : Activity
    {
        MenuInput _menuInput;
        SpriteFont _font;

        public DialogActivity(Game game)
            : base(game)
        {
            base.IsTransparent = true;
            _menuInput = new MenuInput();
        }

        protected override void Initialize()
        {
            _font = Content.Load<SpriteFont>("DefaultFont");
            base.Initialize();
        }

        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            SpriteBatch.Begin();
            SpriteBatch.DrawString(_font, "Teste de Dialog", Vector2.Zero, Color.Black);
            SpriteBatch.End();
        }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _menuInput.Update(gameTime);
            if (_menuInput.Press(MenuButton.Cancel))
                Exit();
        }
    }
}
