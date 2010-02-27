using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1
{
    class GameObject
    {
        public Texture2D sprite;
        public Vector2 position;
        public float rotation;
        public Vector2 center;
        public Vector2 velocity;
        public bool alive;

        public GameObject(Texture2D aSprite)
        {
            sprite = aSprite;
            position = Vector2.Zero;
            velocity = Vector2.Zero;
            rotation = 0f;
            center = new Vector2(sprite.Width / 2f, sprite.Height / 2f);
            alive = false;
        }

    }
}
