using ATowerDefenceGame.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace ATowerDefenceGame
{
    class GameLevel : ILevel
    {
        private static readonly float ProjectileCircleRadius = 30f;

        private GraphicsDevice _graphicsDevice;
        private Wizard _wizard;
        public SafeList<Enemy> Enemies;
        public SafeList<Projectile> Projectiles;

        public GameLevel(GraphicsDevice gd)
        {
            Game1.Instance.IsMouseVisible = true;
            BackgroundColor = Color.LightBlue;

            _graphicsDevice = gd;
            Enemies = new SafeList<Enemy>();
            Projectiles = new SafeList<Projectile>();

            int x = GameSettings.BaseWidth / 2 - 64 / 2;
            int y = GameSettings.FloorLevel - 64 * 5 + 4;
            
            _wizard = new Wizard(new Vector2(GameSettings.BaseWidth / 2, y + 28));
            Objects.Add(_wizard);

            Objects.Add(new Tower(0) { Position = new Rectangle(x, y, 64, 64) });
            y += 64;

            for (; y < GameSettings.FloorLevel; y += 64)
                Objects.Add(new Tower() { Position = new Rectangle(x, y, 64, 64) });

            for (int gx = 0; gx < GameSettings.BaseWidth; gx+= 32)
            {
                for (int gy = GameSettings.FloorLevel; gy < GameSettings.BaseHeight; gy += 32)
                {
                    Objects.Add(new Ground(
                        gy == GameSettings.FloorLevel,
                        new Rectangle(gx, gy, 32, 32)
                    ));
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.KeyPressed(Keys.Space))
                Enemies.Add(new EnemyKnight());

            if (InputManager.MousePressed())
                CreateFireball();

            base.Update(gameTime);

            foreach (var projectile in Projectiles)
            {
                projectile.Update(gameTime);

                // brute-force this for now
                foreach (var enemy in Enemies)
                {
                    if (enemy.Bounding.Overlaps(projectile.Bounding))
                        projectile.Hit(enemy);
                }

                if (projectile.Destroyed)
                    Projectiles.Remove(projectile);
            }
            foreach (var enemy in Enemies)
            {
                enemy.Update(gameTime);
                if (enemy.Dead)
                    Enemies.Remove(enemy);
            }

            Projectiles.Commit();
            Enemies.Commit();
        }

        private void CreateFireball()
        {
            var mp = InputManager.MouseState.Position.ToVector2();
            mp = ScreenToTarget(mp);
            var spawnCircle = new Circle(_wizard.Position, ProjectileCircleRadius);
            var pos = spawnCircle.Sample();
            var dir = mp - pos;
            dir.Normalize();
            var fb = new Fireball(pos, dir * 200f);
            Projectiles.Add(fb);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            var rectangle = new Rectangle(0, 0, 1, GameSettings.BaseHeight);
            var thic = false;

            for (rectangle.X = 16; rectangle.X < GameSettings.BaseWidth; rectangle.X += 16)
            {
                rectangle.Width = thic ? 2 : 1;
                spriteBatch.DrawRectangle(rectangle, Color.Gray);
                thic = !thic;
            }

            rectangle = new Rectangle(0, 0, GameSettings.BaseWidth, 1);
            thic = false;
            for (rectangle.Y = 16; rectangle.Y < GameSettings.BaseHeight; rectangle.Y += 16)
            {
                rectangle.Height = thic ? 2 : 1;
                spriteBatch.DrawRectangle(rectangle, Color.Gray);
                thic = !thic;
            }

            base.Draw(gameTime, spriteBatch);

            foreach (var enemy in Enemies)
                enemy.Draw(gameTime, spriteBatch);
            foreach (var projectile in Projectiles)
                projectile.Draw(gameTime, spriteBatch);
        }

        public Vector2 ScreenToTarget(Vector2 v)
        {
            var x = v.X / _graphicsDevice.PresentationParameters.BackBufferWidth * GameSettings.BaseWidth;
            var y = v.Y / _graphicsDevice.PresentationParameters.BackBufferHeight * GameSettings.BaseHeight;
            return new Vector2(x, y);
        }
    }
}