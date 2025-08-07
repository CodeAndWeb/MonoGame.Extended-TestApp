using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Graphics;
using System;
using System.Diagnostics;


/// <summary>
/// Complete implementation demonstrating sprite sheet loading via Content Pipeline
/// </summary>
public class TestApp : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Sprite sheet assets
    private Texture2DAtlas _spriteAtlas;
    private SpriteSheet _characterSpriteSheet;
    private Sprite _capguy, _capguyRotated;
    private AnimatedSprite _walkAnimation;
    private AnimatedSprite _rotatingArrowAnimation;

    // NinePatch assets
    private NinePatch _leftRightTopNinePatch;

    // Display properties
    private float _angle = 0f;
    private int _walkPositionX = 0;

    public TestApp()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set window properties
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 800;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load sprite atlas from Content Pipeline
        _spriteAtlas = Content.Load<Texture2DAtlas>("spritesheet");

        _capguy = _spriteAtlas.CreateSprite("walk/0001");
        _capguyRotated = _spriteAtlas.CreateSprite("walk/0003");
        Debug.WriteLineIf(_capguy.TextureRegion.IsRotated, "walk/0001 shouldn't be rotated on sheet!");
        Debug.WriteLineIf(!_capguyRotated.TextureRegion.IsRotated, "walk/0003 should be rotated on sheet!");

        _characterSpriteSheet = new SpriteSheet("character", _spriteAtlas);

        _characterSpriteSheet.DefineAnimation("walk", builder =>
        {
            for (int i = 1; i <= 16; i++)
            {
                builder.AddFrame($"walk/{i:D4}", TimeSpan.FromMilliseconds(60));
            }
            builder.IsLooping(true);
        });
        _characterSpriteSheet.DefineAnimation("trim-test", builder =>
        {
            for (int i = 0; i <= 6; i++)
            {
                builder.AddFrame($"sprite-{i:D}", TimeSpan.FromMilliseconds(500));
            }
            builder.IsLooping(true);
        });

        _walkAnimation = new AnimatedSprite(_characterSpriteSheet, "walk");
        _rotatingArrowAnimation = new AnimatedSprite(_characterSpriteSheet, "trim-test");

        // Create NinePatch from left-right-top sprite
        var leftRightTopRegion = _spriteAtlas.GetRegion("left-right-top");
        _leftRightTopNinePatch = leftRightTopRegion.CreateNinePatch(25, 25, 25, 5);
        Debug.WriteLineIf(!leftRightTopRegion.IsRotated, "left-right-top should be rotated on sheet!");
    }

    protected override void Update(GameTime gameTime)
    {
        _walkAnimation.Update(gameTime);
        _walkPositionX = (_walkPositionX + 1) % 300;
        _rotatingArrowAnimation.Update(gameTime);
        _angle += (float)Math.PI / 300;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        // Draw the walking animation in the top-left area
        var scale = new Vector2(0.8f, 1.2f);
        var walkPosition = new Vector2(100 + _walkPositionX, 400);
        var clippingRect = new Rectangle(150, 0, 200, 400);
        DrawClippedSprite(_walkAnimation, walkPosition, 0, scale, clippingRect);
        _spriteBatch.DrawRectangle(clippingRect, Color.Gray);
        _spriteBatch.DrawRectangle(_walkAnimation.GetBoundingRectangle(new Transform2(walkPosition, 0, scale)), Color.Yellow);
        _spriteBatch.DrawCircle(new CircleF(walkPosition, 10), 12, Color.Red, 2);

        // rotating arrow
        var arrowPosition = new Vector2(200, 580);
        _spriteBatch.Draw(_rotatingArrowAnimation, arrowPosition, _angle);
        _spriteBatch.DrawRectangle(_rotatingArrowAnimation.GetBoundingRectangle(new Transform2(arrowPosition, _angle)), Color.Yellow);
        _spriteBatch.DrawCircle(new CircleF(arrowPosition, 10), 12, Color.Red, 2);

        // arrow with clipping
        arrowPosition = new Vector2(460, 580);
        clippingRect = new Rectangle(440, 585, 40, 60);
        DrawClippedSprite(_rotatingArrowAnimation, arrowPosition, _angle, Vector2.One, clippingRect);
        _spriteBatch.DrawRectangle(clippingRect, Color.Green);
        _spriteBatch.DrawCircle(new CircleF(arrowPosition, 10), 12, Color.Red, 2);

        // Draw the NinePatch in six configurations as requested
        DrawNinePatchVariants(_spriteBatch, _leftRightTopNinePatch, 550, 30);

        DrawFlippedSprites(_capguy, 30);
        DrawFlippedSprites(_capguyRotated, 250);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        // Clean up resources
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }

    /// <summary>
    /// Draws the ninepatch in six configurations as described in the demo task.
    /// </summary>
    private void DrawNinePatchVariants(SpriteBatch spriteBatch, NinePatch ninePatch, int baseX, int baseY)
    {
        int nativeWidth = 100;
        int nativeHeight = 100;
        int x = baseX;
        int y = baseY;

        // 1. Unscaled, no clipRect
        var destRect1 = new Rectangle(x, y, nativeWidth, nativeHeight);
        spriteBatch.Draw(ninePatch, destRect1, Color.White);
        y += 250;

        // 2. Unscaled, clipRect = bottom right quarter
        var clipRectBottomRight = new Rectangle(x + 50, y + 50, 50, 50);
        var destRect2 = new Rectangle(x, y, nativeWidth, nativeHeight);
        spriteBatch.Draw(ninePatch, destRect2, Color.White, clipRectBottomRight);
        y += 250;

        // 3. Unscaled, clipRect = a few transparent pixels at the top (top 5 pixels, full width)
        var clipRectTopTransparent = new Rectangle(x, y, nativeWidth, 5);
        var destRect3 = new Rectangle(x, y, nativeWidth, nativeHeight);
        spriteBatch.Draw(ninePatch, destRect3, Color.White, clipRectTopTransparent);
        x = baseX + 150;
        y = baseY;

        // 4. Scaled x2 (width), x3 (height), no clipRect
        var destRect4 = new Rectangle(x, y, nativeWidth * 3, nativeHeight * 2);
        spriteBatch.Draw(ninePatch, destRect4, Color.White);
        y += 250;

        // 5. Scaled x2/x3, clipRect = bottom right quarter
        var destRect5 = new Rectangle(x, y, 300, 200);
        spriteBatch.Draw(ninePatch, destRect5, Color.White, new Rectangle(x + 150, y + 100, 150, 100));
        y += 250;

        // 6. Scaled x2/x3, clipRect = top transparent pixels
        var destRect6 = new Rectangle(x, y, nativeWidth * 3, nativeHeight * 2);
        spriteBatch.Draw(ninePatch, destRect6, Color.White, new Rectangle(x, y, 300, 15));

        // Draw rectangles around the NinePatches to show their bounds
        spriteBatch.DrawRectangle(destRect1, Color.Red, 2);
        spriteBatch.DrawRectangle(destRect2, Color.Blue, 2);
        spriteBatch.DrawRectangle(destRect3, Color.Green, 2);
        spriteBatch.DrawRectangle(destRect4, Color.Pink, 2);
        spriteBatch.DrawRectangle(destRect5, Color.Orange, 2);
        spriteBatch.DrawRectangle(destRect6, Color.Purple, 2);
    }

    private void DrawClippedSprite(Sprite sprite, Vector2 pos, float angle, Vector2 scale, Rectangle clippingRect)
    {
        _spriteBatch.Draw(sprite.TextureRegion, pos, Color.White, angle, sprite.Origin, scale, SpriteEffects.None, 1, clippingRect);
    }

    private void DrawFlippedSprites(Sprite sprite, int yPos)
    {
        Vector2 scale = new Vector2(0.5f, 0.5f);
        Vector2 pos = new Vector2(1100, yPos);
        float angle = 0f;

        _spriteBatch.DrawCircle(new CircleF(pos, 5), 8, Color.Red);
        _spriteBatch.Draw(sprite.TextureRegion, pos, Color.White, angle, Vector2.Zero, scale, SpriteEffects.None, 1);

        pos.X += 100;
        _spriteBatch.DrawCircle(new CircleF(pos, 5), 8, Color.Red);
        _spriteBatch.Draw(sprite.TextureRegion, pos, Color.White, angle, Vector2.Zero, scale, SpriteEffects.FlipHorizontally, 1);

        pos.X += 100;
        _spriteBatch.DrawCircle(new CircleF(pos, 5), 8, Color.Red);
        _spriteBatch.Draw(sprite.TextureRegion, pos, Color.White, angle, Vector2.Zero, scale, SpriteEffects.FlipVertically, 1);

        pos.X += 100;
        _spriteBatch.DrawCircle(new CircleF(pos, 5), 8, Color.Red);
        _spriteBatch.Draw(sprite.TextureRegion, pos, Color.White, angle, Vector2.Zero, scale, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 1);
    }
} 
